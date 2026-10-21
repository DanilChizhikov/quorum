using NUnit.Framework;
using Unity.PerformanceTesting;

namespace DTech.Quorum.Tests.Performance
{
	[TestFixture]
	internal sealed class QuorumBrainEnableBenchmarks
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
		public void Enable_SingleBrain_EmptyService()
		{
			_set = CreateSet(1);

			Measure.Method(() => _set.EnableAll())
				.WarmupCount(10)
				.MeasurementCount(100)
				.CleanUp(() => _set.DisableAll())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Enable_OneThousandBrains()
		{
			_set = CreateSet(BRAIN_COUNT);

			Measure.Method(() => _set.EnableAll())
				.WarmupCount(2)
				.MeasurementCount(10)
				.CleanUp(() => _set.DisableAll())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Disable_OneThousandBrains()
		{
			_set = CreateSet(BRAIN_COUNT);

			Measure.Method(() => _set.DisableAll())
				.WarmupCount(2)
				.MeasurementCount(10)
				.SetUp(() => _set.EnableAll())
				.GC()
				.Run();
		}

		[Test, Performance]
		public void EnableDisable_Cycle_SingleBrain()
		{
			_set = CreateSet(1);
			PerfQuorumBrain brain = _set.First;

			Measure.Method(() =>
				{
					brain.Enable();
					brain.Disable();
				})
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(100)
				.GC()
				.Run();
		}

		private static PerfBrainSet CreateSet(int brainCount)
		{
			var setup = new PerfBrainSetup(
				brainCount,
				ACTION_COUNT,
				CONSIDERATION_COUNT,
				PerfConsiderationKind.Constant);

			return PerfBrainSet.Create(setup);
		}
	}
}
