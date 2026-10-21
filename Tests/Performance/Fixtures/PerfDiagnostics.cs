#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
using DTech.UtilityAI.Diagnostics;
#endif

namespace DTech.UtilityAI.Tests.Performance
{
	internal static class PerfDiagnostics
	{
		public static void ClearBrainRegistry()
		{
#if UNITY_EDITOR || UTILITYAI_DIAGNOSTICS
			UtilityBrainDebugRegistry.Clear();
#endif
		}
	}
}
