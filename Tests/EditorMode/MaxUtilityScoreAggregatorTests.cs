using System.Collections.Generic;
using NUnit.Framework;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class MaxUtilityScoreAggregatorTests
	{
		private MaxUtilityScoreAggregator _aggregator;
		private MockUtilityContext _context;
		
		[SetUp]
		public void Setup()
		{
			_aggregator = new MaxUtilityScoreAggregator();
			_context = new MockUtilityContext();
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
		public void Aggregate_MultipleActions_ReturnsHighestUtilityIndex()
		{
			var action1 = new MockUtilityAction(0.3f);
			var action2 = new MockUtilityAction(0.8f);
			var action3 = new MockUtilityAction(0.5f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(1));
		}
		
		[Test]
		public void Aggregate_AllSameUtility_ReturnsFirstIndex()
		{
			var action1 = new MockUtilityAction(0.5f);
			var action2 = new MockUtilityAction(0.5f);
			var action3 = new MockUtilityAction(0.5f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(0));
		}
		
		[Test]
		public void Aggregate_AllNegativeUtilities_ReturnsNegativeOne()
		{
			var action1 = new MockUtilityAction(-0.5f);
			var action2 = new MockUtilityAction(-0.2f);
			var action3 = new MockUtilityAction(-0.8f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(-1));
		}
		
		[Test]
		public void Aggregate_MixedPositiveAndNegative_ReturnsHighestUtilityIndex()
		{
			var action1 = new MockUtilityAction(-0.5f);
			var action2 = new MockUtilityAction(0.3f);
			var action3 = new MockUtilityAction(-0.1f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(1));
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
		public void Aggregate_OnlyOnePositiveUtility_ReturnsThatIndex()
		{
			var action1 = new MockUtilityAction(0f);
			var action2 = new MockUtilityAction(0f);
			var action3 = new MockUtilityAction(0.01f);
			var actions = new List<IUtilityAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(2));
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
