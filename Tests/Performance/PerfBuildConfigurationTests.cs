using NUnit.Framework;

namespace DTech.Quorum.Tests.Performance
{
	[TestFixture]
	internal sealed class PerfBuildConfigurationTests
	{
		[Test]
		public void Report_BuildConfiguration()
		{
			bool isDevelopmentBuild;
#if DEVELOPMENT_BUILD
			isDevelopmentBuild = true;
#else
			isDevelopmentBuild = false;
#endif
			bool isEditor;
#if UNITY_EDITOR
			isEditor = true;
#else
			isEditor = false;
#endif
			bool hasBrainDiagnostics;
#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
			hasBrainDiagnostics = true;
#else
			hasBrainDiagnostics = false;
#endif

			Assert.Pass(
				$"UNITY_EDITOR={isEditor} DEVELOPMENT_BUILD={isDevelopmentBuild} " +
				$"brain diagnostics compiled in={hasBrainDiagnostics}");
		}
	}
}
