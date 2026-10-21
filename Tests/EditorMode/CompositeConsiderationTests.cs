using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class CompositeConsiderationTests
	{
		private Blackboard.RuntimeBlackboard _blackboard;
		private TestQuorumContext _context;
		
		[SetUp]
		public void Setup()
		{
			_blackboard = new Blackboard.RuntimeBlackboard();
			_context = new TestQuorumContext(new TestQuorumAgent(), _blackboard);
		}
		
		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
		}
		
		[Test]
		public void Evaluate_WithoutConsiderations_ReturnsZero()
		{
			var composite = new CompositeConsideration();
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0f));
		}
		
		[Test]
		public void Evaluate_Multiply_ReturnsProduct()
		{
			CompositeConsideration composite = CreateComposite(ConsiderationCombineMode.Multiply, 0.5f, 0.5f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.25f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Min_ReturnsLowestScore()
		{
			CompositeConsideration composite = CreateComposite(ConsiderationCombineMode.Min, 0.8f, 0.3f, 0.5f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.3f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Max_ReturnsHighestScore()
		{
			CompositeConsideration composite = CreateComposite(ConsiderationCombineMode.Max, 0.8f, 0.3f, 0.5f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.8f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Average_ReturnsMeanScore()
		{
			CompositeConsideration composite = CreateComposite(ConsiderationCombineMode.Average, 0.2f, 0.4f, 0.6f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.4f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_Sum_ReturnsTotalScore()
		{
			CompositeConsideration composite = CreateComposite(ConsiderationCombineMode.Sum, 0.2f, 0.4f);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.6f).Within(0.0001f));
		}
		
		[Test]
		public void Evaluate_NullEntries_AreSkipped()
		{
			var composite = new CompositeConsideration(
				ConsiderationCombineMode.Sum,
				null,
				new TestFixedConsideration(0.5f),
				null);
			
			float result = composite.Evaluate(_context);
			
			Assert.That(result, Is.EqualTo(0.5f).Within(0.0001f));
		}
		
		private static CompositeConsideration CreateComposite(ConsiderationCombineMode mode, params float[] scores)
		{
			var considerations = new Consideration[scores.Length];
			for (int i = 0; i < scores.Length; i++)
			{
				considerations[i] = new TestFixedConsideration(scores[i]);
			}
			
			return new CompositeConsideration(mode, considerations);
		}
	}
}
