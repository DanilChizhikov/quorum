using System;
using System.Collections.Generic;
using System.Threading;

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
		
		public bool IsEnabled { get; private set; }
		public BrainUpdateType UpdateType { get; }
		public IReadOnlyList<IUtilityAction> Actions => Config.Actions;
		
		protected TConfig Config { get; }
		
		private bool _isDisposed;
		private IUtilityAction _currentAction;
		private bool _hasCurrentAction;
		private UtilityActionState _lastActionState;
		private bool _isRequestingSwitchAction;
		private BrainSwitchStateRequest _request;

		public UtilityBrain(TConfig config)
		{
			Config = config;
			UpdateType = config.UpdateType;
			_updateProvider = UpdateUtility.GetProvider(config.UpdateType);
		}
		
		public void Enable()
		{
			if (IsEnabled)
			{
				return;
			}

			_currentAction = null;
			_hasCurrentAction = false;
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
			_request = new BrainSwitchStateRequest(delay, OnRequestSwitchActionCompleted, token);
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
			for (int i = 0; i < Actions.Count; i++)
			{
				IUtilityAction utilityAction = Actions[i];
				if (utilityAction.GetType().IsAssignableFrom(typeof(TAction)))
				{
					action = (TAction)utilityAction;
					return true;
				}
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
		}
		
		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}
			
			Disable();
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
			if (_hasCurrentAction)
			{
				_currentAction.Enter(_context);
			}
			
			OnActionChanged?.Invoke();
		}
		
		private void OnRequestSwitchActionCompleted()
		{
			_request.Dispose();
			_request = null;
			int actionIndex = Config.ScoreAggregator.Aggregate(Actions, _context);
			IUtilityAction action = actionIndex >= 0 ? Actions[actionIndex] : null;
			SetCurrentAction(action);
			_isRequestingSwitchAction = false;
		}
	}
}
