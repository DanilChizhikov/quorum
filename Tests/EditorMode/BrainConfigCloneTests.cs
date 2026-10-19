using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class BrainConfigCloneTests
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
		public void Brain_UsesClonedConfig_NotSourceAsset()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			
			using var brain = new TestUtilityBrain(_config, _context);
			
			Assert.That(brain.RuntimeConfig, Is.Not.SameAs(_config));
		}
		
		[Test]
		public void Brain_ActionsAreClones_NotSourceInstances()
		{
			var sourceAction = new TestUtilityAction(1f);
			_config.SetActions(new UtilityAction[] { sourceAction });
			
			using var brain = new TestUtilityBrain(_config, _context);
			
			Assert.That(brain.Actions.Count, Is.EqualTo(1));
			Assert.That(brain.Actions[0], Is.Not.SameAs(sourceAction));
		}
		
		[Test]
		public void TwoBrains_FromSameConfig_HaveIndependentActions()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			
			using var firstBrain = new TestUtilityBrain(_config, _context);
			using var secondBrain = new TestUtilityBrain(_config, _context);
			
			var firstAction = (TestUtilityAction)firstBrain.Actions[0];
			var secondAction = (TestUtilityAction)secondBrain.Actions[0];
			firstAction.SetUtility(0.25f);
			
			Assert.That(firstAction, Is.Not.SameAs(secondAction));
			Assert.That(secondAction.Utility, Is.EqualTo(1f));
		}
		
		[Test]
		public void Brain_MutatingClonedAction_DoesNotChangeSourceConfig()
		{
			var sourceAction = new TestUtilityAction(1f);
			_config.SetActions(new UtilityAction[] { sourceAction });
			
			using var brain = new TestUtilityBrain(_config, _context);
			var clonedAction = (TestUtilityAction)brain.Actions[0];
			clonedAction.SetUtility(0.5f);
			
			Assert.That(sourceAction.Utility, Is.EqualTo(1f));
		}
		
		[Test]
		public void Brain_NestedConsideration_IsClonedAsWell()
		{
			var sourceConsideration = new TestUtilityConsideration(1f);
			var sourceAction = new TestUtilityAction(1f);
			sourceAction.SetConsideration(sourceConsideration);
			_config.SetActions(new UtilityAction[] { sourceAction });
			
			using var brain = new TestUtilityBrain(_config, _context);
			var clonedAction = (TestUtilityAction)brain.Actions[0];
			var clonedConsideration = clonedAction.ActionConsideration as TestUtilityConsideration;
			
			Assert.That(clonedConsideration, Is.Not.Null);
			Assert.That(clonedConsideration, Is.Not.SameAs(sourceConsideration));
			
			clonedConsideration.SetScore(0.5f);
			
			Assert.That(sourceConsideration.Score, Is.EqualTo(1f));
		}
		
		[Test]
		public void Dispose_DestroysClonedConfig()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			var brain = new TestUtilityBrain(_config, _context);
			TestBrainConfig runtimeConfig = brain.RuntimeConfig;
			
			brain.Dispose();
			
			Assert.That(runtimeConfig == null, Is.True);
			Assert.That(_config == null, Is.False);
		}
	}
}
