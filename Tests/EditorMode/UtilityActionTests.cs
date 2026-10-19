using NUnit.Framework;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class UtilityActionTests
	{
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestUtilityContext _context;
		
		[SetUp]
		public void Setup()
		{
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestUtilityContext(new TestUtilityAgent(), _blackboard);
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
			action.SetConsideration(new TestUtilityConsideration(0.75f));
			
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
		
		private sealed class ConsiderationDrivenAction : UtilityAction
		{
			public override void Enter(IUtilityContext context) { }
			
			public override UtilityActionState Tick(IUtilityContext context) => UtilityActionState.Running;
			
			public override void Exit(IUtilityContext context) { }
		}
	}
}
