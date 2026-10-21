#if UNITY_EDITOR || DEVELOPMENT_BUILD
using DTech.UtilityAI.Diagnostics;
#endif

namespace DTech.UtilityAI.Tests.Performance
{
	internal static class PerfDiagnostics
	{
		public static void ClearBrainRegistry()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			UtilityBrainDebugRegistry.Clear();
#endif
		}
	}
}
