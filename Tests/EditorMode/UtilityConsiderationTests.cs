using DTech.Blackboard;
using NUnit.Framework;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class UtilityConsiderationTests
	{
		private MockAgent _agent;
		private Blackboard.RuntimeBlackboard _blackboard;
		private MockUtilityContext _context;
		
		[SetUp]
		public void Setup()
		{
			_agent = new MockAgent();
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new MockUtilityContext(_agent, _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Evaluate_ReturnsExpectedValue()
		{
			var consideration = new TestConsideration();
			consideration.SetReturnValue(0.75f);
			
			float result = consideration.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.75f));
		}
		
		[Test]
		public void Evaluate_WithoutBlackboardVariable_ReturnsFallbackValue()
		{
			var consideration = new ContextDependentConsideration();
			
			float result = consideration.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.5f));
		}
		
		[Test]
		public void Evaluate_NullContext_HandlesGracefully()
		{
			var consideration = new TestConsideration();
			consideration.SetReturnValue(0.5f);
			
			Assert.DoesNotThrow(() => consideration.Evaluate(null));
			
		}
		
		private class MockAgent : IUtilityAgent { }
		
		private class MockUtilityContext : UtilityContext<MockAgent>
		{
			public MockUtilityContext(MockAgent agent, Blackboard.RuntimeBlackboard blackboard) : base(agent, blackboard) { }
		}
		
		private class TestConsideration : UtilityConsideration
		{
			private float _returnValue = 1f;
			
			public void SetReturnValue(float value) => _returnValue = value;
			
			public override float Evaluate(IUtilityContext context) => _returnValue;
		}
		
		private class ContextDependentConsideration : UtilityConsideration
		{
			public override float Evaluate(IUtilityContext context)
			{
				if (context?.Blackboard == null)
				{
					return 0f;
				}
				
				if (context.Blackboard.TryGetVariable("TestValue", out BlackboardVariable<int> variable))
				{
					return variable.Value / 100f;
				}
				
				return 0.5f;
			}
		}
	}
}
