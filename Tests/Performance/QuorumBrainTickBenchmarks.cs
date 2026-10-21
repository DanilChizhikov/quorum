using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace DTech.Quorum.Tests.Performance
{
	[TestFixture]
	internal sealed class QuorumBrainTickBenchmarks
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
		public void Tick_SingleBrain_Direct()
		{
			_set = CreateEnabledSet(1);
			PerfQuorumBrain brain = _set.First;

			Measure.Method(() => brain.Update())
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(1000)
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Tick_OneThousandBrains_Direct()
		{
			_set = CreateEnabledSet(BRAIN_COUNT);

			Measure.Method(() => _set.TickAll())
				.WarmupCount(5)
				.MeasurementCount(50)
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Tick_OneThousandBrains_ThroughUpdateService()
		{
			_set = CreateEnabledSet(BRAIN_COUNT);
			bool hasDispatcher = PerfUpdateServiceScope.TryGetDispatcher(
				typeof(Update),
				out PlayerLoopSystem.UpdateFunction dispatcher);

			Assert.That(hasDispatcher, Is.True, "Update phase does not contain the utility update service");

			Measure.Method(() => dispatcher())
				.WarmupCount(5)
				.MeasurementCount(50)
				.GC()
				.Run();
		}

		private static PerfBrainSet CreateEnabledSet(int brainCount)
		{
			var setup = new PerfBrainSetup(
				brainCount,
				ACTION_COUNT,
				CONSIDERATION_COUNT,
				PerfConsiderationKind.CompositeBlackboard);

			PerfBrainSet set = PerfBrainSet.Create(setup);
			set.EnableAll();
			return set;
		}
	}
}
