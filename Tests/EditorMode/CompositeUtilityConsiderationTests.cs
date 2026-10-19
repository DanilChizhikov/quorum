using NUnit.Framework;

namespace DTech.UtilityAI.Tests.EditorMode
{
	[TestFixture]
	internal sealed class CompositeUtilityConsiderationTests
	{
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestUtilityContext _context;
		
		[SetUp]
		public void Setup()
		{
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestUtilityContext(new TestUtilityAgent(), _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Evaluate_WithoutConsiderations_ReturnsZero()
		{
			var composite = new CompositeUtilityConsideration();
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0f));
		}
		
		[Test]
		public void Evaluate_Multiply_ReturnsProduct()
		{
			CompositeUtilityConsideration composite = CreateComposite(ConsiderationCombineMode.Multiply, 0.5f, 0.5f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.25f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Min_ReturnsLowestScore()
		{
			CompositeUtilityConsideration composite = CreateComposite(ConsiderationCombineMode.Min, 0.8f, 0.3f, 0.5f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.3f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Max_ReturnsHighestScore()
		{
			CompositeUtilityConsideration composite = CreateComposite(ConsiderationCombineMode.Max, 0.8f, 0.3f, 0.5f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.8f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Average_ReturnsMeanScore()
		{
			CompositeUtilityConsideration composite = CreateComposite(ConsiderationCombineMode.Average, 0.2f, 0.4f, 0.6f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.4f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Sum_ReturnsTotalScore()
		{
			CompositeUtilityConsideration composite = CreateComposite(ConsiderationCombineMode.Sum, 0.2f, 0.4f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.6f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_NullEntries_AreSkipped()
		{
			var composite = new CompositeUtilityConsideration(
				ConsiderationCombineMode.Sum,
				null,
				new TestUtilityConsideration(0.5f),
				null);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.5f).Within(0.0001f));
		}
		
		private static CompositeUtilityConsideration CreateComposite(ConsiderationCombineMode mode, params float[] scores)
		{
			var considerations = new UtilityConsideration[scores.Length];
			for (int i = 0; i < scores.Length; i++)
			{
				considerations[i] = new TestUtilityConsideration(scores[i]);
			}
			
			return new CompositeUtilityConsideration(mode, considerations);
		}
	}
}
