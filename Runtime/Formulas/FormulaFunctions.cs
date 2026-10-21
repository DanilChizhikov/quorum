using System;
using System.Collections.Generic;

namespace DTech.Quorum
{
	internal static class FormulaFunctions
	{
		private static readonly Dictionary<string, FormulaFunction> _functions =
			new(StringComparer.OrdinalIgnoreCase)
			{
				{ "min", new FormulaFunction("min", 2, FormulaOpCode.Min) },
				{ "max", new FormulaFunction("max", 2, FormulaOpCode.Max) },
				{ "pow", new FormulaFunction("pow", 2, FormulaOpCode.Power) },
				{ "step", new FormulaFunction("step", 2, FormulaOpCode.Step) },
				{ "clamp", new FormulaFunction("clamp", 3, FormulaOpCode.Clamp) },
				{ "clamp01", new FormulaFunction("clamp01", 1, FormulaOpCode.Clamp01) },
				{ "abs", new FormulaFunction("abs", 1, FormulaOpCode.Absolute) },
				{ "sqrt", new FormulaFunction("sqrt", 1, FormulaOpCode.SquareRoot) },
				{ "floor", new FormulaFunction("floor", 1, FormulaOpCode.Floor) },
				{ "ceil", new FormulaFunction("ceil", 1, FormulaOpCode.Ceiling) },
				{ "round", new FormulaFunction("round", 1, FormulaOpCode.Round) },
				{ "sign", new FormulaFunction("sign", 1, FormulaOpCode.Sign) },
				{ "exp", new FormulaFunction("exp", 1, FormulaOpCode.Exponent) },
				{ "log", new FormulaFunction("log", 1, FormulaOpCode.Logarithm) },
				{ "sin", new FormulaFunction("sin", 1, FormulaOpCode.Sine) },
				{ "cos", new FormulaFunction("cos", 1, FormulaOpCode.Cosine) },
				{ "lerp", new FormulaFunction("lerp", 3, FormulaOpCode.Lerp) },
				{ "inverselerp", new FormulaFunction("inverseLerp", 3, FormulaOpCode.InverseLerp) },
				{ "smoothstep", new FormulaFunction("smoothstep", 3, FormulaOpCode.SmoothStep) },
				{ "if", new FormulaFunction("if", 3, FormulaOpCode.Condition) },
			};

		private static readonly Dictionary<string, float> _constants =
			new(StringComparer.OrdinalIgnoreCase)
			{
				{ "pi", 3.14159274f },
				{ "e", 2.71828175f },
			};

		public static bool TryGetFunction(string name, out FormulaFunction function)
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
