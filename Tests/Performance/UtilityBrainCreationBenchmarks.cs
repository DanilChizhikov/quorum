using NUnit.Framework;
using Unity.PerformanceTesting;

namespace DTech.UtilityAI.Tests.Performance
{
	[TestFixture]
	internal sealed class UtilityBrainCreationBenchmarks
	{
		private const int SPAWN_COUNT = 200;
		private const int ACTION_COUNT = 6;
		private const int CONSIDERATION_COUNT = 3;

		private PerfBrainSet _set;

		[TearDown]
		public void TearDown()
		{
			_set?.Dispose();
			_set = null;
			PerfDiagnostics.ClearBrainRegistry();
		}

		[Test, Performance]
		public void Create_SingleBrain_SixActions()
		{
			_set = CreateSet(1, PerfActionKind.Inline);

			Measure.Method(() => _set.BuildBrains())
				.WarmupCount(10)
				.MeasurementCount(50)
				.CleanUp(() => _set.DisposeBrains())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Create_SingleBrain_SixScriptableActionReferences()
		{
			_set = CreateSet(1, PerfActionKind.ScriptableReference);

			Measure.Method(() => _set.BuildBrains())
				.WarmupCount(10)
				.MeasurementCount(50)
				.CleanUp(() => _set.DisposeBrains())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Create_TwoHundredBrains_SixActions()
		{
			_set = CreateSet(SPAWN_COUNT, PerfActionKind.Inline);

			Measure.Method(() => _set.BuildBrains())
				.WarmupCount(2)
				.MeasurementCount(10)
				.CleanUp(() => _set.DisposeBrains())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Create_TwoHundredBrains_SixScriptableActionReferences()
		{
			_set = CreateSet(SPAWN_COUNT, PerfActionKind.ScriptableReference);

			Measure.Method(() => _set.BuildBrains())
				.WarmupCount(2)
				.MeasurementCount(10)
				.CleanUp(() => _set.DisposeBrains())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Dispose_TwoHundredBrains()
		{
			_set = CreateSet(SPAWN_COUNT, PerfActionKind.Inline);

			Measure.Method(() => _set.DisposeBrains())
				.WarmupCount(2)
				.MeasurementCount(10)
				.SetUp(() => _set.BuildBrains())
				.GC()
				.Run();
		}

		private static PerfBrainSet CreateSet(int brainCount, PerfActionKind actionKind)
		{
			var setup = new PerfBrainSetup(
				brainCount,
				ACTION_COUNT,
				CONSIDERATION_COUNT,
				PerfConsiderationKind.CompositeConstants,
				actionKind);

			return PerfBrainSet.CreateWithoutBrains(setup);
		}
	}
}
