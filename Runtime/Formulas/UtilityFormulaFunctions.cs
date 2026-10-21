using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	internal static class UtilityFormulaFunctions
	{
		private static readonly Dictionary<string, UtilityFormulaFunction> _functions =
			new(StringComparer.OrdinalIgnoreCase)
			{
				{ "min", new UtilityFormulaFunction("min", 2, UtilityFormulaOpCode.Min) },
				{ "max", new UtilityFormulaFunction("max", 2, UtilityFormulaOpCode.Max) },
				{ "pow", new UtilityFormulaFunction("pow", 2, UtilityFormulaOpCode.Power) },
				{ "step", new UtilityFormulaFunction("step", 2, UtilityFormulaOpCode.Step) },
				{ "clamp", new UtilityFormulaFunction("clamp", 3, UtilityFormulaOpCode.Clamp) },
				{ "clamp01", new UtilityFormulaFunction("clamp01", 1, UtilityFormulaOpCode.Clamp01) },
				{ "abs", new UtilityFormulaFunction("abs", 1, UtilityFormulaOpCode.Absolute) },
				{ "sqrt", new UtilityFormulaFunction("sqrt", 1, UtilityFormulaOpCode.SquareRoot) },
				{ "floor", new UtilityFormulaFunction("floor", 1, UtilityFormulaOpCode.Floor) },
				{ "ceil", new UtilityFormulaFunction("ceil", 1, UtilityFormulaOpCode.Ceiling) },
				{ "round", new UtilityFormulaFunction("round", 1, UtilityFormulaOpCode.Round) },
				{ "sign", new UtilityFormulaFunction("sign", 1, UtilityFormulaOpCode.Sign) },
				{ "exp", new UtilityFormulaFunction("exp", 1, UtilityFormulaOpCode.Exponent) },
				{ "log", new UtilityFormulaFunction("log", 1, UtilityFormulaOpCode.Logarithm) },
				{ "sin", new UtilityFormulaFunction("sin", 1, UtilityFormulaOpCode.Sine) },
				{ "cos", new UtilityFormulaFunction("cos", 1, UtilityFormulaOpCode.Cosine) },
				{ "lerp", new UtilityFormulaFunction("lerp", 3, UtilityFormulaOpCode.Lerp) },
				{ "inverselerp", new UtilityFormulaFunction("inverseLerp", 3, UtilityFormulaOpCode.InverseLerp) },
				{ "smoothstep", new UtilityFormulaFunction("smoothstep", 3, UtilityFormulaOpCode.SmoothStep) },
				{ "if", new UtilityFormulaFunction("if", 3, UtilityFormulaOpCode.Condition) },
			};

		private static readonly Dictionary<string, float> _constants =
			new(StringComparer.OrdinalIgnoreCase)
			{
				{ "pi", 3.14159274f },
				{ "e", 2.71828175f },
			};

		public static bool TryGetFunction(string name, out UtilityFormulaFunction function)
		{
			if (string.IsNullOrEmpty(name))
			{
				function = default;
				return false;
			}

			return _functions.TryGetValue(name, out function);
		}

		public static bool TryGetConstant(string name, out float value)
		{
			if (string.IsNullOrEmpty(name))
			{
				value = 0f;
				return false;
			}

			return _constants.TryGetValue(name, out value);
		}
	}
}
