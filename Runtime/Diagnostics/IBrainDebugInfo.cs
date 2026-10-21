#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
using System.Collections.Generic;

namespace DTech.Quorum.Diagnostics
{
	internal interface IBrainDebugInfo
	{
		int DebugToken { get; }
		BrainConfig SourceConfig { get; }
		string ConfigName { get; }
		BrainUpdateType UpdateType { get; }
		SwitchActionType SwitchActionType { get; }
		bool IsEnabled { get; }
		IQuorumContext Context { get; }
		IReadOnlyList<IQuorumAction> Actions { get; }
		IQuorumAction CurrentAction { get; }
		QuorumActionState CurrentState { get; }
		IReadOnlyList<BrainSwitchRecord> SwitchLog { get; }
	}
}
#endif
