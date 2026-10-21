using DTech.Blackboard;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace DTech.Quorum.Tests.Performance
{
	[TestFixture]
	internal sealed class BlackboardLookupBenchmarks
	{
		private RuntimeBlackboard _blackboard;

		[SetUp]
		public void SetUp()
		{
			_blackboard = PerfBrainSet.CreateBlackboard();
		}

		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
			_blackboard = null;
		}

		[Test, Performance]
		public void TryGetVariable_Generic_ByName()
		{
			RuntimeBlackboard blackboard = _blackboard;

			Measure.Method(() => blackboard.TryGetVariable(PerfBrainSet.HungerKey, out BlackboardVariable<float> _))
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(1000)
				.GC()
				.Run();
		}

		[Test, Performance]
		public void TryGetVariable_NonGeneric_ByName()
		{
			RuntimeBlackboard blackboard = _blackboard;

			Measure.Method(() => blackboard.TryGetVariable(PerfBrainSet.HungerKey, out BlackboardVariable _))
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(1000)
				.GC()
				.Run();
		}
	}
}
