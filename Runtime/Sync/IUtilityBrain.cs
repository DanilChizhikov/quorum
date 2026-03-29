using System;
using System.Collections.Generic;
using System.Threading;

namespace DTech.UtilityAI
{
	public interface IUtilityBrain : IDisposable
	{
		event Action OnActionChanged; 
		event Action<UtilityActionState> OnActionStateChange; 
		
		bool IsEnabled { get; }
		BrainUpdateType UpdateType { get; }
		IReadOnlyList<IUtilityAction> Actions { get; }
		
		void Enable();
		void RequestSwitchAction(float delay = 0f, CancellationToken token = default);
		void Update();
		TAction GetAction<TAction>() where TAction : IUtilityAction;
		bool TryGetAction<TAction>(out TAction action) where TAction : IUtilityAction;
		bool TryGetCurrentAction(out IUtilityAction action);
		void Disable();
	}
}