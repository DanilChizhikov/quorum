#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
using System.Collections.Generic;

namespace DTech.UtilityAI.Diagnostics
{
	internal interface IUtilityBrainDebugInfo
	{
		BrainConfig SourceConfig { get; }
		string ConfigName { get; }
		BrainUpdateType UpdateType { get; }
		SwitchActionType SwitchActionType { get; }
		bool IsEnabled { get; }
		IUtilityContext Context { get; }
		IReadOnlyList<IUtilityAction> Actions { get; }
		IUtilityAction CurrentAction { get; }
		UtilityActionState CurrentState { get; }
		IReadOnlyList<UtilityBrainSwitchRecord> SwitchLog { get; }
	}
}
#endif
