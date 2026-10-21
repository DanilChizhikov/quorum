using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class ScriptableQuorumActionTests
	{
		private MockAgent _agent;
		private Blackboard.RuntimeBlackboard _blackboard;
		private MockQuorumContext _context;
		
		[SetUp]
		public void Setup()
		{
			_agent = new MockAgent();
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new MockQuorumContext(_agent, _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Enter_CallsOverrideMethod()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			action.SetConsideration(new MockConsideration());
			
			action.Enter(_context);
			
			Assert.That(action.EnterCalled, Is.True);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Tick_CallsOverrideMethod()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			action.SetConsideration(new MockConsideration());
			
			action.Tick(_context);
			
			Assert.That(action.TickCalled, Is.True);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Exit_CallsOverrideMethod()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			action.SetConsideration(new MockConsideration());
			
			action.Exit(_context);
			
			Assert.That(action.ExitCalled, Is.True);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void CalculateUtility_WithConsideration_ReturnsConsiderationValue()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
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
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			
			float utility = action.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0f));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Enter_WrongContextType_DoesNotCallOverride()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			var wrongContext = new WrongMockQuorumContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockQuorumContext but got WrongMockQuorumContext"));
			
			action.Enter(wrongContext);
			
			Assert.That(action.EnterCalled, Is.False);
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Tick_WrongContextType_ReturnsFailed()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			var wrongContext = new WrongMockQuorumContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockQuorumContext but got WrongMockQuorumContext"));
			
			QuorumActionState result = action.Tick(wrongContext);
			
			Assert.That(result, Is.EqualTo(QuorumActionState.Failed));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void CalculateUtility_WrongContextType_ReturnsZero()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			var wrongContext = new WrongMockQuorumContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockQuorumContext but got WrongMockQuorumContext"));
			
			float utility = action.CalculateUtility(wrongContext);
			
			Assert.That(utility, Is.EqualTo(0f));
			Object.DestroyImmediate(action);
		}
		
		[Test]
		public void Exit_WrongContextType_DoesNotCallOverride()
		{
			var action = ScriptableObject.CreateInstance<TestScriptableQuorumAction>();
			var wrongContext = new WrongMockQuorumContext(new WrongMockAgent(), _blackboard);
			
			LogAssert.Expect(LogType.Error, new Regex("expects context MockQuorumContext but got WrongMockQuorumContext"));
			
			action.Exit(wrongContext);
			
			Assert.That(action.ExitCalled, Is.False);
			Object.DestroyImmediate(action);
		}
		
		private class MockAgent : IQuorumAgent { }
		private class WrongMockAgent : IQuorumAgent { }
		
		private class MockQuorumContext : QuorumContext<MockAgent>
		{
			public MockQuorumContext(MockAgent agent, Blackboard.RuntimeBlackboard blackboard) : base(agent, blackboard) { }
		}
		
		private class WrongMockQuorumContext : QuorumContext<WrongMockAgent>
		{
			public WrongMockQuorumContext(WrongMockAgent agent, Blackboard.RuntimeBlackboard blackboard) : base(agent, blackboard) { }
		}
		
		private class MockConsideration : Consideration
		{
			private float _utility = 1f;
			
			public void SetUtility(float utility) => _utility = utility;
			
			public override float Evaluate(IQuorumContext context) => _utility;
		}
		
		private class TestScriptableQuorumAction : ScriptableQuorumAction<MockAgent, MockQuorumContext>
		{
			public bool EnterCalled { get; private set; }
			public bool TickCalled { get; private set; }
			public bool ExitCalled { get; private set; }
			

			protected override void Enter(MockQuorumContext context)
			{
				EnterCalled = true;
			}
			
			protected override QuorumActionState OnTick(MockQuorumContext context)
			{
				TickCalled = true;
				return QuorumActionState.Running;
			}
			
			protected override float CalculateUtility(MockQuorumContext context) =>
				Consideration != null ? Consideration.Evaluate(context) : 0f;
			
			protected override void Exit(MockQuorumContext context)
			{
				ExitCalled = true;
			}
		}
	}
}
