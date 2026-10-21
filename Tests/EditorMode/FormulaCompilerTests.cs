using System.Collections.Generic;
using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	[TestFixture]
	internal sealed class FormulaCompilerTests
	{
		private const float Tolerance = 0.0001f;

		[Test]
		public void Compile_Arithmetic_RespectsPrecedence()
		{
			Assert.AreEqual(14f, Evaluate("2 + 3 * 4"), Tolerance);
			Assert.AreEqual(20f, Evaluate("(2 + 3) * 4"), Tolerance);
			Assert.AreEqual(1f, Evaluate("7 % 3"), Tolerance);
			Assert.AreEqual(2f, Evaluate(".5 + 1.5"), Tolerance);
		}

		[Test]
		public void Compile_UnaryMinus_BindsLooserThanPower()
		{
			Assert.AreEqual(-2f, Evaluate("-3 + 1"), Tolerance);
			Assert.AreEqual(-4f, Evaluate("-2 ^ 2"), Tolerance);
			Assert.AreEqual(0.5f, Evaluate("2 ^ -1"), Tolerance);
			Assert.AreEqual(5f, Evaluate("+5"), Tolerance);
		}

		[Test]
		public void Compile_Power_IsRightAssociative()
		{
			Assert.AreEqual(512f, Evaluate("2 ^ 3 ^ 2"), Tolerance);
		}

		[Test]
		public void Compile_Functions_ProduceExpectedValues()
		{
			Assert.AreEqual(1f, Evaluate("clamp01(2)"), Tolerance);
			Assert.AreEqual(3f, Evaluate("clamp(5, 0, 3)"), Tolerance);
			Assert.AreEqual(2f, Evaluate("min(2, 3)"), Tolerance);
			Assert.AreEqual(3f, Evaluate("max(2, 3)"), Tolerance);
			Assert.AreEqual(8f, Evaluate("pow(2, 3)"), Tolerance);
			Assert.AreEqual(5f, Evaluate("lerp(0, 10, 0.5)"), Tolerance);
			Assert.AreEqual(0.5f, Evaluate("inverseLerp(0, 10, 5)"), Tolerance);
			Assert.AreEqual(0.5f, Evaluate("smoothstep(0, 1, 0.5)"), Tolerance);
			Assert.AreEqual(1f, Evaluate("step(1, 2)"), Tolerance);
			Assert.AreEqual(0f, Evaluate("step(1, 0.5)"), Tolerance);
			Assert.AreEqual(2f, Evaluate("abs(-2)"), Tolerance);
			Assert.AreEqual(-1f, Evaluate("sign(-3)"), Tolerance);
			Assert.AreEqual(1f, Evaluate("floor(1.7)"), Tolerance);
			Assert.AreEqual(2f, Evaluate("ceil(1.2)"), Tolerance);
		}

		[Test]
		public void Compile_FunctionNames_AreCaseInsensitive()
		{
			Assert.AreEqual(1f, Evaluate("CLAMP01(2)"), Tolerance);
			Assert.AreEqual(0.5f, Evaluate("InverseLerp(0, 10, 5)"), Tolerance);
		}

		[Test]
		public void Compile_Constants_ProduceExpectedValues()
		{
			Assert.AreEqual(3.14159f, Evaluate("pi"), 0.001f);
			Assert.AreEqual(2.71828f, Evaluate("e"), 0.001f);
		}

		[Test]
		public void Compile_Comparisons_ProduceOneOrZero()
		{
			Assert.AreEqual(1f, Evaluate("3 > 2"), Tolerance);
			Assert.AreEqual(0f, Evaluate("2 > 3"), Tolerance);
			Assert.AreEqual(1f, Evaluate("1 <= 1"), Tolerance);
			Assert.AreEqual(1f, Evaluate("2 == 2"), Tolerance);
			Assert.AreEqual(0f, Evaluate("2 != 2"), Tolerance);
			Assert.AreEqual(1f, Evaluate("1 + 1 == 2"), Tolerance);
		}

		[Test]
		public void Compile_Logic_ProducesOneOrZero()
		{
			Assert.AreEqual(0f, Evaluate("1 && 0"), Tolerance);
			Assert.AreEqual(1f, Evaluate("1 || 0"), Tolerance);
			Assert.AreEqual(1f, Evaluate("!0"), Tolerance);
			Assert.AreEqual(0f, Evaluate("!5"), Tolerance);
			Assert.AreEqual(1f, Evaluate("1 || 0 && 0"), Tolerance);
		}

		[Test]
		public void Compile_Condition_PicksBranch()
		{
			Assert.AreEqual(2f, Evaluate("if(1, 2, 3)"), Tolerance);
			Assert.AreEqual(3f, Evaluate("if(0, 2, 3)"), Tolerance);
		}

		[Test]
		public void Compile_TotalArithmetic_NeverProducesInfinityOrNaN()
		{
			Assert.AreEqual(0f, Evaluate("1 / 0"), Tolerance);
			Assert.AreEqual(0f, Evaluate("5 % 0"), Tolerance);
			Assert.AreEqual(0f, Evaluate("sqrt(-4)"), Tolerance);
			Assert.AreEqual(0f, Evaluate("log(0)"), Tolerance);
			Assert.AreEqual(2f, Evaluate("sqrt(4)"), Tolerance);
			Assert.AreEqual(0f, Evaluate("if(hunger > 0, 1 / hunger, 0)"), Tolerance);
		}

		[Test]
		public void Compile_Variables_ReadFromValuesArray()
		{
			var variables = new Dictionary<string, float> { { "hunger", 8f } };

			Assert.AreEqual(4f, Evaluate("hunger / 2", variables), Tolerance);
		}

		[Test]
		public void Compile_BracedVariableName_AllowsSpaces()
		{
			var variables = new Dictionary<string, float> { { "Max Hunger", 10f } };

			Assert.AreEqual(9f, Evaluate("{Max Hunger} - 1", variables), Tolerance);
		}

		[Test]
		public void Compile_RepeatedVariable_SharesOneSlot()
		{
			Assert.IsTrue(FormulaCompiler.TryCompile("hunger + HUNGER", out FormulaProgram program, out _));

			Assert.AreEqual(1, program.VariableCount);
			Assert.AreEqual("hunger", program.VariableNames[0]);
		}

		[Test]
		public void Compile_Variables_KeepFirstSeenOrder()
		{
			Assert.IsTrue(FormulaCompiler.TryCompile("b + a", out FormulaProgram program, out _));

			Assert.AreEqual(2, program.VariableCount);
			Assert.AreEqual("b", program.VariableNames[0]);
			Assert.AreEqual("a", program.VariableNames[1]);
		}

		[Test]
		public void Compile_EmptyFormula_Fails()
		{
			Assert.AreEqual("Formula is empty", CompileError(null).Message);
			Assert.AreEqual("Formula is empty", CompileError(string.Empty).Message);
			Assert.AreEqual("Formula is empty", CompileError("	 ").Message);
		}

		[Test]
		public void Compile_UnbalancedParentheses_Fails()
		{
			const string unclosed = "clamp01(hunger / 100";

			FormulaError error = CompileError(unclosed);

			Assert.AreEqual("Expected ')'", error.Message);
			Assert.AreEqual(unclosed.Length, error.Position);
			Assert.AreEqual("Unmatched ')'", CompileError("1)").Message);
		}

		[Test]
		public void Compile_UnknownFunction_Fails()
		{
			FormulaError error = CompileError("foo(1)");

			Assert.AreEqual("Unknown function 'foo'", error.Message);
			Assert.AreEqual(0, error.Position);
		}

		[Test]
		public void Compile_WrongArity_Fails()
		{
			StringAssert.Contains("takes 3 argument(s), got 1", CompileError("clamp(1)").Message);
			StringAssert.Contains("takes 2 argument(s), got 3", CompileError("min(1, 2, 3)").Message);
		}

		[Test]
		public void Compile_DanglingOperator_Fails()
		{
			FormulaError error = CompileError("1 +");

			Assert.AreEqual("Expected a value", error.Message);
			Assert.AreEqual(3, error.Position);
			Assert.AreEqual("Expected a value", CompileError("* 2").Message);
		}

		[Test]
		public void Compile_MissingOperator_Fails()
		{
			FormulaError error = CompileError("1 2");

			Assert.AreEqual("Expected an operator", error.Message);
			Assert.AreEqual(2, error.Position);
		}

		[Test]
		public void Compile_UnsupportedSymbols_ExplainThemselves()
		{
			StringAssert.Contains("use '=='", CompileError("1 = 2").Message);
			StringAssert.Contains("use '&&'", CompileError("1 & 2").Message);
			StringAssert.Contains("use '||'", CompileError("1 | 2").Message);
			StringAssert.Contains("Unexpected character '#'", CompileError("1 # 2").Message);
		}

		[Test]
		public void Compile_MalformedVariableName_Fails()
		{
			Assert.AreEqual("Expected '}' to close the variable name", CompileError("{abc").Message);
			Assert.AreEqual("Variable name is empty", CompileError("{}").Message);
			StringAssert.Contains("Unexpected '}'", CompileError("1 + }").Message);
		}

		[Test]
		public void Compile_CommaOutsideCall_Fails()
		{
			Assert.AreEqual("Unexpected ','", CompileError("1, 2").Message);
			Assert.AreEqual("Unexpected ','", CompileError("(1, 2)").Message);
		}

		[Test]
		public void Compile_ValidFormula_ReportsNoError()
		{
			Assert.IsTrue(FormulaCompiler.TryCompile("clamp01(hunger)", out _, out FormulaError error));

			Assert.IsTrue(error.IsValid);
			Assert.AreEqual(string.Empty, error.ToString());
		}

		[Test]
		public void Compile_StackSize_CoversDeepestExpression()
		{
			Assert.IsTrue(FormulaCompiler.TryCompile("if(1, 2, 3)", out FormulaProgram program, out _));

			Assert.GreaterOrEqual(program.StackSize, 3);
		}

		[Test]
		public void Tokenize_TokenSpans_SliceTheSourceExactly()
		{
			const string expression = "clamp01({Max Hunger} <= 1.5) ";
			var tokens = new List<FormulaToken>();

			Assert.IsTrue(FormulaTokenizer.TryTokenize(expression, tokens, out _));

			Assert.AreEqual(6, tokens.Count);
			Assert.AreEqual("clamp01", Slice(expression, tokens[0]));
			Assert.AreEqual("(", Slice(expression, tokens[1]));
			Assert.AreEqual("{Max Hunger}", Slice(expression, tokens[2]));
			Assert.AreEqual("<=", Slice(expression, tokens[3]));
			Assert.AreEqual("1.5", Slice(expression, tokens[4]));
			Assert.AreEqual(")", Slice(expression, tokens[5]));
		}

		[Test]
		public void Tokenize_TokenSpans_StayInsideTheSource()
		{
			const string expression = "if(.5 <= a, -b ^ 2, min(1, {c d}))";
			var tokens = new List<FormulaToken>();

			Assert.IsTrue(FormulaTokenizer.TryTokenize(expression, tokens, out _));

			for (int i = 0; i < tokens.Count; i++)
			{
				FormulaToken token = tokens[i];
				Assert.GreaterOrEqual(token.Position, 0);
				Assert.GreaterOrEqual(token.Length, 1);
				Assert.LessOrEqual(token.Position + token.Length, expression.Length);
			}
		}

		[Test]
		public void Tokenize_PartialTokens_SurviveAnError()
		{
			var tokens = new List<FormulaToken>();

			Assert.IsFalse(FormulaTokenizer.TryTokenize("clamp01(a) # b", tokens, out _));

			Assert.AreEqual(4, tokens.Count);
			Assert.AreEqual("clamp01", Slice("clamp01(a) # b", tokens[0]));
		}

		private static float Evaluate(string expression) => Evaluate(expression, null);

		private static float Evaluate(string expression, Dictionary<string, float> variables)
		{
			Assert.IsTrue(
				FormulaCompiler.TryCompile(expression, out FormulaProgram program, out FormulaError error),
				$"'{expression}' failed to compile: {error}");

			var values = new float[program.VariableCount];
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = variables != null && variables.TryGetValue(program.VariableNames[i], out float value)
					? value
					: 0f;
			}

			return program.Evaluate(values, new float[program.StackSize]);
		}

		private static string Slice(string expression, in FormulaToken token) =>
			expression.Substring(token.Position, token.Length);

		private static FormulaError CompileError(string expression)
		{
			Assert.IsFalse(
				FormulaCompiler.TryCompile(expression, out _, out FormulaError error),
				$"'{expression}' compiled but was expected to fail");

			return error;
		}
	}
}
