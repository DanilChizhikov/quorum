using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class QuorumBrainTests
	{
		private TestBrainConfig _config;
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestQuorumContext _context;
		private TestQuorumBrain _brain;
		
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
			_brain?.Dispose();
			_brain = null;
			if (_config != null)
			{
				Object.DestroyImmediate(_config);
			}
			
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Constructor_WithConfig_SetsProperties()
		{
			_brain = CreateBrain();
			
			Assert.That(_brain.IsEnabled, Is.False);
			Assert.That(_brain.UpdateType, Is.EqualTo(_config.UpdateType));
			Assert.That(_brain.Actions.Count, Is.EqualTo(0));
		}
		
		[Test]
		public void Constructor_CopiesConfigActionsIntoBrain()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.5f), new TestQuorumAction(0.1f) });
			
			_brain = CreateBrain();
			
			Assert.That(_brain.Actions.Count, Is.EqualTo(2));
			Assert.That(_brain.Actions[0], Is.InstanceOf<TestQuorumAction>());
		}
		
		[Test]
		public void Enable_WhenDisabled_SetsIsEnabledTrue()
		{
			_brain = CreateBrain();
			
			_brain.Enable();
			
			Assert.That(_brain.IsEnabled, Is.True);
		}
		
		[Test]
		public void Enable_WhenAlreadyEnabled_DoesNotThrow()
		{
			_brain = CreateBrain();
			
			_brain.Enable();
			
			Assert.DoesNotThrow(() => _brain.Enable());
			Assert.That(_brain.IsEnabled, Is.True);
		}
		
		[Test]
		public void Disable_WhenEnabled_SetsIsEnabledFalse()
		{
			_brain = CreateBrain();
			
			_brain.Enable();
			_brain.Disable();
			
			Assert.That(_brain.IsEnabled, Is.False);
		}
		
		[Test]
		public void Disable_WhenAlreadyDisabled_DoesNotThrow()
		{
			_brain = CreateBrain();
			
			_brain.Disable();
			
			Assert.DoesNotThrow(() => _brain.Disable());
			Assert.That(_brain.IsEnabled, Is.False);
		}
		
		[Test]
		public void TryGetCurrentAction_WhenNotEnabled_ReturnsFalse()
		{
			_brain = CreateBrain();
			
			bool result = _brain.TryGetCurrentAction(out IQuorumAction action);
			
			Assert.That(result, Is.False);
			Assert.That(action, Is.Null);
		}
		
		[Test]
		public void TryGetAction_ExistingAction_ReturnsBrainInstance()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.5f) });
			_brain = CreateBrain();
			
			bool result = _brain.TryGetAction(out TestQuorumAction foundAction);
			
			Assert.That(result, Is.True);
			Assert.That(foundAction, Is.SameAs(_brain.Actions[0]));
		}
		
		[Test]
		public void TryGetAction_ByBaseType_ReturnsAction()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.5f) });
			_brain = CreateBrain();
			
			bool result = _brain.TryGetAction(out QuorumAction foundAction);
			
			Assert.That(result, Is.True);
			Assert.That(foundAction, Is.SameAs(_brain.Actions[0]));
		}
		
		[Test]
		public void TryGetAction_NonExistingAction_ReturnsFalse()
		{
			_config.SetActions(Array.Empty<QuorumAction>());
			_brain = CreateBrain();
			
			bool result = _brain.TryGetAction(out TestQuorumAction foundAction);
			
			Assert.That(result, Is.False);
			Assert.That(foundAction, Is.Null);
		}
		
		[Test]
		public void GetAction_ExistingAction_ReturnsAction()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.5f) });
			_brain = CreateBrain();
			
			TestQuorumAction result = _brain.GetAction<TestQuorumAction>();
			
			Assert.That(result, Is.SameAs(_brain.Actions[0]));
		}
		
		[Test]
		public void GetAction_NonExistingAction_ReturnsNull()
		{
			_config.SetActions(Array.Empty<QuorumAction>());
			_brain = CreateBrain();
			
			TestQuorumAction result = _brain.GetAction<TestQuorumAction>();
			
			Assert.That(result, Is.Null);
		}
		
		[Test]
		public void RequestSwitchAction_WhenNotEnabled_DoesNotThrow()
		{
			_brain = CreateBrain();
			
			Assert.DoesNotThrow(() => _brain.RequestSwitchAction());
		}
		
		[Test]
		public void RequestSwitchAction_WhenEnabled_SwitchesToHighestUtilityAction()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.1f), new TestQuorumAction(0.2f) });
			_brain = CreateBrain();
			_brain.Enable();
			var lowUtilityAction = (TestQuorumAction)_brain.Actions[0];
			var highUtilityAction = (TestQuorumAction)_brain.Actions[1];
			lowUtilityAction.SetUtility(0.9f);
			
			_brain.RequestSwitchAction();
			
			Assert.That(_brain.TryGetCurrentAction(out IQuorumAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(lowUtilityAction));
			Assert.That(highUtilityAction.ExitCount, Is.EqualTo(1));
			Assert.That(lowUtilityAction.EnterCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Update_WhenActionCompleted_RequestsSwitch()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(0.9f), new TestQuorumAction(0.1f) });
			_brain = CreateBrain();
			_brain.Enable();
			var firstAction = (TestQuorumAction)_brain.Actions[0];
			var secondAction = (TestQuorumAction)_brain.Actions[1];
			firstAction.TickState = QuorumActionState.Completed;
			firstAction.SetUtility(0f);
			
			_brain.Update();
			
			Assert.That(_brain.TryGetCurrentAction(out IQuorumAction currentAction), Is.True);
			Assert.That(currentAction, Is.SameAs(secondAction));
			Assert.That(firstAction.ExitCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Dispose_WhenNotDisposed_CanBeCalled()
		{
			_brain = CreateBrain();
			
			Assert.DoesNotThrow(() => _brain.Dispose());
		}
		
		[Test]
		public void Dispose_WhenAlreadyDisposed_DoesNotThrow()
		{
			_brain = CreateBrain();
			
			_brain.Dispose();
			
			Assert.DoesNotThrow(() => _brain.Dispose());
		}
		
		private TestQuorumBrain CreateBrain() => new(_config, _context);
	}
}
