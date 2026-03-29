using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public sealed class UtilityExecutor
	{
		private readonly UtilityBrain _brain;
		private bool _isInitialized;
		private IUtilityAction _currentAction;

		public UtilityExecutor(UtilityBrain brain)
		{
			_brain = brain ?? throw new ArgumentNullException(nameof(brain));
			State = UtilityActionState.Idle;
		}

		public UtilityActionState State { get; private set; }
		public IUtilityAction CurrentAction => _currentAction;

		public bool Initialize(IUtilityContext context)
		{
			if (_isInitialized)
			{
				return true;
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			try
			{
				IReadOnlyList<UtilityActionDefinition> definitions = _brain.ActionDefinitions;
				for (int i = 0; i < definitions.Count; i++)
				{
					definitions[i].Action.Initialize(context);
				}
			}
			catch
			{
				State = UtilityActionState.Failed;
				return false;
			}

			_isInitialized = true;
			State = UtilityActionState.Idle;
			return true;
		}

		public UtilityActionState Tick(IUtilityContext context)
		{
			if (!Initialize(context))
			{
				return State;
			}

			UtilityDecision<IUtilityAction> decision;
			try
			{
				decision = _brain.Decide(context);
			}
			catch
			{
				State = UtilityActionState.Failed;
				return State;
			}

			if (!decision.HasDecision)
			{
				CancelAndExitCurrent(context);
				if (State != UtilityActionState.Failed && State != UtilityActionState.Cancelled)
				{
					State = UtilityActionState.Idle;
				}
				return State;
			}

			if (!ReferenceEquals(_currentAction, decision.Action) && !SwitchToAction(decision.Action, context))
			{
				return State;
			}

			return TickCurrentAction(context);
		}

		private bool SwitchToAction(IUtilityAction nextAction, IUtilityContext context)
		{
			CancelAndExitCurrent(context);
			if (State == UtilityActionState.Failed)
			{
				return false;
			}

			try
			{
				nextAction.Enter(context);
			}
			catch
			{
				State = UtilityActionState.Failed;
				_currentAction = null;
				return false;
			}

			_currentAction = nextAction;
			State = UtilityActionState.Running;
			return true;
		}

		private UtilityActionState TickCurrentAction(IUtilityContext context)
		{
			if (_currentAction == null)
			{
				State = UtilityActionState.Idle;
				return State;
			}

			UtilityActionState actionState;
			try
			{
				actionState = _currentAction.Tick(context);
			}
			catch
			{
				State = UtilityActionState.Failed;
				ExitCurrent(context);
				return State;
			}

			if (actionState == UtilityActionState.Running)
			{
				State = UtilityActionState.Running;
				return State;
			}

			State = actionState;
			ExitCurrent(context);
			return State;
		}

		private void CancelAndExitCurrent(IUtilityContext context)
		{
			if (_currentAction == null)
			{
				return;
			}

			IUtilityAction action = _currentAction;
			try
			{
				action.Cancel(context);
			}
			catch
			{
				State = UtilityActionState.Failed;
			}

			try
			{
				action.Exit(context);
			}
			catch
			{
				State = UtilityActionState.Failed;
			}

			_currentAction = null;
		}

		private void ExitCurrent(IUtilityContext context)
		{
			if (_currentAction == null)
			{
				return;
			}

			IUtilityAction action = _currentAction;
			try
			{
				action.Exit(context);
			}
			catch
			{
				State = UtilityActionState.Failed;
			}

			_currentAction = null;
		}
	}
}
