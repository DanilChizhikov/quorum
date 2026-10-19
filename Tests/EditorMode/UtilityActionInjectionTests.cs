using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class UtilityActionInjectionTests
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
		public void Constructor_InjectsEveryActionOnce()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f), new TestUtilityAction(0.5f) });
			var injector = new RecordingUtilityActionInjector();
			
			using var brain = new TestUtilityBrain(_config, _context, injector);
			
			Assert.That(injector.Targets.Count, Is.EqualTo(2));
			Assert.That(injector.Targets[0], Is.SameAs(brain.Actions[0]));
			Assert.That(injector.Targets[1], Is.SameAs(brain.Actions[1]));
		}
		
		[Test]
		public void Constructor_InjectsBeforeInitialize()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			var injector = new RecordingUtilityActionInjector();
			
			using var brain = new TestUtilityBrain(_config, _context, injector);
			
			Assert.That(injector.InitializeCountsOnInject[0], Is.EqualTo(0));
			Assert.That(((TestUtilityAction)brain.Actions[0]).InitializeCount, Is.EqualTo(1));
		}
		
		[Test]
		public void Constructor_InitializesActionsWithBrainContext()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			
			using var brain = new TestUtilityBrain(_config, _context);
			
			var action = (TestUtilityAction)brain.Actions[0];
			Assert.That(action.InitializeCount, Is.EqualTo(1));
			Assert.That(action.LastContext, Is.SameAs(_context));
		}
		
		[Test]
		public void Constructor_WithoutInjector_DoesNotThrow()
		{
			_config.SetActions(new UtilityAction[] { new TestUtilityAction(1f) });
			
			Assert.DoesNotThrow(() =>
			{
				using var brain = new TestUtilityBrain(_config, _context);
			});
		}
		
		[Test]
		public void Constructor_NullActionEntries_AreSkipped()
		{
			_config.SetActions(new UtilityAction[] { null, new TestUtilityAction(1f), null });
			var injector = new RecordingUtilityActionInjector();
			
			using var brain = new TestUtilityBrain(_config, _context, injector);
			
			Assert.That(brain.Actions.Count, Is.EqualTo(1));
			Assert.That(injector.Targets.Count, Is.EqualTo(1));
		}
	}
}
