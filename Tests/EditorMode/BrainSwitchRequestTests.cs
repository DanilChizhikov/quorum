using System;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class BrainSwitchRequestTests
	{
		private const float PendingDelay = 10f;
		
		private TestBrainConfig _config;
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestUtilityContext _context;
		private TestUtilityBrain _brain;
		
		[SetUp]
		public void Setup()
		{
			_config = ScriptableObject.CreateInstance<TestBrainConfig>();
			_config.SetScoreAggregator(new MaxUtilityScoreAggregator());
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f), new TestUtilityAction(0f) });
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestUtilityContext(new TestUtilityAgent(), _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			_brain?.Dispose();
			_brain = null;
			if (_config != null)
			{
				Object.DestroyImmediate(_config);
			}
			
			_blackboard?.Dispose();
		}
		
		[Test]
		public void RequestSwitchAction_CancelledToken_DoesNotSwitch()
		{
			_brain = CreateEnabledBrain(out TestUtilityAction firstAction, out TestUtilityAction secondAction);
			firstAction.SetUtility(0f);
			secondAction.SetUtility(1f);
			using var cancellation = new CancellationTokenSource();
			cancellation.Cancel();
			
			_brain.RequestSwitchAction(PendingDelay, cancellation.Token);
			_brain.Update();
			
			Assert.That(_brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(firstAction));
			Assert.That(secondAction.EnterCount, Is.EqualTo(0));
		}
		
		[Test]
		public void RequestSwitchAction_AfterCancelledRequest_SwitchesAgain()
		{
			_brain = CreateEnabledBrain(out TestUtilityAction firstAction, out TestUtilityAction secondAction);
			using var cancellation = new CancellationTokenSource();
			cancellation.Cancel();
			_brain.RequestSwitchAction(PendingDelay, cancellation.Token);
			_brain.Update();
			firstAction.SetUtility(0f);
			secondAction.SetUtility(1f);
			
			_brain.RequestSwitchAction();
			
			Assert.That(_brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(secondAction));
		}
		
		[Test]
		public void RequestSwitchAction_Immediate_SupersedesPendingRequest()
		{
			_brain = CreateEnabledBrain(out TestUtilityAction firstAction, out TestUtilityAction secondAction);
			_brain.RequestSwitchAction(PendingDelay, CancellationToken.None);
			firstAction.SetUtility(0f);
			secondAction.SetUtility(1f);
			
			_brain.RequestSwitchAction();
			
			Assert.That(_brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(secondAction));
		}
		
		[Test]
		public void RequestSwitchAction_TwiceAfterPendingRequest_SwitchesBothTimes()
		{
			_brain = CreateEnabledBrain(out TestUtilityAction firstAction, out TestUtilityAction secondAction);
			_brain.RequestSwitchAction(PendingDelay, CancellationToken.None);
			firstAction.SetUtility(0f);
			secondAction.SetUtility(1f);
			_brain.RequestSwitchAction();
			firstAction.SetUtility(1f);
			secondAction.SetUtility(0f);
			
			// The first immediate request must have dropped the pending one: a request left pending would
			// keep the brain flagged as requesting and make this second request a no-op.
			_brain.RequestSwitchAction();
			
			Assert.That(_brain.TryGetCurrentAction(out IUtilityAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(firstAction));
		}
		
		[Test]
		public void Enable_AfterDispose_StaysDisabled()
		{
			_brain = new TestUtilityBrain(_config, _context);
			_brain.Dispose();
			
			_brain.Enable();
			
			Assert.That(_brain.IsEnabled, Is.False);
		}
		
		[Test]
		public void Constructor_NullConfig_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new TestUtilityBrain(null, _context));
		}
		
		[Test]
		public void Constructor_NullContext_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new TestUtilityBrain(_config, null));
		}
		
		private TestUtilityBrain CreateEnabledBrain(out TestUtilityAction firstAction, out TestUtilityAction secondAction)
		{
			var brain = new TestUtilityBrain(_config, _context);
			brain.Enable();
			firstAction = (TestUtilityAction)brain.Actions[0];
			secondAction = (TestUtilityAction)brain.Actions[1];
			return brain;
		}
	}
}
