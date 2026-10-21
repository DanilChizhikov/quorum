using System;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
using DTech.Quorum.Diagnostics;
#endif
using UnityEngine;

namespace DTech.Quorum
{
	public abstract class QuorumBrain<TConfig, TContext> : IQuorumBrain
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
		, IBrainDebugInfo
#endif
		where TConfig : BrainConfig
		where TContext : class, IQuorumContext
	{
		public event Action OnActionChanged;
		public event Action<QuorumActionState> OnActionStateChange;
		
		private readonly TContext _context;
		private readonly IBrainUpdateProvider _updateProvider;
		private readonly IQuorumAction[] _actions;
		private readonly Action _cachedUpdate;
		private readonly SwitchActionType _switchActionType;
		private readonly ScoreAggregator _scoreAggregator;
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
		private readonly BrainSwitchLog _switchLog = new();
		private readonly int _debugToken;
#endif
		
		public bool IsEnabled { get; private set; }
		public BrainUpdateType UpdateType { get; }
		public IReadOnlyList<IQuorumAction> Actions => _actions;
		
		protected TConfig Config { get; }
		protected TContext Context => _context;
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
		int IBrainDebugInfo.DebugToken => _debugToken;
		BrainConfig IBrainDebugInfo.SourceConfig => Config;
		string IBrainDebugInfo.ConfigName => Config != null ? Config.name : string.Empty;
		BrainUpdateType IBrainDebugInfo.UpdateType => UpdateType;
		SwitchActionType IBrainDebugInfo.SwitchActionType => _switchActionType;
		bool IBrainDebugInfo.IsEnabled => IsEnabled;
		IQuorumContext IBrainDebugInfo.Context => _context;
		IReadOnlyList<IQuorumAction> IBrainDebugInfo.Actions => _actions;
		IQuorumAction IBrainDebugInfo.CurrentAction => _hasCurrentAction ? _currentAction : null;
		QuorumActionState IBrainDebugInfo.CurrentState => _lastActionState;
		IReadOnlyList<BrainSwitchRecord> IBrainDebugInfo.SwitchLog => _switchLog;
#endif
		
		private bool _isDisposed;
		private IQuorumAction _currentAction;
		private bool _hasCurrentAction;
		private QuorumActionState _lastActionState;
		private bool _isRequestingSwitchAction;
		private BrainSwitchStateRequest _request;

		public QuorumBrain(TConfig config, TContext context, IQuorumActionInjector injector = null)
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
			_actions = CreateActions(config, context, injector ?? new NullActionInjector());
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			_debugToken = BrainDebugRegistry.Register(this);
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
			_lastActionState = QuorumActionState.Running;
			_updateProvider.AddUpdate(_cachedUpdate);
			IsEnabled = true;
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			QuorumProfilerCounters.BrainEnabled();
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

			QuorumActionState actionState = _currentAction.Tick(_context);
			if (_lastActionState != actionState)
			{
				_lastActionState = actionState;
				OnActionStateChange?.Invoke(actionState);
			}
			
			if (_switchActionType == SwitchActionType.OnActionFinished)
			{
				switch (actionState)
				{
					case QuorumActionState.Completed:
					case QuorumActionState.Failed:
					{
						RequestSwitchAction();
					} break;
				}
			}
		}

		public TAction GetAction<TAction>()
			where TAction : IQuorumAction
		{
			TAction action = TryGetAction(out TAction cachedAction) ? cachedAction : default;
			return action;
		}

		public bool TryGetAction<TAction>(out TAction action)
			where TAction : IQuorumAction
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

		public bool TryGetCurrentAction(out IQuorumAction action)
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
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			QuorumProfilerCounters.BrainDisabled();
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
			
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			BrainDebugRegistry.Unregister(_debugToken);
#endif
		}

		private static IQuorumAction[] CreateActions(BrainConfig config, IQuorumContext context, IQuorumActionInjector injector)
		{
			IReadOnlyList<QuorumAction> configActions = config.Actions;
			if (configActions == null)
			{
				return Array.Empty<IQuorumAction>();
			}

			var actions = new List<IQuorumAction>(configActions.Count);
			for (int i = 0; i < configActions.Count; i++)
			{
				QuorumAction configAction = configActions[i];
				if (configAction == null)
				{
					continue;
				}

				QuorumAction action = configAction.Clone();
				injector.Inject(action);
				action.Initialize(context);
				action.InitializeConsideration(context);
				actions.Add(action);
			}

			return actions.ToArray();
		}

		private void SetCurrentAction(IQuorumAction action, bool restart = false)
		{
			if (action == _currentAction && !restart)
			{
				return;
			}
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			
			IQuorumAction previousAction = _hasCurrentAction ? _currentAction : null;
#endif
			
			if (_hasCurrentAction)
			{
				_currentAction.Exit(_context);
			}
			
			_currentAction = action;
			_hasCurrentAction = _currentAction != null;
			_lastActionState = QuorumActionState.Running;
			if (_hasCurrentAction)
			{
				_currentAction.Enter(_context);
			}
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			
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
			ScoreAggregator scoreAggregator = _scoreAggregator;
			if (scoreAggregator == null)
			{
				Debug.LogError($"[{GetType().Name}] Score aggregator is not set in config {Config.name}!");
				return;
			}
			
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			int actionIndex;
			using (QuorumProfilerCounters.SelectActionMarker.Auto())
			{
				actionIndex = scoreAggregator.Aggregate(_actions, _context);
			}
#else
			int actionIndex = scoreAggregator.Aggregate(_actions, _context);
#endif
			IQuorumAction action = actionIndex >= 0 && actionIndex < _actions.Length ? _actions[actionIndex] : null;
			bool restart = action != null && action == _currentAction && _lastActionState != QuorumActionState.Running;
			SetCurrentAction(action, restart);
		}
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
		
		private void RecordSwitch(IQuorumAction fromAction, IQuorumAction toAction)
		{
			float score = float.NaN;
			if (toAction != null && BrainDebugRegistry.IsObserved)
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
			
			var record = new BrainSwitchRecord(
				Time.realtimeSinceStartupAsDouble,
				Array.IndexOf(_actions, fromAction),
				Array.IndexOf(_actions, toAction),
				score);
			
			_switchLog.Add(record);
			QuorumProfilerCounters.ActionSwitched();
		}
#endif
	}
}
