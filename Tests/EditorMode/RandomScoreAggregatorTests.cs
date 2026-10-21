using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class RandomScoreAggregatorTests
	{
		private RandomScoreAggregator _aggregator;
		private MockQuorumContext _context;
		
		[SetUp]
		public void Setup()
		{
			_aggregator = new RandomScoreAggregator();
			_context = new MockQuorumContext();
			Random.InitState(42);
		}
		
		[Test]
		public void Aggregate_EmptyActions_ReturnsNegativeOne()
		{
			var actions = new List<IQuorumAction>();
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(-1));
		}
		
		[Test]
		public void Aggregate_SingleAction_ReturnsZero()
		{
			var action = new MockQuorumAction(0.5f);
			var actions = new List<IQuorumAction> { action };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(0));
		}
		
		[Test]
		public void Aggregate_MultipleActions_ReturnsValidIndex()
		{
			var action1 = new MockQuorumAction(0.3f);
			var action2 = new MockQuorumAction(0.8f);
			var action3 = new MockQuorumAction(0.5f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.GreaterThanOrEqualTo(0));
			Assert.That(result, Is.LessThan(actions.Count));
		}
		
		[Test]
		public void Aggregate_AllZeroUtilities_ReturnsNegativeOne()
		{
			var action1 = new MockQuorumAction(0f);
			var action2 = new MockQuorumAction(0f);
			var action3 = new MockQuorumAction(0f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(-1));
		}
		
		[Test]
		public void Aggregate_HigherUtilityMoreLikely_ReturnsHigherUtilityWithSeed()
		{
			var highUtilityAction = new MockQuorumAction(0.9f);
			var lowUtilityAction = new MockQuorumAction(0.1f);
			var actions = new List<IQuorumAction> { lowUtilityAction, highUtilityAction };
			
			int highUtilityCount = 0;
			int iterations = 100;
			
			for (int i = 0; i < iterations; i++)
			{
				Random.InitState(i);
				int result = _aggregator.Aggregate(actions, _context);
				if (result == 1)
				{
					highUtilityCount++;
				}
			}
			
			Assert.That(highUtilityCount, Is.GreaterThan(iterations * 0.7f));
		}
		
		[Test]
		public void Aggregate_MultipleCallsWithDifferentSeeds_ReturnDifferentResults()
		{
			var action1 = new MockQuorumAction(0.3f);
			var action2 = new MockQuorumAction(0.4f);
			var action3 = new MockQuorumAction(0.3f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			var results = new HashSet<int>();
			
			for (int i = 0; i < 50; i++)
			{
				Random.InitState(i);
				results.Add(_aggregator.Aggregate(actions, _context));
			}
			
			Assert.That(results.Count, Is.GreaterThan(1));
		}
		
		[Test]
		public void Aggregate_ZeroWeightAction_IsNeverPicked()
		{
			var zeroWeightAction = new MockQuorumAction(0f);
			var weightedAction = new MockQuorumAction(1f);
			var actions = new List<IQuorumAction> { zeroWeightAction, weightedAction };
			
			for (int i = 0; i < 200; i++)
			{
				Random.InitState(i);
				
				Assert.That(_aggregator.Aggregate(actions, _context), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void Aggregate_ManySmallWeights_NeverReturnsNegativeOne()
		{
			var actions = new List<IQuorumAction>();
			for (int i = 0; i < 64; i++)
			{
				actions.Add(new MockQuorumAction(0.01f));
			}
			
			for (int i = 0; i < 200; i++)
			{
				Random.InitState(i);
				
				Assert.That(_aggregator.Aggregate(actions, _context), Is.GreaterThanOrEqualTo(0));
			}
		}
		
		private class MockQuorumContext : IQuorumContext
		{
			public IQuorumAgent BaseAgent => null;
			public Blackboard.RuntimeBlackboard Blackboard => null;
		}
		
		private class MockQuorumAction : IQuorumAction
		{
			private readonly float _utility;
			
			public MockQuorumAction(float utility)
			{
				_utility = utility;
			}
			
			public void Initialize(IQuorumContext context) { }
			
			public void Enter(IQuorumContext context) { }
			
			public QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;
			
			public float CalculateUtility(IQuorumContext context) => _utility;
			
			public void Exit(IQuorumContext context) { }
		}
	}
}
