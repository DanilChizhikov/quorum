using System.Collections.Generic;
using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class MaxScoreAggregatorTests
	{
		private MaxScoreAggregator _aggregator;
		private MockQuorumContext _context;
		
		[SetUp]
		public void Setup()
		{
			_aggregator = new MaxScoreAggregator();
			_context = new MockQuorumContext();
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
		public void Aggregate_MultipleActions_ReturnsHighestUtilityIndex()
		{
			var action1 = new MockQuorumAction(0.3f);
			var action2 = new MockQuorumAction(0.8f);
			var action3 = new MockQuorumAction(0.5f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(1));
		}
		
		[Test]
		public void Aggregate_AllSameUtility_ReturnsFirstIndex()
		{
			var action1 = new MockQuorumAction(0.5f);
			var action2 = new MockQuorumAction(0.5f);
			var action3 = new MockQuorumAction(0.5f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(0));
		}
		
		[Test]
		public void Aggregate_AllNegativeUtilities_ReturnsNegativeOne()
		{
			var action1 = new MockQuorumAction(-0.5f);
			var action2 = new MockQuorumAction(-0.2f);
			var action3 = new MockQuorumAction(-0.8f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(-1));
		}
		
		[Test]
		public void Aggregate_MixedPositiveAndNegative_ReturnsHighestUtilityIndex()
		{
			var action1 = new MockQuorumAction(-0.5f);
			var action2 = new MockQuorumAction(0.3f);
			var action3 = new MockQuorumAction(-0.1f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(1));
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
		public void Aggregate_OnlyOnePositiveUtility_ReturnsThatIndex()
		{
			var action1 = new MockQuorumAction(0f);
			var action2 = new MockQuorumAction(0f);
			var action3 = new MockQuorumAction(0.01f);
			var actions = new List<IQuorumAction> { action1, action2, action3 };
			
			int result = _aggregator.Aggregate(actions, _context);
			
			Assert.That(result, Is.EqualTo(2));
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
