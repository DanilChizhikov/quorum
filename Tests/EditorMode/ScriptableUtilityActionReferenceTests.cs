using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class ScriptableUtilityActionReferenceTests
	{
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestUtilityContext _context;
		private TestScriptableAction _action;
		
		[SetUp]
		public void Setup()
		{
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestUtilityContext(new TestUtilityAgent(), _blackboard);
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
			var reference = new ScriptableUtilityActionReference(_action);
			
			reference.Initialize(_context);
			
			Assert.That(_action.InitializeCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Enter_ForwardsToAsset()
		{
			var reference = new ScriptableUtilityActionReference(_action);
			
			reference.Enter(_context);
			
			Assert.That(_action.EnterCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Tick_ForwardsToAssetAndReturnsItsState()
		{
			var reference = new ScriptableUtilityActionReference(_action);
			_action.TickState = UtilityActionState.Completed;
			
			UtilityActionState state = reference.Tick(_context);
			
			Assert.That(_action.TickCount, Is.EqualTo(1));
			Assert.That(state, Is.EqualTo(UtilityActionState.Completed));
		}
		
		[Test]
		public void CalculateUtility_ForwardsToAsset()
		{
			var reference = new ScriptableUtilityActionReference(_action);
			_action.SetUtility(0.75f);
			
			float utility = reference.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0.75f));
		}
		
		[Test]
		public void Exit_ForwardsToAsset()
		{
			var reference = new ScriptableUtilityActionReference(_action);
			
			reference.Exit(_context);
			
			Assert.That(_action.ExitCount, Is.EqualTo(1));
		}
		
		[Test]
		public void WithoutAsset_TickReturnsFailed()
		{
			var reference = new ScriptableUtilityActionReference();
			
			UtilityActionState state = reference.Tick(_context);
			
			Assert.That(state, Is.EqualTo(UtilityActionState.Failed));
		}
		
		[Test]
		public void WithoutAsset_CalculateUtilityReturnsZero()
		{
			var reference = new ScriptableUtilityActionReference();
			
			float utility = reference.CalculateUtility(_context);
			
			Assert.That(utility, Is.EqualTo(0f));
		}
		
		[Test]
		public void WithoutAsset_LifecycleCallsDoNotThrow()
		{
			var reference = new ScriptableUtilityActionReference();
			
			Assert.DoesNotThrow(() => reference.Initialize(_context));
			Assert.DoesNotThrow(() => reference.Enter(_context));
			Assert.DoesNotThrow(() => reference.Exit(_context));
		}
	}
}
