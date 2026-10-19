using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI
{
	public abstract class UtilityBrain<TConfig, TContext> : IUtilityBrain
		where TConfig : BrainConfig
		where TContext : IUtilityContext
	{
		public event Action OnActionChanged;
		public event Action<UtilityActionState> OnActionStateChange;
		
		private readonly TContext _context;
		private readonly IBrainUpdateProvider _updateProvider;
		private readonly IUtilityAction[] _actions;
		
		public bool IsEnabled { get; private set; }
		public BrainUpdateType UpdateType { get; }
		public IReadOnlyList<IUtilityAction> Actions => _actions;
		
		protected TConfig Config { get; }
		protected TContext Context => _context;
		
		private bool _isDisposed;
		private IUtilityAction _currentAction;
		private bool _hasCurrentAction;
		private UtilityActionState _lastActionState;
		private bool _isRequestingSwitchAction;
		private BrainSwitchStateRequest _request;

		public UtilityBrain(TConfig config, TContext context, IUtilityActionInjector injector = null)
		{
			_context = context;
			Config = (TConfig)config.CreateRuntimeInstance();
			UpdateType = Config.UpdateType;
			_updateProvider = UpdateUtility.GetProvider(UpdateType);
			_actions = CreateActions(Config, context, injector ?? new NullUtilityActionInjector());
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

		private void SetCurrentAction(IUtilityAction action)
		{
			if (action == _currentAction)
			{
				return;
			}
			
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
			
			int actionIndex = scoreAggregator.Aggregate(_actions, _context);
			IUtilityAction action = actionIndex >= 0 && actionIndex < _actions.Length ? _actions[actionIndex] : null;
			SetCurrentAction(action);
		}
	}
}
