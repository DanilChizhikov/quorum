using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class ScriptableQuorumActionReferenceTests
	{
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestQuorumContext _context;
		private TestScriptableAction _action;
		
		[SetUp]
		public void Setup()
		{
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestQuorumContext(new TestQuorumAgent(), _blackboard);
			_action = ScriptableObject.CreateInstance<TestScriptableAction>();
		}
		
		[TearDown]
		public void TearDown()
		{
			if (_action != null)
			{
				Object.DestroyImmediate(_action);
			}
			
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Initialize_ForwardsToAsset()
		{
			var reference = new ScriptableQuorumActionReference(_action);
			
			reference.Initialize(_context);
			
			Assert.That(_action.InitializeCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Enter_ForwardsToAsset()
		{
			var reference = new ScriptableQuorumActionReference(_action);
			
			reference.Enter(_context);
			
			Assert.That(_action.EnterCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Tick_ForwardsToAssetAndReturnsItsState()
		{
			var reference = new ScriptableQuorumActionReference(_action);
			_action.TickState = QuorumActionState.Completed;
			
			QuorumActionState state = reference.Tick(_context);
			
			Assert.That(_action.TickCount, Is.EqualTo(1));
			Assert.That(state, Is.EqualTo(QuorumActionState.Completed));
		}
		
		[Test]
		public void CalculateUtility_ForwardsToAsset()
		{
			var reference = new ScriptableQuorumActionReference(_action);
			_action.SetUtility(0.75f);
			
			float utility = reference.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0.75f));
		}
		
		[Test]
		public void Exit_ForwardsToAsset()
		{
			var reference = new ScriptableQuorumActionReference(_action);
			
			reference.Exit(_context);
			
			Assert.That(_action.ExitCount, Is.EqualTo(1));
		}
		
		[Test]
		public void WithoutAsset_TickReturnsFailed()
		{
			var reference = new ScriptableQuorumActionReference();
			
			QuorumActionState state = reference.Tick(_context);
			
			Assert.That(state, Is.EqualTo(QuorumActionState.Failed));
		}
		
		[Test]
		public void WithoutAsset_CalculateUtilityReturnsZero()
		{
			var reference = new ScriptableQuorumActionReference();
			
			float utility = reference.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0f));
		}
		
		[Test]
		public void WithoutAsset_LifecycleCallsDoNotThrow()
		{
			var reference = new ScriptableQuorumActionReference();
			
			Assert.DoesNotThrow(() => reference.Initialize(_context));
			Assert.DoesNotThrow(() => reference.Enter(_context));
			Assert.DoesNotThrow(() => reference.Exit(_context));
		}
	}
}
