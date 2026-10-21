using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class BrainConfigCloneTests
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
		public void Brain_UsesSourceConfig_WithoutCloningIt()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			
			using var brain = new TestQuorumBrain(_config, _context);

			Assert.That(brain.SourceConfig, Is.SameAs(_config));
		}
		
		[Test]
		public void Brain_ActionsAreClones_NotSourceInstances()
		{
			var sourceAction = new TestQuorumAction(1f);
			_config.SetActions(new QuorumAction[] { sourceAction });
			
			using var brain = new TestQuorumBrain(_config, _context);
			
			Assert.That(brain.Actions.Count, Is.EqualTo(1));
			Assert.That(brain.Actions[0], Is.Not.SameAs(sourceAction));
		}
		
		[Test]
		public void TwoBrains_FromSameConfig_HaveIndependentActions()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			
			using var firstBrain = new TestQuorumBrain(_config, _context);
			using var secondBrain = new TestQuorumBrain(_config, _context);
			
			var firstAction = (TestQuorumAction)firstBrain.Actions[0];
			var secondAction = (TestQuorumAction)secondBrain.Actions[0];
			firstAction.SetUtility(0.25f);
			
			Assert.That(firstAction, Is.Not.SameAs(secondAction));
			Assert.That(secondAction.Utility, Is.EqualTo(1f));
		}
		
		[Test]
		public void Brain_MutatingClonedAction_DoesNotChangeSourceConfig()
		{
			var sourceAction = new TestQuorumAction(1f);
			_config.SetActions(new QuorumAction[] { sourceAction });
			
			using var brain = new TestQuorumBrain(_config, _context);
			var clonedAction = (TestQuorumAction)brain.Actions[0];
			clonedAction.SetUtility(0.5f);
			
			Assert.That(sourceAction.Utility, Is.EqualTo(1f));
		}
		
		[Test]
		public void Brain_NestedConsideration_IsClonedAsWell()
		{
			var sourceConsideration = new TestFixedConsideration(1f);
			var sourceAction = new TestQuorumAction(1f);
			sourceAction.SetConsideration(sourceConsideration);
			_config.SetActions(new QuorumAction[] { sourceAction });
			
			using var brain = new TestQuorumBrain(_config, _context);
			var clonedAction = (TestQuorumAction)brain.Actions[0];
			var clonedConsideration = clonedAction.ActionConsideration as TestFixedConsideration;
			
			Assert.That(clonedConsideration, Is.Not.Null);
			Assert.That(clonedConsideration, Is.Not.SameAs(sourceConsideration));
			
			clonedConsideration.SetScore(0.5f);
			
			Assert.That(sourceConsideration.Score, Is.EqualTo(1f));
		}
		
		[Test]
		public void Dispose_DoesNotDestroySourceConfig()
		{
			_config.SetActions(new QuorumAction[] { new TestQuorumAction(1f) });
			var brain = new TestQuorumBrain(_config, _context);

			brain.Dispose();

			Assert.That(_config == null, Is.False);
		}

		[Test]
		public void Brain_DoesNotInitializeSourceConfigActions()
		{
			var sourceAction = new TestQuorumAction(1f);
			_config.SetActions(new QuorumAction[] { sourceAction });

			using var brain = new TestQuorumBrain(_config, _context);

			Assert.That(sourceAction.InitializeCount, Is.EqualTo(0));
			Assert.That(((TestQuorumAction)brain.Actions[0]).InitializeCount, Is.EqualTo(1));
		}
	}
}
