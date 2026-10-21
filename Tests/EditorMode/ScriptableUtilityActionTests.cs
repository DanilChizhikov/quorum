using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class ScriptableUtilityActionTests
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
		public void Enter_CallsOverrideMethod()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			action.SetConsideration(new MockConsideration());
			
			action.Enter(_context);
			
			Assert.That(action.EnterCalled, Is.True);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Tick_CallsOverrideMethod()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			action.SetConsideration(new MockConsideration());
			
			action.Tick(_context);
			
			Assert.That(action.TickCalled, Is.True);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Exit_CallsOverrideMethod()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			action.SetConsideration(new MockConsideration());
			
			action.Exit(_context);
			
			Assert.That(action.ExitCalled, Is.True);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void CalculateUtility_WithConsideration_ReturnsConsiderationValue()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			var consideration = new MockConsideration();
			consideration.SetUtility(0.75f);
			action.SetConsideration(consideration);
			
			float utility = action.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0.75f));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void CalculateUtility_WithoutConsideration_ReturnsZero()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			
			float utility = action.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0f));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Enter_WrongContextType_DoesNotCallOverride()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			var wrongContext = new WrongMockUtilityContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockUtilityContext but got WrongMockUtilityContext"));
			
			action.Enter(wrongContext);
			
			Assert.That(action.EnterCalled, Is.False);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Tick_WrongContextType_ReturnsFailed()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			var wrongContext = new WrongMockUtilityContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockUtilityContext but got WrongMockUtilityContext"));
			
			UtilityActionState result = action.Tick(wrongContext);
			
			Assert.That(result, Is.EqualTo(UtilityActionState.Failed));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void CalculateUtility_WrongContextType_ReturnsZero()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			var wrongContext = new WrongMockUtilityContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockUtilityContext but got WrongMockUtilityContext"));
			
			float utility = action.CalculateUtility(wrongContext);
			
			Assert.That(utility, Is.EqualTo(0f));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Exit_WrongContextType_DoesNotCallOverride()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableUtilityAction>();
			var wrongContext = new WrongMockUtilityContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockUtilityContext but got WrongMockUtilityContext"));
			
			action.Exit(wrongContext);
			
			Assert.That(action.ExitCalled, Is.False);
			Object.DestroyImmediate(action);
		}
		
		private class MockAgent : IUtilityAgent { }
		private class WrongMockAgent : IUtilityAgent { }
		
		private class MockUtilityContext : UtilityContext<MockAgent>
		{
			public MockUtilityContext(MockAgent agent, Blackboard.RuntimeBlackboard blackboard) : base(agent, blackboard) { }
		}
		
		private class WrongMockUtilityContext : UtilityContext<WrongMockAgent>
		{
			public WrongMockUtilityContext(WrongMockAgent agent, Blackboard.RuntimeBlackboard blackboard) : base(agent, blackboard) { }
		}
		
		private class MockConsideration : UtilityConsideration
		{
			private float _utility = 1f;
			
			public void SetUtility(float utility) => _utility = utility;
			
			public override float Evaluate(IUtilityContext context) => _utility;
		}
		
		private class TestScriptableUtilityAction : ScriptableUtilityAction<MockAgent, MockUtilityContext>
		{
			public bool EnterCalled { get; private set; }
			public bool TickCalled { get; private set; }
			public bool ExitCalled { get; private set; }
			

			protected override void Enter(MockUtilityContext context)
			{
				EnterCalled = true;
			}
			
			protected override UtilityActionState OnTick(MockUtilityContext context)
			{
				TickCalled = true;
				return UtilityActionState.Running;
			}
			
			protected override float CalculateUtility(MockUtilityContext context) =>
				Consideration != null ? Consideration.Evaluate(context) : 0f;
			
			protected override void Exit(MockUtilityContext context)
			{
				ExitCalled = true;
			}
		}
	}
}
