using System;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
using DTech.UtilityAI.Diagnostics;
#endif
using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class UtilityBrain<TConfig, TContext> : IUtilityBrain
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
		, IUtilityBrainDebugInfo
#endif
		where TConfig : BrainConfig
		where TContext : class, IUtilityContext
	{
		public event Action OnActionChanged;
		public event Action<UtilityActionState> OnActionStateChange;
		
		private readonly TContext _context;
		private readonly IBrainUpdateProvider _updateProvider;
		private readonly IUtilityAction[] _actions;
		private readonly Action _cachedUpdate;
		private readonly SwitchActionType _switchActionType;
		private readonly UtilityScoreAggregator _scoreAggregator;
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
		private readonly UtilityBrainSwitchLog _switchLog = new();
		private readonly int _debugToken;
#endif
		
		public bool IsEnabled { get; private set; }
		public BrainUpdateType UpdateType { get; }
		public IReadOnlyList<IUtilityAction> Actions => _actions;
		
		protected TConfig Config { get; }
		protected TContext Context => _context;
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
		BrainConfig IUtilityBrainDebugInfo.SourceConfig => Config;
		string IUtilityBrainDebugInfo.ConfigName => Config != null ? Config.name : string.Empty;
		BrainUpdateType IUtilityBrainDebugInfo.UpdateType => UpdateType;
		SwitchActionType IUtilityBrainDebugInfo.SwitchActionType => _switchActionType;
		bool IUtilityBrainDebugInfo.IsEnabled => IsEnabled;
		IUtilityContext IUtilityBrainDebugInfo.Context => _context;
		IReadOnlyList<IUtilityAction> IUtilityBrainDebugInfo.Actions => _actions;
		IUtilityAction IUtilityBrainDebugInfo.CurrentAction => _hasCurrentAction ? _currentAction : null;
		UtilityActionState IUtilityBrainDebugInfo.CurrentState => _lastActionState;
		IReadOnlyList<UtilityBrainSwitchRecord> IUtilityBrainDebugInfo.SwitchLog => _switchLog;
#endif
		
		private bool _isDisposed;
		private IUtilityAction _currentAction;
		private bool _hasCurrentAction;
		private UtilityActionState _lastActionState;
		private bool _isRequestingSwitchAction;
		private BrainSwitchStateRequest _request;

		public UtilityBrain(TConfig config, TContext context, IUtilityActionInjector injector = null)
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			_context = context;
			_cachedUpdate = Update;
			Config = config;
			UpdateType = config.UpdateType;
			_switchActionType = config.SwitchActionType;
			_scoreAggregator = config.ScoreAggregator?.Clone();
			_updateProvider = UpdateUtility.GetProvider(UpdateType);
			_actions = CreateActions(config, context, injector ?? new NullUtilityActionInjector());
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			_debugToken = UtilityBrainDebugRegistry.Register(this);
#endif
		}
		
		public void Enable()
		{
			if (_isDisposed || IsEnabled)
			{
				return;
			}

			_currentAction = null;
			_hasCurrentAction = false;
			_lastActionState = UtilityActionState.Running;
			_updateProvider.AddUpdate(_cachedUpdate);
			IsEnabled = true;
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			UtilityProfilerCounters.SyncActiveBrains();
#endif
			RequestSwitchAction();
		}

		public void RequestSwitchAction(float delay = 0f, CancellationToken token = default)
		{
			if (_isDisposed || !IsEnabled)
			{
				return;
			}

			if (delay > 0f)
			{
				if (_isRequestingSwitchAction)
				{
					return;
				}

				_isRequestingSwitchAction = true;
				_request ??= new BrainSwitchStateRequest(OnRequestSwitchActionCompleted, OnRequestSwitchActionCancelled);
				_request.Arm(delay, token);
				return;
			}

			// An immediate request supersedes a pending delayed one, otherwise a brain that finished
			// its action while a delay was armed would stay in that final state until the delay ran out.
			_request?.Cancel();
			_isRequestingSwitchAction = true;
			OnRequestSwitchActionCompleted();
		}

		public void Update()
		{
			if (!IsEnabled)
			{
				return;
			}

			_request?.Tick();
			if (!_hasCurrentAction)
			{
				return;
			}

			UtilityActionState actionState = _currentAction.Tick(_context);
			if (_lastActionState != actionState)
			{
				_lastActionState = actionState;
				OnActionStateChange?.Invoke(actionState);
			}
			
			if (_switchActionType == SwitchActionType.OnActionFinished)
			{
				switch (actionState)
				{
					case UtilityActionState.Completed:
					case UtilityActionState.Failed:
					{
						RequestSwitchAction();
					} break;
				}
			}
		}

		public TAction GetAction<TAction>()
			where TAction : IUtilityAction
		{
			TAction action = TryGetAction(out TAction cachedAction) ? cachedAction : default;
			return action;
		}

		public bool TryGetAction<TAction>(out TAction action)
			where TAction : IUtilityAction
		{
			action = default;
			for (int i = 0; i < _actions.Length; i++)
			{
				if (_actions[i] is not TAction typedAction)
				{
					continue;
				}
				
				action = typedAction;
				return true;
			}

			return false;
		}

		public bool TryGetCurrentAction(out IUtilityAction action)
		{
			action = _currentAction;
			return _hasCurrentAction;
		}

		public void Disable()
		{
			if (!IsEnabled)
			{
				return;
			}
			
			_request?.Cancel();
			_isRequestingSwitchAction = false;
			_updateProvider.RemoveUpdate(_cachedUpdate);
			SetCurrentAction(null);
			IsEnabled = false;
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			UtilityProfilerCounters.SyncActiveBrains();
#endif
		}
		
		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}
			
			_isDisposed = true;
			Disable();
			OnActionChanged = null;
			OnActionStateChange = null;
			for (int i = 0; i < _actions.Length; i++)
			{
				if (_actions[i] is IDisposable disposableAction)
				{
					disposableAction.Dispose();
				}
			}
			
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			UtilityBrainDebugRegistry.Unregister(_debugToken);
#endif
		}

		private static IUtilityAction[] CreateActions(BrainConfig config, IUtilityContext context, IUtilityActionInjector injector)
		{
			IReadOnlyList<UtilityAction> configActions = config.Actions;
			if (configActions == null)
			{
				return Array.Empty<IUtilityAction>();
			}

			var actions = new List<IUtilityAction>(configActions.Count);
			for (int i = 0; i < configActions.Count; i++)
			{
				UtilityAction configAction = configActions[i];
				if (configAction == null)
				{
					continue;
				}

				UtilityAction action = configAction.Clone();
				injector.Inject(action);
				action.Initialize(context);
				action.InitializeConsideration(context);
				actions.Add(action);
			}

			return actions.ToArray();
		}

		private void SetCurrentAction(IUtilityAction action, bool restart = false)
		{
			if (action == _currentAction && !restart)
			{
				return;
			}
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			
			IUtilityAction previousAction = _hasCurrentAction ? _currentAction : null;
#endif
			
			if (_hasCurrentAction)
			{
				_currentAction.Exit(_context);
			}
			
			_currentAction = action;
			_hasCurrentAction = _currentAction != null;
			_lastActionState = UtilityActionState.Running;
			if (_hasCurrentAction)
			{
				_currentAction.Enter(_context);
			}
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			
			RecordSwitch(previousAction, _currentAction);
#endif
			
			OnActionChanged?.Invoke();
		}
		
		private void OnRequestSwitchActionCancelled()
		{
			_isRequestingSwitchAction = false;
		}

		private void OnRequestSwitchActionCompleted()
		{
			_request?.Cancel();
			_isRequestingSwitchAction = false;
			UtilityScoreAggregator scoreAggregator = _scoreAggregator;
			if (scoreAggregator == null)
			{
				Debug.LogError($"[{GetType().Name}] Score aggregator is not set in config {Config.name}!");
				return;
			}
			
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			int actionIndex;
			using (UtilityProfilerCounters.SelectActionMarker.Auto())
			{
				actionIndex = scoreAggregator.Aggregate(_actions, _context);
			}
#else
			int actionIndex = scoreAggregator.Aggregate(_actions, _context);
#endif
			IUtilityAction action = actionIndex >= 0 && actionIndex < _actions.Length ? _actions[actionIndex] : null;
			// A finished action that wins again must start over, otherwise it keeps returning its final state.
			bool restart = action != null && action == _currentAction && _lastActionState != UtilityActionState.Running;
			SetCurrentAction(action, restart);
		}
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
		
		private void RecordSwitch(IUtilityAction fromAction, IUtilityAction toAction)
		{
			float score = float.NaN;
			if (toAction != null && UtilityBrainDebugRegistry.IsObserved)
			{
				try
				{
					score = toAction.CalculateUtility(_context);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			
			var record = new UtilityBrainSwitchRecord(
				Time.realtimeSinceStartupAsDouble,
				Array.IndexOf(_actions, fromAction),
				Array.IndexOf(_actions, toAction),
				score);
			
			_switchLog.Add(record);
			UtilityProfilerCounters.ActionSwitched();
		}
#endif
	}
}
