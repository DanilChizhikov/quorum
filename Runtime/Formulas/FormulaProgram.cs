using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTech.Quorum
{
	/// <summary>
	/// A compiled formula: a flat instruction list run over caller-owned buffers, so evaluating it
	/// allocates nothing. The program is immutable and holds no agent data, so one instance can be
	/// shared by every agent that uses the same formula.
	/// </summary>
	public sealed class FormulaProgram
	{
		private readonly FormulaInstruction[] _instructions;
		private readonly string[] _variableNames;

		/// <summary>
		/// Gets how many distinct variables the formula reads.
		/// </summary>
		public int VariableCount => _variableNames.Length;

		/// <summary>
		/// Gets how many values the formula needs on the evaluation stack at most.
		/// </summary>
		public int StackSize { get; }

		/// <summary>
		/// Gets the variable names in slot order, which is the order Evaluate reads its values array in.
		/// </summary>
		public IReadOnlyList<string> VariableNames => _variableNames;

		internal FormulaProgram(FormulaInstruction[] instructions, string[] variableNames, int stackSize)
		{
			_instructions = instructions;
			_variableNames = variableNames;
			StackSize = Mathf.Max(1, stackSize);
		}

		/// <summary>
		/// Runs the formula. Division, modulo, square root and logarithm are total: an input they
		/// have no answer for yields 0 instead of an infinity or a NaN.
		/// </summary>
		/// <param name="variables">Variable values in slot order, at least VariableCount long.</param>
		/// <param name="stack">Scratch buffer, at least StackSize long. Its contents are overwritten.</param>
		/// <returns>Result of the formula.</returns>
		public float Evaluate(float[] variables, float[] stack)
		{
			int top = 0;
			for (int i = 0; i < _instructions.Length; i++)
			{
				FormulaInstruction instruction = _instructions[i];
				switch (instruction.Code)
				{
					case FormulaOpCode.PushConstant:
					{
						stack[top++] = instruction.Value;
						break;
					}
					case FormulaOpCode.PushVariable:
					{
						stack[top++] = variables[instruction.VariableIndex];
						break;
					}
					case FormulaOpCode.Add:
					{
						float right = stack[--top];
						stack[top - 1] += right;
						break;
					}
					case FormulaOpCode.Subtract:
					{
						float right = stack[--top];
						stack[top - 1] -= right;
						break;
					}
					case FormulaOpCode.Multiply:
					{
						float right = stack[--top];
						stack[top - 1] *= right;
						break;
					}
					case FormulaOpCode.Divide:
					{
						float right = stack[--top];
						stack[top - 1] = right == 0f ? 0f : stack[top - 1] / right;
						break;
					}
					case FormulaOpCode.Modulo:
					{
						float right = stack[--top];
						stack[top - 1] = right == 0f ? 0f : stack[top - 1] % right;
						break;
					}
					case FormulaOpCode.Power:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Pow(stack[top - 1], right);
						break;
					}
					case FormulaOpCode.Negate:
					{
						stack[top - 1] = -stack[top - 1];
						break;
					}
					case FormulaOpCode.Not:
					{
						stack[top - 1] = stack[top - 1] == 0f ? 1f : 0f;
						break;
					}
					case FormulaOpCode.Less:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] < right ? 1f : 0f;
						break;
					}
					case FormulaOpCode.LessOrEqual:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] <= right ? 1f : 0f;
						break;
					}
					case FormulaOpCode.Greater:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] > right ? 1f : 0f;
						break;
					}
					case FormulaOpCode.GreaterOrEqual:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] >= right ? 1f : 0f;
						break;
					}
					case FormulaOpCode.Equal:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Approximately(stack[top - 1], right) ? 1f : 0f;
						break;
					}
					case FormulaOpCode.NotEqual:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Approximately(stack[top - 1], right) ? 0f : 1f;
						break;
					}
					case FormulaOpCode.And:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] != 0f && right != 0f ? 1f : 0f;
						break;
					}
					case FormulaOpCode.Or:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] != 0f || right != 0f ? 1f : 0f;
						break;
					}
					case FormulaOpCode.Min:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Min(stack[top - 1], right);
						break;
					}
					case FormulaOpCode.Max:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Max(stack[top - 1], right);
						break;
					}
					case FormulaOpCode.Step:
					{
						float value = stack[--top];
						stack[top - 1] = value >= stack[top - 1] ? 1f : 0f;
						break;
					}
					case FormulaOpCode.Clamp:
					{
						float max = stack[--top];
						float min = stack[--top];
						stack[top - 1] = Mathf.Clamp(stack[top - 1], min, max);
						break;
					}
					case FormulaOpCode.Clamp01:
					{
						stack[top - 1] = Mathf.Clamp01(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Absolute:
					{
						stack[top - 1] = Mathf.Abs(stack[top - 1]);
						break;
					}
					case FormulaOpCode.SquareRoot:
					{
						float value = stack[top - 1];
						stack[top - 1] = value < 0f ? 0f : Mathf.Sqrt(value);
						break;
					}
					case FormulaOpCode.Floor:
					{
						stack[top - 1] = Mathf.Floor(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Ceiling:
					{
						stack[top - 1] = Mathf.Ceil(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Round:
					{
						stack[top - 1] = Mathf.Round(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Sign:
					{
						stack[top - 1] = Mathf.Sign(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Exponent:
					{
						stack[top - 1] = Mathf.Exp(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Logarithm:
					{
						float value = stack[top - 1];
						stack[top - 1] = value <= 0f ? 0f : Mathf.Log(value);
						break;
					}
					case FormulaOpCode.Sine:
					{
						stack[top - 1] = Mathf.Sin(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Cosine:
					{
						stack[top - 1] = Mathf.Cos(stack[top - 1]);
						break;
					}
					case FormulaOpCode.Lerp:
					{
						float time = stack[--top];
						float to = stack[--top];
						stack[top - 1] = Mathf.Lerp(stack[top - 1], to, time);
						break;
					}
					case FormulaOpCode.InverseLerp:
					{
						float value = stack[--top];
						float to = stack[--top];
						stack[top - 1] = Mathf.InverseLerp(stack[top - 1], to, value);
						break;
					}
					case FormulaOpCode.SmoothStep:
					{
						float time = stack[--top];
						float to = stack[--top];
						stack[top - 1] = Mathf.SmoothStep(stack[top - 1], to, time);
						break;
					}
					case FormulaOpCode.Condition:
					{
						float otherwise = stack[--top];
						float then = stack[--top];
						stack[top - 1] = stack[top - 1] != 0f ? then : otherwise;
						break;
					}
					default:
					{
						throw new ArgumentOutOfRangeException(nameof(instruction), instruction.Code, "Unknown formula op code.");
					}
				}
			}

			return top > 0 ? stack[0] : 0f;
		}
	}
}
