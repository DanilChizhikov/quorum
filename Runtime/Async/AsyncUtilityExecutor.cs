using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DTech.UtilityAI
{
	public sealed class AsyncUtilityExecutor
	{
		private readonly AsyncUtilityBrain _brain;
		private bool _isInitialized;
		private IAsyncUtilityAction _currentAction;

		public AsyncUtilityExecutor(AsyncUtilityBrain brain)
		{
			_brain = brain ?? throw new ArgumentNullException(nameof(brain));
			State = UtilityActionState.Idle;
		}

		public UtilityActionState State { get; private set; }
		public IAsyncUtilityAction CurrentAction => _currentAction;

		public async ValueTask<bool> InitializeAsync(IUtilityContext context, CancellationToken token)
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
				IReadOnlyList<UtilityAsyncActionDefinition> definitions = _brain.ActionDefinitions;
				for (int i = 0; i < definitions.Count; i++)
				{
					token.ThrowIfCancellationRequested();
					await definitions[i].Action.InitializeAsync(context, token);
				}
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
				return false;
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

		public async ValueTask<UtilityActionState> TickAsync(IUtilityContext context, CancellationToken token)
		{
			if (!await InitializeAsync(context, token))
			{
				return State;
			}

			UtilityDecision<IAsyncUtilityAction> decision;
			try
			{
				decision = await _brain.DecideAsync(context, token);
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
				await CancelAndExitCurrentAsync(context, token);
				return State;
			}
			catch
			{
				State = UtilityActionState.Failed;
				await ExitCurrentAsync(context, token);
				return State;
			}

			if (!decision.HasDecision)
			{
				await CancelAndExitCurrentAsync(context, token);
				if (State != UtilityActionState.Failed && State != UtilityActionState.Cancelled)
				{
					State = UtilityActionState.Idle;
				}
				return State;
			}

			if (!ReferenceEquals(_currentAction, decision.Action) && !await SwitchToActionAsync(decision.Action, context, token))
			{
				return State;
			}

			return await TickCurrentActionAsync(context, token);
		}

		private async ValueTask<bool> SwitchToActionAsync(IAsyncUtilityAction nextAction, IUtilityContext context, CancellationToken token)
		{
			await CancelAndExitCurrentAsync(context, token);
			if (State == UtilityActionState.Failed)
			{
				return false;
			}

			try
			{
				await nextAction.EnterAsync(context, token);
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
				_currentAction = null;
				return false;
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

		private async ValueTask<UtilityActionState> TickCurrentActionAsync(IUtilityContext context, CancellationToken token)
		{
			if (_currentAction == null)
			{
				State = UtilityActionState.Idle;
				return State;
			}

			UtilityActionState actionState;
			try
			{
				actionState = await _currentAction.TickAsync(context, token);
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
				await ExitCurrentAsync(context, token);
				return State;
			}
			catch
			{
				State = UtilityActionState.Failed;
				await ExitCurrentAsync(context, token);
				return State;
			}

			if (actionState == UtilityActionState.Running)
			{
				State = UtilityActionState.Running;
				return State;
			}

			State = actionState;
			await ExitCurrentAsync(context, token);
			return State;
		}

		private async ValueTask CancelAndExitCurrentAsync(IUtilityContext context, CancellationToken token)
		{
			if (_currentAction == null)
			{
				return;
			}

			IAsyncUtilityAction action = _currentAction;
			try
			{
				await action.CancelAsync(context, token);
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
			}
			catch
			{
				State = UtilityActionState.Failed;
			}

			try
			{
				await action.ExitAsync(context, token);
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
			}
			catch
			{
				State = UtilityActionState.Failed;
			}

			_currentAction = null;
		}

		private async ValueTask ExitCurrentAsync(IUtilityContext context, CancellationToken token)
		{
			if (_currentAction == null)
			{
				return;
			}

			IAsyncUtilityAction action = _currentAction;
			try
			{
				await action.ExitAsync(context, token);
			}
			catch (OperationCanceledException)
			{
				State = UtilityActionState.Cancelled;
			}
			catch
			{
				State = UtilityActionState.Failed;
			}

			_currentAction = null;
		}
	}
}
