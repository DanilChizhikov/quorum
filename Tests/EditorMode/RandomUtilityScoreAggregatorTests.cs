using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class RandomUtilityScoreAggregatorTests
	{
		private RandomUtilityScoreAggregator _aggregator;
		private MockUtilityContext _context;
		
		[SetUp]
		public void Setup()
		{
			_aggregator = new RandomUtilityScoreAggregator();
			_context = new MockUtilityContext();
			Random.InitState(42);
		}
		
		[Test]
		public void Aggregate_EmptyActions_ReturnsNegativeOne()
		{
			var actions = new List<IUtilityAction>();
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(-1));
		}
		
		[Test]
		public void Aggregate_SingleAction_ReturnsZero()
		{
			var action = new MockUtilityAction(0.5f);
			var actions = new List<IUtilityAction> { action };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(0));
		}
		
		[Test]
		public void Aggregate_MultipleActions_ReturnsValidIndex()
		{
			var action1 = new MockUtilityAction(0.3f);
			var action2 = new MockUtilityAction(0.8f);
			var action3 = new MockUtilityAction(0.5f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.GreaterThanOrEqualTo(0));
			Assert.That(result, Is.LessThan(actions.Count));
		}
		
		[Test]
		public void Aggregate_AllZeroUtilities_ReturnsNegativeOne()
		{
			var action1 = new MockUtilityAction(0f);
			var action2 = new MockUtilityAction(0f);
			var action3 = new MockUtilityAction(0f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(-1));
		}
		
		[Test]
		public void Aggregate_HigherUtilityMoreLikely_ReturnsHigherUtilityWithSeed()
		{
			var highUtilityAction = new MockUtilityAction(0.9f);
			var lowUtilityAction = new MockUtilityAction(0.1f);
			var actions = new List<IUtilityAction> { lowUtilityAction, highUtilityAction };
			
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
			var action1 = new MockUtilityAction(0.3f);
			var action2 = new MockUtilityAction(0.4f);
			var action3 = new MockUtilityAction(0.3f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
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
			var zeroWeightAction = new MockUtilityAction(0f);
			var weightedAction = new MockUtilityAction(1f);
			var actions = new List<IUtilityAction> { zeroWeightAction, weightedAction };
			
			for (int i = 0; i < 200; i++)
			{
				Random.InitState(i);
				
				Assert.That(_aggregator.Aggregate(actions, _context), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void Aggregate_ManySmallWeights_NeverReturnsNegativeOne()
		{
			var actions = new List<IUtilityAction>();
			for (int i = 0; i < 64; i++)
			{
				actions.Add(new MockUtilityAction(0.01f));
			}
			
			for (int i = 0; i < 200; i++)
			{
				Random.InitState(i);
				
				Assert.That(_aggregator.Aggregate(actions, _context), Is.GreaterThanOrEqualTo(0));
			}
		}
		
		private class MockUtilityContext : IUtilityContext
		{
			public IUtilityAgent BaseAgent => null;
			public Blackboard.RuntimeBlackboard Blackboard => null;
		}
		
		private class MockUtilityAction : IUtilityAction
		{
			private readonly float _utility;
			
			public MockUtilityAction(float utility)
			{
				_utility = utility;
			}
			
			public void Initialize(IUtilityContext context) { }
			
			public void Enter(IUtilityContext context) { }
			
			public UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;
			
			public float CalculateUtility(IUtilityContext context) => _utility;
			
			public void Exit(IUtilityContext context) { }
		}
	}
}
