using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class UtilityBrainLifecycleTests
	{
		private TestBrainConfig _config;
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestUtilityContext _context;
		
		[SetUp]
		public void Setup()
		{
			_config = ScriptableObject.CreateInstance<TestBrainConfig>();
			_config.SetScoreAggregator(new MaxUtilityScoreAggregator());
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestUtilityContext(new TestUtilityAgent(), _blackboard);
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
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(0.2f), new TestUtilityAction(0.9f) });
			using var brain = new TestUtilityBrain(_config, _context);
			
			brain.Enable();
			
			var expectedAction = (TestUtilityAction)brain.Actions[1];
			Assert.That(brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(expectedAction));
			Assert.That(expectedAction.EnterCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Update_TicksCurrentActionWithContext()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			using var brain = new TestUtilityBrain(_config, _context);
			brain.Enable();
			
			brain.Update();
			
			var action = (TestUtilityAction)brain.Actions[0];
			Assert.That(action.TickCount, Is.GreaterThanOrEqualTo(1));
			Assert.That(action.LastContext, Is.SameAs(_context));
		}
		
		[Test]
		public void Disable_ExitsCurrentActionAndClearsState()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			using var brain = new TestUtilityBrain(_config, _context);
			brain.Enable();
			var action = (TestUtilityAction)brain.Actions[0];
			
			brain.Disable();
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
			Assert.That(brain.IsEnabled, Is.False);
			Assert.That(brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.False);
			Assert.That(currentAction, Is.Null);
		}
		
		[Test]
		public void Disable_StopsTicking()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			using var brain = new TestUtilityBrain(_config, _context);
			brain.Enable();
			var action = (TestUtilityAction)brain.Actions[0];
			brain.Disable();
			int tickCount = action.TickCount;
			
			brain.Update();
			
			Assert.That(action.TickCount, Is.EqualTo(tickCount));
		}
		
		[Test]
		public void Disable_CalledTwice_ExitsOnlyOnce()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			using var brain = new TestUtilityBrain(_config, _context);
			brain.Enable();
			var action = (TestUtilityAction)brain.Actions[0];
			
			brain.Disable();
			Assert.DoesNotThrow(() => brain.Disable());
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Enable_AfterDisable_EntersActionAgain()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			using var brain = new TestUtilityBrain(_config, _context);
			var action = (TestUtilityAction)brain.Actions[0];
			
			brain.Enable();
			brain.Disable();
			brain.Enable();
			
			Assert.That(action.EnterCount, Is.EqualTo(2));
			Assert.That(brain.IsEnabled, Is.True);
		}
		
		[Test]
		public void Dispose_DisablesBrainAndExitsAction()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			var brain = new TestUtilityBrain(_config, _context);
			brain.Enable();
			var action = (TestUtilityAction)brain.Actions[0];
			
			brain.Dispose();
			
			Assert.That(action.ExitCount, Is.EqualTo(1));
			Assert.That(brain.IsEnabled, Is.False);
		}
		
		[Test]
		public void Enable_WithoutScoreAggregator_LogsError()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			_config.SetScoreAggregator(null);
			using var brain = new TestUtilityBrain(_config, _context);
			
			LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Score aggregator is not set"));
			brain.Enable();
			
			Assert.That(brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.False);
			Assert.That(currentAction, Is.Null);
		}
	}
}
