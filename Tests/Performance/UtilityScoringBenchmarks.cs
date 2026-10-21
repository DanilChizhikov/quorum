using NUnit.Framework;
using Unity.PerformanceTesting;

namespace DTech.UtilityAI.Tests.Performance
{
	[TestFixture]
	internal sealed class UtilityScoringBenchmarks
	{
		private const int BRAIN_COUNT = 1000;
		private const int ACTION_COUNT = 6;
		private const int CONSIDERATION_COUNT = 3;

		private PerfUpdateServiceScope _scope;
		private PerfBrainSet _set;

		[SetUp]
		public void SetUp()
		{
			_scope = new PerfUpdateServiceScope();
		}

		[TearDown]
		public void TearDown()
		{
			_set?.Dispose();
			_set = null;
			_scope?.Dispose();
			_scope = null;
			PerfDiagnostics.ClearBrainRegistry();
		}

		[Test, Performance]
		public void Score_SixActions_NoConsideration_Max()
		{
			MeasureSingleBrainScore(PerfConsiderationKind.None);
		}

		[Test, Performance]
		public void Score_SixActions_ConstantConsideration_Max()
		{
			MeasureSingleBrainScore(PerfConsiderationKind.Constant);
		}

		[Test, Performance]
		public void Score_SixActions_CompositeConstants_Max()
		{
			MeasureSingleBrainScore(PerfConsiderationKind.CompositeConstants);
		}

		[Test, Performance]
		public void Score_SixActions_CompositeBlackboard_Max()
		{
			MeasureSingleBrainScore(PerfConsiderationKind.CompositeBlackboard);
		}

		[Test, Performance]
		public void Score_SixActions_CompositeBlackboard_Random()
		{
			_set = CreateEnabledSet(1, PerfConsiderationKind.CompositeBlackboard, new RandomUtilityScoreAggregator());
			PerfUtilityBrain brain = _set.First;

			Measure.Method(() => brain.RequestSwitchAction())
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(100)
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Score_OneThousandBrains_CompositeBlackboard_Max()
		{
			_set = CreateEnabledSet(BRAIN_COUNT, PerfConsiderationKind.CompositeBlackboard, null);

			Measure.Method(() => _set.RequestSwitchAll())
				.WarmupCount(5)
				.MeasurementCount(50)
				.GC()
				.Run();
		}

		private static PerfBrainSet CreateEnabledSet(
			int brainCount,
			PerfConsiderationKind considerationKind,
			UtilityScoreAggregator aggregator)
		{
			var setup = new PerfBrainSetup(brainCount, ACTION_COUNT, CONSIDERATION_COUNT, considerationKind);
			PerfBrainSet set = PerfBrainSet.CreateWithoutBrains(setup);
			if (aggregator != null)
			{
				set.SetScoreAggregator(aggregator);
			}

			set.BuildBrains();
			set.EnableAll();
			return set;
		}

		private void MeasureSingleBrainScore(PerfConsiderationKind considerationKind)
		{
			_set = CreateEnabledSet(1, considerationKind, null);
			PerfUtilityBrain brain = _set.First;

			Measure.Method(() => brain.RequestSwitchAction())
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(100)
				.GC()
				.Run();
		}
	}
}
