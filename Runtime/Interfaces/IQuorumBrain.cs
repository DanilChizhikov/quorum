using System;
using System.Collections.Generic;
using System.Threading;

namespace DTech.Quorum
{
	public interface IQuorumBrain : IDisposable
	{
		event Action OnActionChanged; 
		event Action<QuorumActionState> OnActionStateChange; 
		
		bool IsEnabled { get; }
		BrainUpdateType UpdateType { get; }
		IReadOnlyList<IQuorumAction> Actions { get; }
		
		void Enable();
		void RequestSwitchAction(float delay = 0f, CancellationToken token = default);
		void Update();
		TAction GetAction<TAction>() where TAction : IQuorumAction;
		bool TryGetAction<TAction>(out TAction action) where TAction : IQuorumAction;
		bool TryGetCurrentAction(out IQuorumAction action);
		void Disable();
	}
}