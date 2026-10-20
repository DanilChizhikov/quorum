using System;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using DTech.UtilityAI.Diagnostics;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI
{
	public abstract class UtilityBrain<TConfig, TContext> : IUtilityBrain
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		, IUtilityBrainDebugInfo
#endif
		where TConfig : BrainConfig
		where TContext : IUtilityContext
	{
		public event Action OnActionChanged;
		public event Action<UtilityActionState> OnActionStateChange;
		
		private readonly TContext _context;
		private readonly IBrainUpdateProvider _updateProvider;
		private readonly IUtilityAction[] _actions;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private readonly TConfig _sourceConfig;
		private readonly UtilityBrainSwitchLog _switchLog = new();
#endif
		
		public bool IsEnabled { get; private set; }
		public BrainUpdateType UpdateType { get; }
		public IReadOnlyList<IUtilityAction> Actions => _actions;
		
		protected TConfig Config { get; }
		protected TContext Context => _context;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		BrainConfig IUtilityBrainDebugInfo.SourceConfig => _sourceConfig;
		string IUtilityBrainDebugInfo.ConfigName => _sourceConfig != null ? _sourceConfig.name : string.Empty;
		BrainUpdateType IUtilityBrainDebugInfo.UpdateType => UpdateType;
		SwitchActionType IUtilityBrainDebugInfo.SwitchActionType => Config != null ? Config.SwitchActionType : default;
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			_sourceConfig = config;
#endif
			_context = context;
			Config = (TConfig)config.CreateRuntimeInstance();
			UpdateType = Config.UpdateType;
			_updateProvider = UpdateUtility.GetProvider(UpdateType);
			_actions = CreateActions(Config, context, injector ?? new NullUtilityActionInjector());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UtilityBrainDebugRegistry.Register(this);
#endif
		}
		
		public void Enable()
		{
			if (IsEnabled)
			{
				return;
			}

			_currentAction = null;
			_hasCurrentAction = false;
			_lastActionState = UtilityActionState.Running;
			_updateProvider.AddUpdate(Update);
			IsEnabled = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UtilityProfilerCounters.BrainEnabled();
#endif
			RequestSwitchAction();
		}

		public void RequestSwitchAction(float delay = 0f, CancellationToken token = default)
		{
			if (!IsEnabled || _isRequestingSwitchAction)
			{
				return;
			}

			_isRequestingSwitchAction = true;
			if (delay > 0f)
			{
				_request = new BrainSwitchStateRequest(delay, OnRequestSwitchActionCompleted, token);
			}
			else
			{
				OnRequestSwitchActionCompleted();
			}
		}

		public void Update()
		{
			if (!IsEnabled || !_hasCurrentAction)
			{
				return;
			}

			UtilityActionState actionState = _currentAction.Tick(_context);
			if (_lastActionState != actionState)
			{
				_lastActionState = actionState;
				OnActionStateChange?.Invoke(actionState);
			}
			
			if (Config.SwitchActionType == SwitchActionType.Manual)
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
			
			_request?.Dispose();
			_request = null;
			_isRequestingSwitchAction = false;
			_updateProvider.RemoveUpdate(Update);
			SetCurrentAction(null);
			IsEnabled = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UtilityProfilerCounters.BrainDisabled();
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
			for (int i = 0; i < _actions.Length; i++)
			{
				if (_actions[i] is IDisposable disposableAction)
				{
					disposableAction.Dispose();
				}
			}
			
			DestroyRuntimeConfig(Config);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UtilityBrainDebugRegistry.Unregister(this);
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
				UtilityAction action = configActions[i];
				if (action == null)
				{
					continue;
				}
				
				injector.Inject(action);
				action.Initialize(context);
				actions.Add(action);
			}

			return actions.ToArray();
		}

		private static void DestroyRuntimeConfig(BrainConfig config)
		{
			if (config == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				Object.Destroy(config);
			}
			else
			{
				Object.DestroyImmediate(config);
			}
		}

		private void SetCurrentAction(IUtilityAction action, bool restart = false)
		{
			if (action == _currentAction && !restart)
			{
				return;
			}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			
			RecordSwitch(previousAction, _currentAction);
#endif
			
			OnActionChanged?.Invoke();
		}
		
		private void OnRequestSwitchActionCompleted()
		{
			_request?.Dispose();
			_request = null;
			_isRequestingSwitchAction = false;
			UtilityScoreAggregator scoreAggregator = Config.ScoreAggregator;
			if (scoreAggregator == null)
			{
				Debug.LogError($"[{GetType().Name}] Score aggregator is not set in config {Config.name}!");
				return;
			}
			
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		
		private void RecordSwitch(IUtilityAction fromAction, IUtilityAction toAction)
		{
			float score = float.NaN;
			if (toAction != null)
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
