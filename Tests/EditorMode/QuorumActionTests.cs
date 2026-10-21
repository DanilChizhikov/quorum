using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class QuorumActionTests
	{
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestQuorumContext _context;
		
		[SetUp]
		public void Setup()
		{
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestQuorumContext(new TestQuorumAgent(), _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
		}
		
		[Test]
		public void CalculateUtility_WithConsideration_ReturnsConsiderationScore()
		{
			var action = new ConsiderationDrivenAction();
			action.SetConsideration(new TestFixedConsideration(0.75f));
			
			float utility = action.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0.75f));
		}
		
		[Test]
		public void CalculateUtility_WithoutConsideration_ReturnsZero()
		{
			var action = new ConsiderationDrivenAction();
			
			float utility = action.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0f));
		}
		
		[Test]
		public void Initialize_ByDefault_DoesNotThrow()
		{
			var action = new ConsiderationDrivenAction();
			
			Assert.DoesNotThrow(() => action.Initialize(_context));
		}
		
		private sealed class ConsiderationDrivenAction : QuorumAction
		{
			public override void Enter(IQuorumContext context) { }
			
			public override QuorumActionState Tick(IQuorumContext context) => QuorumActionState.Running;
			
			public override void Exit(IQuorumContext context) { }
		}
	}
}
