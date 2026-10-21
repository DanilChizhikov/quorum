using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace DTech.UtilityAI.Tests.Performance
{
	[TestFixture]
	internal sealed class UtilitySwitchRequestBenchmarks
	{
		private const int BRAIN_COUNT = 1000;
		private const int ACTION_COUNT = 6;
		private const int CONSIDERATION_COUNT = 3;
		private const float PENDING_DELAY = 1000f;

		private static readonly CancellationToken _cancelledToken = new(true);

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
		public void RequestSwitchAction_Immediate()
		{
			_set = CreateEnabledSet(1);
			PerfUtilityBrain brain = _set.First;

			Measure.Method(() => brain.RequestSwitchAction())
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(100)
				.GC()
				.Run();
		}

		[Test, Performance]
		public void RequestSwitchAction_Delayed_CreateAndComplete()
		{
			_set = CreateEnabledSet(1);
			PerfUtilityBrain brain = _set.First;
			PlayerLoopSystem.UpdateFunction dispatcher = GetUpdateDispatcher();

			Measure.Method(() =>
				{
					brain.RequestSwitchAction(PENDING_DELAY, _cancelledToken);
					dispatcher();
				})
				.WarmupCount(10)
				.MeasurementCount(100)
				.IterationsPerMeasurement(10)
				.GC()
				.Run();
		}

		[Test, Performance]
		public void Dispatch_OneThousandPendingRequests()
		{
			_set = CreateEnabledSet(BRAIN_COUNT);
			_set.RequestSwitchAll(PENDING_DELAY, CancellationToken.None);
			PlayerLoopSystem.UpdateFunction dispatcher = GetUpdateDispatcher();

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
				PerfConsiderationKind.Constant);

			PerfBrainSet set = PerfBrainSet.Create(setup);
			set.EnableAll();
			return set;
		}

		private static PlayerLoopSystem.UpdateFunction GetUpdateDispatcher()
		{
			bool hasDispatcher = PerfUpdateServiceScope.TryGetDispatcher(
				typeof(Update),
				out PlayerLoopSystem.UpdateFunction dispatcher);

			Assert.That(hasDispatcher, Is.True, "Update phase does not contain the utility update service");
			return dispatcher;
		}
	}
}
