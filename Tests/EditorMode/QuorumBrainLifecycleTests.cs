using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class QuorumBrainLifecycleTests
	{
		private TestBrainConfig _config;
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestQuorumContext _context;
		
		[SetUp]
		public void Setup()
		{
			_config = ScriptableObject.CreateInstance<TestBrainConfig>();
			_config.SetScoreAggregator(new MaxScoreAggregator());
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestQuorumContext(new TestQuorumAgent(), _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			if (_config != null)
			{
				Object.DestroyImmediate(_config);
			}
			
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Enable_EntersHighestUtilityAction()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.2f), new TestQuorumAction(0.9f) });
			using var brain = new TestQuorumBrain(_config, _context);
			
			brain.Enable();
			
			var expectedAction = (TestQuorumAction)brain.Actions[1];
			Assert.That(brain.TryGetCurrentAction(out IQuorumAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(expectedAction));
			Assert.That(expectedAction.EnterCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Update_TicksCurrentActionWithContext()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			using var brain = new TestQuorumBrain(_config, _context);
			brain.Enable();
			
			brain.Update();
			
			var action = (TestQuorumAction)brain.Actions[0];
			Assert.That(action.TickCount, Is.GreaterThanOrEqualTo(1));
			Assert.That(action.LastContext, Is.SameAs(_context));
		}
		
		[Test]
		public void Update_CompletedActionWinsAgain_RestartsIt()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f), new TestQuorumAction(0.5f) });
			using var brain = new TestQuorumBrain(_config, _context);
			var action = (TestQuorumAction)brain.Actions[0];
			brain.Enable();
			action.TickState = QuorumActionState.Completed;
			
			brain.Update();
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
			Assert.That(action.EnterCount, Is.EqualTo(2));
		}
		
		[Test]
		public void RequestSwitchAction_RunningActionWinsAgain_DoesNotRestartIt()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f), new TestQuorumAction(0.5f) });
			using var brain = new TestQuorumBrain(_config, _context);
			var action = (TestQuorumAction)brain.Actions[0];
			brain.Enable();
			brain.Update();
			
			brain.RequestSwitchAction();
			
			Assert.That(action.ExitCount, Is.EqualTo(0));
			Assert.That(action.EnterCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Disable_ExitsCurrentActionAndClearsState()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			using var brain = new TestQuorumBrain(_config, _context);
			brain.Enable();
			var action = (TestQuorumAction)brain.Actions[0];
			
			brain.Disable();
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
			Assert.That(brain.IsEnabled, Is.False);
			Assert.That(brain.TryGetCurrentAction(out IQuorumAction currentAction), Is.False);
			Assert.That(currentAction, Is.Null);
		}
		
		[Test]
		public void Disable_StopsTicking()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			using var brain = new TestQuorumBrain(_config, _context);
			brain.Enable();
			var action = (TestQuorumAction)brain.Actions[0];
			brain.Disable();
			int tickCount = action.TickCount;
			
			brain.Update();
			
			Assert.That(action.TickCount, Is.EqualTo(tickCount));
		}
		
		[Test]
		public void Disable_CalledTwice_ExitsOnlyOnce()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			using var brain = new TestQuorumBrain(_config, _context);
			brain.Enable();
			var action = (TestQuorumAction)brain.Actions[0];
			
			brain.Disable();
			Assert.DoesNotThrow(() => brain.Disable());
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Enable_AfterDisable_EntersActionAgain()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			using var brain = new TestQuorumBrain(_config, _context);
			var action = (TestQuorumAction)brain.Actions[0];
			
			brain.Enable();
			brain.Disable();
			brain.Enable();
			
			Assert.That(action.EnterCount, Is.EqualTo(2));
			Assert.That(brain.IsEnabled, Is.True);
		}
		
		[Test]
		public void Dispose_DisablesBrainAndExitsAction()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			var brain = new TestQuorumBrain(_config, _context);
			brain.Enable();
			var action = (TestQuorumAction)brain.Actions[0];
			
			brain.Dispose();
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
			Assert.That(brain.IsEnabled, Is.False);
		}
		
		[Test]
		public void Enable_WithoutScoreAggregator_LogsError()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			_config.SetScoreAggregator(null);
			using var brain = new TestQuorumBrain(_config, _context);
			
			LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Score aggregator is not set"));
			brain.Enable();
			
			Assert.That(brain.TryGetCurrentAction(out IQuorumAction currentAction), Is.False);
			Assert.That(currentAction, Is.Null);
		}
	}
}
