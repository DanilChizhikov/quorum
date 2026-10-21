using DTech.Blackboard;
using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class FormulaConsiderationTests
	{
		private const float Tolerance = 0.0001f;

		private RuntimeBlackboard _blackboard;
		private TestQuorumContext _context;

		[SetUp]
		public void SetUp()
		{
			_blackboard = new RuntimeBlackboard();
			_context = new TestQuorumContext(new TestQuorumAgent(), _blackboard);
		}

		[TearDown]
		public void TearDown()
		{
			_blackboard?.Dispose();
			_blackboard = null;
		}

		[Test]
		public void Evaluate_FloatVariable_UsesBlackboardValue()
		{
			AddFloat("Hunger", 80f);
			var consideration = new FormulaConsideration("clamp01(Hunger / 100)");
			consideration.Initialize(_context);

			Assert.AreEqual(0.8f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_IntVariable_UsesBlackboardValue()
		{
			_blackboard.Add(new BlackboardVariable<int> { Name = "Wood", Value = 3 });
			var consideration = new FormulaConsideration("Wood * 2");
			consideration.Initialize(_context);

			Assert.AreEqual(6f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_BoolVariable_ReadsAsOneOrZero()
		{
			var variable = new BlackboardVariable<bool> { Name = "FoodAvailable", Value = true };
			_blackboard.Add(variable);
			var consideration = new FormulaConsideration("if(FoodAvailable, 1, 0.2)");
			consideration.Initialize(_context);

			Assert.AreEqual(1f, consideration.Evaluate(_context), Tolerance);

			variable.Value = false;

			Assert.AreEqual(0.2f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_VariableNameIsCaseInsensitive()
		{
			AddFloat("Hunger", 50f);
			var consideration = new FormulaConsideration("hunger");
			consideration.Initialize(_context);

			Assert.AreEqual(50f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_BracedVariableName_ResolvesNameWithSpaces()
		{
			AddFloat("Max Hunger", 120f);
			var consideration = new FormulaConsideration("{Max Hunger} / 2");
			consideration.Initialize(_context);

			Assert.AreEqual(60f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_ChangedVariableValue_IsPickedUp()
		{
			BlackboardVariable<float> variable = AddFloat("Hunger", 10f);
			var consideration = new FormulaConsideration("Hunger");
			consideration.Initialize(_context);

			Assert.AreEqual(10f, consideration.Evaluate(_context), Tolerance);

			variable.Value = 42f;

			Assert.AreEqual(42f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_MissingVariable_ReturnsZero()
		{
			var consideration = new FormulaConsideration("Hunger");
			consideration.Initialize(_context);

			Assert.AreEqual(0f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_OneMissingVariable_ZeroesTheWholeFormula()
		{
			var consideration = new FormulaConsideration("100 - Hunger");
			consideration.Initialize(_context);

			Assert.AreEqual(0f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_PartiallyMissingVariables_ReturnsZero()
		{
			AddFloat("Hunger", 80f);
			var consideration = new FormulaConsideration("Hunger + Thirst");
			consideration.Initialize(_context);

			Assert.AreEqual(0f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_InvalidFormula_ReturnsZeroWithoutThrowing()
		{
			var consideration = new FormulaConsideration("1 +");
			consideration.Initialize(_context);

			Assert.AreEqual(0f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_NonFiniteResult_ReturnsZero()
		{
			var consideration = new FormulaConsideration("pow(0, -1)");
			consideration.Initialize(_context);

			Assert.AreEqual(0f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_WithoutInitialize_ReturnsZero()
		{
			AddFloat("Hunger", 25f);
			var consideration = new FormulaConsideration("Hunger / 100");

			Assert.AreEqual(0f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluate_NullBlackboard_ReturnsZero()
		{
			var context = new TestQuorumContext(new TestQuorumAgent(), null);
			var consideration = new FormulaConsideration("Hunger");
			consideration.Initialize(context);

			Assert.AreEqual(0f, consideration.Evaluate(context), Tolerance);
		}

		[Test]
		public void Initialize_AnotherBlackboard_RebindsToIt()
		{
			AddFloat("Hunger", 1f);
			var consideration = new FormulaConsideration("Hunger");
			consideration.Initialize(_context);

			Assert.AreEqual(1f, consideration.Evaluate(_context), Tolerance);

			using var other = new RuntimeBlackboard();
			other.Add(new BlackboardVariable<float> { Name = "Hunger", Value = 2f });
			var otherContext = new TestQuorumContext(new TestQuorumAgent(), other);
			consideration.Initialize(otherContext);

			Assert.AreEqual(2f, consideration.Evaluate(otherContext), Tolerance);
		}

		[Test]
		public void UnresolvedVariables_NamesEveryMissingVariableInOrder()
		{
			AddFloat("Energy", 1f);
			var consideration = new FormulaConsideration("Hunger + Energy + Thirst");
			consideration.Initialize(_context);

			Assert.AreEqual("Hunger, Thirst", consideration.UnresolvedVariables);
		}

		[Test]
		public void UnresolvedVariables_WhenEveryNameResolves_IsNull()
		{
			AddFloat("Hunger", 1f);
			var consideration = new FormulaConsideration("Hunger * 2");
			consideration.Initialize(_context);

			Assert.IsNull(consideration.UnresolvedVariables);
		}

		[Test]
		public void UnresolvedVariables_WithoutBlackboard_IsNull()
		{
			var context = new TestQuorumContext(new TestQuorumAgent(), null);
			var consideration = new FormulaConsideration("Hunger");
			consideration.Initialize(context);

			Assert.IsNull(consideration.UnresolvedVariables);
		}

		[Test]
		public void UnresolvedVariables_AfterTheNameAppears_IsNullAgain()
		{
			var consideration = new FormulaConsideration("Hunger");
			consideration.Initialize(_context);

			Assert.AreEqual("Hunger", consideration.UnresolvedVariables);

			AddFloat("Hunger", 7f);
			consideration.Initialize(_context);

			Assert.IsNull(consideration.UnresolvedVariables);
			Assert.AreEqual(7f, consideration.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Clone_ScoresOnlyAfterItsOwnInitialize()
		{
			AddFloat("Hunger", 4f);
			var original = new FormulaConsideration("Hunger * 2");
			original.Initialize(_context);

			var clone = (FormulaConsideration)original.Clone();

			Assert.AreNotSame(original, clone);
			Assert.AreEqual(0f, clone.Evaluate(_context), Tolerance);

			clone.Initialize(_context);

			Assert.AreEqual(8f, clone.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Clone_TwoBrains_KeepTheirOwnBlackboard()
		{
			var original = new FormulaConsideration("Hunger");
			var first = (FormulaConsideration)original.Clone();
			var second = (FormulaConsideration)original.Clone();

			using var firstBlackboard = new RuntimeBlackboard();
			firstBlackboard.Add(new BlackboardVariable<float> { Name = "Hunger", Value = 1f });
			using var secondBlackboard = new RuntimeBlackboard();
			secondBlackboard.Add(new BlackboardVariable<float> { Name = "Hunger", Value = 2f });

			var firstContext = new TestQuorumContext(new TestQuorumAgent(), firstBlackboard);
			var secondContext = new TestQuorumContext(new TestQuorumAgent(), secondBlackboard);
			first.Initialize(firstContext);
			second.Initialize(secondContext);

			Assert.AreEqual(1f, first.Evaluate(firstContext), Tolerance);
			Assert.AreEqual(2f, second.Evaluate(secondContext), Tolerance);
			Assert.AreEqual(1f, first.Evaluate(firstContext), Tolerance);
		}

		[Test]
		public void Evaluate_ComposedInComposite_MultipliesWithOtherConsiderations()
		{
			AddFloat("Hunger", 50f);
			var composite = new CompositeConsideration(
				ConsiderationCombineMode.Multiply,
				new FormulaConsideration("Hunger / 100"),
				new FormulaConsideration("2"));

			composite.Initialize(_context);

			Assert.AreEqual(1f, composite.Evaluate(_context), Tolerance);
		}

		[Test]
		public void Evaluator_ChangedExpression_IsNoLongerCurrent()
		{
			AddFloat("Hunger", 10f);
			var evaluator = new FormulaEvaluator();
			Assert.IsTrue(evaluator.TrySetExpression("Hunger"));
			evaluator.Bind(_blackboard);

			Assert.IsTrue(evaluator.IsCurrent("Hunger", _blackboard));
			Assert.AreEqual(10f, evaluator.Evaluate(0f), Tolerance);
			Assert.IsFalse(evaluator.IsCurrent("Hunger * 2", _blackboard));

			Assert.IsTrue(evaluator.TrySetExpression("Hunger * 2"));
			evaluator.Bind(_blackboard);

			Assert.AreEqual(20f, evaluator.Evaluate(0f), Tolerance);
		}

		[Test]
		public void Evaluator_InvalidExpression_ReportsErrorAndFallsBack()
		{
			var evaluator = new FormulaEvaluator();

			Assert.IsFalse(evaluator.TrySetExpression("clamp01("));

			Assert.IsFalse(evaluator.IsCompiled);
			Assert.IsFalse(evaluator.Error.IsValid);
			Assert.AreEqual(0.5f, evaluator.Evaluate(0.5f), Tolerance);
		}

		[Test]
		public void Evaluator_MissingVariable_FallsBackInsteadOfSubstitutingZero()
		{
			var evaluator = new FormulaEvaluator();
			Assert.IsTrue(evaluator.TrySetExpression("100 - Hunger"));
			evaluator.Bind(_blackboard);

			Assert.AreEqual("Hunger", evaluator.UnresolvedVariables);
			Assert.AreEqual(-7f, evaluator.Evaluate(-7f), Tolerance);
		}

		private BlackboardVariable<float> AddFloat(string name, float value)
		{
			var variable = new BlackboardVariable<float> { Name = name, Value = value };
			_blackboard.Add(variable);
			return variable;
		}
	}
}
