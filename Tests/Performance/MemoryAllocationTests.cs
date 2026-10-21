using System;
using NUnit.Framework;

namespace DTech.UtilityAI.Tests.Performance
{
	[TestFixture]
	internal sealed class MemoryAllocationTests
	{
		private const int BRAIN_COUNT = 1000;
		private const int ACTION_COUNT = 6;
		private const int CONSIDERATION_COUNT = 3;
		private const int WarmupIterations = 10;
		private const int MeasurementIterations = 100;
		private const long NoAllocationsBudget = 0L;

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

		[Test]
		public void Tick_SteadyState_OneThousandBrains_NoAllocations()
		{
			if (!IsAllocationTrackingSupported())
			{
				Assert.Ignore("GC.GetAllocatedBytesForCurrentThread is not supported on this runtime.");
			}

			_set = CreateSet(BRAIN_COUNT, PerfConsiderationKind.CompositeBlackboard);
			_set.EnableAll();
			Action tick = _set.TickAll;
			Warmup(tick);

			long bytesPerCall = MeasureBytesPerCall(tick, MeasurementIterations);

			AssertWithinBudget(
				bytesPerCall,
				NoAllocationsBudget,
				nameof(Tick_SteadyState_OneThousandBrains_NoAllocations));
		}

		[Test]
		public void EnableDisable_Cycle_SingleBrain_NoAllocations()
		{
			if (!IsAllocationTrackingSupported())
			{
				Assert.Ignore("GC.GetAllocatedBytesForCurrentThread is not supported on this runtime.");
			}

			_set = CreateSet(1, PerfConsiderationKind.Constant);
			PerfUtilityBrain brain = _set.First;
			Action cycle = () =>
			{
				brain.Enable();
				brain.Disable();
			};

			Warmup(cycle);

			long bytesPerCall = MeasureBytesPerCall(cycle, MeasurementIterations);

			AssertWithinBudget(
				bytesPerCall,
				NoAllocationsBudget,
				nameof(EnableDisable_Cycle_SingleBrain_NoAllocations));
		}

		[Test]
		public void RequestSwitchAction_Immediate_NoAllocations()
		{
			if (!IsAllocationTrackingSupported())
			{
				Assert.Ignore("GC.GetAllocatedBytesForCurrentThread is not supported on this runtime.");
			}

			_set = CreateSet(1, PerfConsiderationKind.Constant);
			_set.EnableAll();
			PerfUtilityBrain brain = _set.First;
			Action request = () => brain.RequestSwitchAction();
			Warmup(request);

			long bytesPerCall = MeasureBytesPerCall(request, MeasurementIterations);

			AssertWithinBudget(
				bytesPerCall,
				NoAllocationsBudget,
				nameof(RequestSwitchAction_Immediate_NoAllocations));
		}

		[Test]
		public void Score_SixActions_CompositeBlackboardCached_NoAllocations()
		{
			if (!IsAllocationTrackingSupported())
			{
				Assert.Ignore("GC.GetAllocatedBytesForCurrentThread is not supported on this runtime.");
			}

			_set = CreateSet(1, PerfConsiderationKind.CompositeBlackboardCached);
			_set.EnableAll();
			PerfUtilityBrain brain = _set.First;
			Action request = () => brain.RequestSwitchAction();
			Warmup(request);

			long bytesPerCall = MeasureBytesPerCall(request, MeasurementIterations);

			AssertWithinBudget(
				bytesPerCall,
				NoAllocationsBudget,
				nameof(Score_SixActions_CompositeBlackboardCached_NoAllocations));
		}

		private static PerfBrainSet CreateSet(int brainCount, PerfConsiderationKind considerationKind)
		{
			var setup = new PerfBrainSetup(brainCount, ACTION_COUNT, CONSIDERATION_COUNT, considerationKind);
			return PerfBrainSet.Create(setup);
		}

		private static bool IsAllocationTrackingSupported()
		{
			try
			{
				GC.GetAllocatedBytesForCurrentThread();
				return true;
			}
			catch (NotImplementedException)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}
		}

		private static void Warmup(Action action)
		{
			for (int i = 0; i < WarmupIterations; i++)
			{
				action();
			}
		}

		private static long MeasureBytesPerCall(Action action, int iterations)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			long before = GC.GetAllocatedBytesForCurrentThread();
			for (int i = 0; i < iterations; i++)
			{
				action();
			}

			long after = GC.GetAllocatedBytesForCurrentThread();
			return (after - before) / iterations;
		}

		private static void AssertWithinBudget(long actualBytesPerCall, long budget, string testName)
		{
			Assert.That(
				actualBytesPerCall,
				Is.LessThanOrEqualTo(budget),
				$"{testName}: {actualBytesPerCall} bytes/call exceeds budget of {budget} bytes/call. " +
				"Investigate recent changes to the brain tick hot path.");
		}
	}
}
