#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
using DTech.Quorum.Diagnostics;
#endif

namespace DTech.Quorum.Tests.Performance
{
	internal static class PerfDiagnostics
	{
		public static void ClearBrainRegistry()
		{
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			BrainDebugRegistry.Clear();
#endif
		}
	}
}
