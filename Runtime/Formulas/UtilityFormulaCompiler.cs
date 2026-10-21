using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	/// <summary>
	/// Turns a formula written by hand into a program the evaluator can run without allocating.
	/// Compiling is not cheap enough for a per-frame call: do it once and keep the program.
	/// </summary>
	public static class UtilityFormulaCompiler
	{
		private const int OrPrecedence = 1;
		private const int AndPrecedence = 2;
		private const int EqualityPrecedence = 3;
		private const int ComparisonPrecedence = 4;
		private const int AdditivePrecedence = 5;
		private const int MultiplicativePrecedence = 6;
		private const int UnaryPrecedence = 7;
		private const int PowerPrecedence = 8;

		private const string ExpectedValueMessage = "Expected a value";
		private const string ExpectedOperatorMessage = "Expected an operator";
		private const string UnmatchedCloseMessage = "Unmatched ')'";
		private const string ExpectedCloseMessage = "Expected ')'";
		private const string UnexpectedCommaMessage = "Unexpected ','";
		private const string MalformedMessage = "Formula is malformed";
		private const string SingleValueMessage = "Formula does not produce a single value";

		/// <summary>
		/// Compiles a formula.
		/// </summary>
		/// <param name="expression">Formula text as a designer typed it.</param>
		/// <param name="program">Compiled program, or null when the formula is invalid.</param>
		/// <param name="error">Why the formula is invalid, or the default value when it compiled.</param>
		/// <returns>TRUE when the formula compiled, FALSE otherwise.</returns>
		public static bool TryCompile(string expression, out UtilityFormulaProgram program, out UtilityFormulaError error)
		{
			program = null;
			var tokens = new List<UtilityFormulaToken>();
			if (!UtilityFormulaTokenizer.TryTokenize(expression, tokens, out error))
			{
				return false;
			}

			var state = new CompileState(tokens.Count);
			for (int i = 0; i < tokens.Count; i++)
			{
				if (!TryConsume(tokens, ref i, state, out error))
				{
					return false;
				}
			}

			if (state.ExpectOperand)
			{
				error = new UtilityFormulaError(ExpectedValueMessage, expression.Length);
				return false;
			}

			while (state.Frames.Count > 0)
			{
				Frame frame = state.Frames.Pop();
				if (frame.Kind != FrameKind.Operator)
				{
					error = new UtilityFormulaError(ExpectedCloseMessage, expression.Length);
					return false;
				}

				if (!TryEmitOperation(state, frame, out error))
				{
					return false;
				}
			}

			if (state.Depth != 1)
			{
				error = new UtilityFormulaError(SingleValueMessage, 0);
				return false;
			}

			program = new UtilityFormulaProgram(
				state.Instructions.ToArray(),
				state.VariableNames.ToArray(),
				state.MaxDepth);

			return true;
		}

		private static bool TryConsume(List<UtilityFormulaToken> tokens, ref int index, CompileState state, out UtilityFormulaError error)
		{
			UtilityFormulaToken token = tokens[index];
			switch (token.Type)
			{
				case UtilityFormulaTokenType.Number:
				{
					return TryPushValue(state, UtilityFormulaInstruction.Constant(token.Value), token.Position, out error);
				}
				case UtilityFormulaTokenType.Name:
				{
					int slot = state.GetVariableSlot(token.Text);
					return TryPushValue(state, UtilityFormulaInstruction.Variable(slot), token.Position, out error);
				}
				case UtilityFormulaTokenType.Identifier:
				{
					return TryConsumeIdentifier(tokens, ref index, state, out error);
				}
				case UtilityFormulaTokenType.Operator:
				{
					return TryConsumeOperator(state, token, out error);
				}
				case UtilityFormulaTokenType.LeftParenthesis:
				{
					if (!state.ExpectOperand)
					{
						error = new UtilityFormulaError(ExpectedOperatorMessage, token.Position);
						return false;
					}

					state.Frames.Push(Frame.Parenthesis(token.Position));
					error = default;
					return true;
				}
				case UtilityFormulaTokenType.RightParenthesis:
				{
					return TryCloseParenthesis(state, token, out error);
				}
				case UtilityFormulaTokenType.Comma:
				{
					return TryConsumeComma(state, token, out error);
				}
				default:
				{
					throw new ArgumentOutOfRangeException(nameof(token), token.Type, null);
				}
			}
		}

		private static bool TryPushValue(CompileState state, UtilityFormulaInstruction instruction, int position, out UtilityFormulaError error)
		{
			if (!state.ExpectOperand)
			{
				error = new UtilityFormulaError(ExpectedOperatorMessage, position);
				return false;
			}

			state.Emit(instruction, 0);
			state.ExpectOperand = false;
			error = default;
			return true;
		}

		private static bool TryConsumeIdentifier(List<UtilityFormulaToken> tokens, ref int index, CompileState state, out UtilityFormulaError error)
		{
			UtilityFormulaToken token = tokens[index];
			bool isCall = index + 1 < tokens.Count &&
						tokens[index + 1].Type == UtilityFormulaTokenType.LeftParenthesis;

			if (!isCall)
			{
				if (UtilityFormulaFunctions.TryGetConstant(token.Text, out float constant))
				{
					return TryPushValue(state, UtilityFormulaInstruction.Constant(constant), token.Position, out error);
				}

				int slot = state.GetVariableSlot(token.Text);
				return TryPushValue(state, UtilityFormulaInstruction.Variable(slot), token.Position, out error);
			}

			if (!state.ExpectOperand)
			{
				error = new UtilityFormulaError(ExpectedOperatorMessage, token.Position);
				return false;
			}

			if (!UtilityFormulaFunctions.TryGetFunction(token.Text, out UtilityFormulaFunction function))
			{
				error = new UtilityFormulaError($"Unknown function '{token.Text}'", token.Position);
				return false;
			}

			state.Frames.Push(Frame.Function(function, token.Position));
			state.ExpectOperand = true;
			index++;
			error = default;
			return true;
		}

		private static bool TryConsumeOperator(CompileState state, UtilityFormulaToken token, out UtilityFormulaError error)
		{
			error = default;
			if (state.ExpectOperand)
			{
				if (string.Equals(token.Text, "+", StringComparison.Ordinal))
				{
					return true;
				}

				if (!TryGetUnary(token.Text, out UtilityFormulaOpCode unaryCode))
				{
					error = new UtilityFormulaError(ExpectedValueMessage, token.Position);
					return false;
				}

				state.Frames.Push(Frame.Operator(unaryCode, UnaryPrecedence, true, 1, token.Position));
				return true;
			}

			if (!TryGetBinary(token.Text, out UtilityFormulaOpCode code, out int precedence, out bool rightAssociative))
			{
				error = new UtilityFormulaError($"Operator '{token.Text}' needs a value on its left", token.Position);
				return false;
			}

			while (state.Frames.Count > 0)
			{
				Frame top = state.Frames.Peek();
				if (top.Kind != FrameKind.Operator || top.Precedence < precedence)
				{
					break;
				}

				if (top.Precedence == precedence && rightAssociative)
				{
					break;
				}

				state.Frames.Pop();
				if (!TryEmitOperation(state, top, out error))
				{
					return false;
				}
			}

			state.Frames.Push(Frame.Operator(code, precedence, rightAssociative, 2, token.Position));
			state.ExpectOperand = true;
			return true;
		}

		private static bool TryCloseParenthesis(CompileState state, UtilityFormulaToken token, out UtilityFormulaError error)
		{
			error = default;
			if (state.ExpectOperand)
			{
				error = new UtilityFormulaError(ExpectedValueMessage, token.Position);
				return false;
			}

			while (state.Frames.Count > 0)
			{
				Frame frame = state.Frames.Pop();
				if (frame.Kind == FrameKind.Operator)
				{
					if (!TryEmitOperation(state, frame, out error))
					{
						return false;
					}

					continue;
				}

				if (frame.Kind == FrameKind.Function)
				{
					if (frame.ArgumentCount != frame.Arity)
					{
						error = GetArityError(frame, frame.ArgumentCount);
						return false;
					}

					if (!TryEmitOperation(state, frame, out error))
					{
						return false;
					}
				}

				state.ExpectOperand = false;
				return true;
			}

			error = new UtilityFormulaError(UnmatchedCloseMessage, token.Position);
			return false;
		}

		private static bool TryConsumeComma(CompileState state, UtilityFormulaToken token, out UtilityFormulaError error)
		{
			error = default;
			if (state.ExpectOperand)
			{
				error = new UtilityFormulaError(ExpectedValueMessage, token.Position);
				return false;
			}

			while (state.Frames.Count > 0)
			{
				Frame top = state.Frames.Peek();
				if (top.Kind == FrameKind.Operator)
				{
					state.Frames.Pop();
					if (!TryEmitOperation(state, top, out error))
					{
						return false;
					}

					continue;
				}

				if (top.Kind != FrameKind.Function)
				{
					break;
				}

				state.Frames.Pop();
				top.ArgumentCount++;
				if (top.ArgumentCount > top.Arity)
				{
					error = GetArityError(top, top.ArgumentCount);
					return false;
				}

				state.Frames.Push(top);
				state.ExpectOperand = true;
				return true;
			}

			error = new UtilityFormulaError(UnexpectedCommaMessage, token.Position);
			return false;
		}

		private static bool TryEmitOperation(CompileState state, in Frame frame, out UtilityFormulaError error)
		{
			if (state.Depth < frame.Arity)
			{
				error = new UtilityFormulaError(MalformedMessage, frame.Position);
				return false;
			}

			state.Emit(UtilityFormulaInstruction.Operation(frame.Code), frame.Arity);
			error = default;
			return true;
		}

		private static UtilityFormulaError GetArityError(in Frame frame, int argumentCount) =>
			new($"'{frame.Name}' takes {frame.Arity} argument(s), got {argumentCount}", frame.Position);

		private static bool TryGetUnary(string text, out UtilityFormulaOpCode code)
		{
			switch (text)
			{
				case "-":
				{
					code = UtilityFormulaOpCode.Negate;
					return true;
				}
				case "!":
				{
					code = UtilityFormulaOpCode.Not;
					return true;
				}
				default:
				{
					code = default;
					return false;
				}
			}
		}

		private static bool TryGetBinary(string text, out UtilityFormulaOpCode code, out int precedence, out bool rightAssociative)
		{
			rightAssociative = false;
			switch (text)
			{
				case "||":
				{
					code = UtilityFormulaOpCode.Or;
					precedence = OrPrecedence;
					return true;
				}
				case "&&":
				{
					code = UtilityFormulaOpCode.And;
					precedence = AndPrecedence;
					return true;
				}
				case "==":
				{
					code = UtilityFormulaOpCode.Equal;
					precedence = EqualityPrecedence;
					return true;
				}
				case "!=":
				{
					code = UtilityFormulaOpCode.NotEqual;
					precedence = EqualityPrecedence;
					return true;
				}
				case "<":
				{
					code = UtilityFormulaOpCode.Less;
					precedence = ComparisonPrecedence;
					return true;
				}
				case "<=":
				{
					code = UtilityFormulaOpCode.LessOrEqual;
					precedence = ComparisonPrecedence;
					return true;
				}
				case ">":
				{
					code = UtilityFormulaOpCode.Greater;
					precedence = ComparisonPrecedence;
					return true;
				}
				case ">=":
				{
					code = UtilityFormulaOpCode.GreaterOrEqual;
					precedence = ComparisonPrecedence;
					return true;
				}
				case "+":
				{
					code = UtilityFormulaOpCode.Add;
					precedence = AdditivePrecedence;
					return true;
				}
				case "-":
				{
					code = UtilityFormulaOpCode.Subtract;
					precedence = AdditivePrecedence;
					return true;
				}
				case "*":
				{
					code = UtilityFormulaOpCode.Multiply;
					precedence = MultiplicativePrecedence;
					return true;
				}
				case "/":
				{
					code = UtilityFormulaOpCode.Divide;
					precedence = MultiplicativePrecedence;
					return true;
				}
				case "%":
				{
					code = UtilityFormulaOpCode.Modulo;
					precedence = MultiplicativePrecedence;
					return true;
				}
				case "^":
				{
					code = UtilityFormulaOpCode.Power;
					precedence = PowerPrecedence;
					rightAssociative = true;
					return true;
				}
				default:
				{
					code = default;
					precedence = 0;
					return false;
				}
			}
		}

		private enum FrameKind : byte
		{
			Operator = 0,
			Parenthesis = 1,
			Function = 2,
		}

		private struct Frame
		{
			public FrameKind Kind;
			public UtilityFormulaOpCode Code;
			public string Name;
			public int Precedence;
			public bool RightAssociative;
			public int Arity;
			public int ArgumentCount;
			public int Position;

			public static Frame Parenthesis(int position) =>
				new() { Kind = FrameKind.Parenthesis, Position = position };

			public static Frame Function(in UtilityFormulaFunction function, int position) =>
				new()
				{
					Kind = FrameKind.Function,
					Code = function.Code,
					Name = function.Name,
					Arity = function.Arity,
					ArgumentCount = 1,
					Position = position,
				};

			public static Frame Operator(UtilityFormulaOpCode code, int precedence, bool rightAssociative, int arity, int position) =>
				new()
				{
					Kind = FrameKind.Operator,
					Code = code,
					Precedence = precedence,
					RightAssociative = rightAssociative,
					Arity = arity,
					Position = position,
				};
		}

		private sealed class CompileState
		{
			private readonly Dictionary<string, int> _variableIndices = new(StringComparer.OrdinalIgnoreCase);

			public readonly List<UtilityFormulaInstruction> Instructions;
			public readonly Stack<Frame> Frames = new();
			public readonly List<string> VariableNames = new();

			public bool ExpectOperand = true;
			public int Depth;
			public int MaxDepth;

			public CompileState(int capacity)
			{
				Instructions = new List<UtilityFormulaInstruction>(capacity);
			}

			public int GetVariableSlot(string name)
			{
				string key = name ?? string.Empty;
				if (_variableIndices.TryGetValue(key, out int slot))
				{
					return slot;
				}

				slot = VariableNames.Count;
				VariableNames.Add(key);
				_variableIndices[key] = slot;
				return slot;
			}

			public void Emit(UtilityFormulaInstruction instruction, int arity)
			{
				Instructions.Add(instruction);
				Depth += 1 - arity;
				if (Depth > MaxDepth)
				{
					MaxDepth = Depth;
				}
			}
		}
	}
}
