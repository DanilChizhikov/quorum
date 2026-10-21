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

			var setup = new PerfBrainSetup(
				BRAIN_COUNT,
				ACTION_COUNT,
				CONSIDERATION_COUNT,
				PerfConsiderationKind.CompositeBlackboard);

			_set = PerfBrainSet.Create(setup);
			_set.EnableAll();
			Action tick = _set.TickAll;
			Warmup(tick);

			long bytesPerCall = MeasureBytesPerCall(tick, MeasurementIterations);

			AssertWithinBudget(
				bytesPerCall,
				NoAllocationsBudget,
				nameof(Tick_SteadyState_OneThousandBrains_NoAllocations));
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
