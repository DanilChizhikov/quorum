using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTech.UtilityAI
{
	/// <summary>
	/// A compiled formula: a flat instruction list run over caller-owned buffers, so evaluating it
	/// allocates nothing. The program is immutable and holds no agent data, so one instance can be
	/// shared by every agent that uses the same formula.
	/// </summary>
	public sealed class UtilityFormulaProgram
	{
		private readonly UtilityFormulaInstruction[] _instructions;
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

		internal UtilityFormulaProgram(UtilityFormulaInstruction[] instructions, string[] variableNames, int stackSize)
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
				UtilityFormulaInstruction instruction = _instructions[i];
				switch (instruction.Code)
				{
					case UtilityFormulaOpCode.PushConstant:
					{
						stack[top++] = instruction.Value;
						break;
					}
					case UtilityFormulaOpCode.PushVariable:
					{
						stack[top++] = variables[instruction.VariableIndex];
						break;
					}
					case UtilityFormulaOpCode.Add:
					{
						float right = stack[--top];
						stack[top - 1] += right;
						break;
					}
					case UtilityFormulaOpCode.Subtract:
					{
						float right = stack[--top];
						stack[top - 1] -= right;
						break;
					}
					case UtilityFormulaOpCode.Multiply:
					{
						float right = stack[--top];
						stack[top - 1] *= right;
						break;
					}
					case UtilityFormulaOpCode.Divide:
					{
						float right = stack[--top];
						stack[top - 1] = right == 0f ? 0f : stack[top - 1] / right;
						break;
					}
					case UtilityFormulaOpCode.Modulo:
					{
						float right = stack[--top];
						stack[top - 1] = right == 0f ? 0f : stack[top - 1] % right;
						break;
					}
					case UtilityFormulaOpCode.Power:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Pow(stack[top - 1], right);
						break;
					}
					case UtilityFormulaOpCode.Negate:
					{
						stack[top - 1] = -stack[top - 1];
						break;
					}
					case UtilityFormulaOpCode.Not:
					{
						stack[top - 1] = stack[top - 1] == 0f ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.Less:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] < right ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.LessOrEqual:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] <= right ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.Greater:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] > right ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.GreaterOrEqual:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] >= right ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.Equal:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Approximately(stack[top - 1], right) ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.NotEqual:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Approximately(stack[top - 1], right) ? 0f : 1f;
						break;
					}
					case UtilityFormulaOpCode.And:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] != 0f && right != 0f ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.Or:
					{
						float right = stack[--top];
						stack[top - 1] = stack[top - 1] != 0f || right != 0f ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.Min:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Min(stack[top - 1], right);
						break;
					}
					case UtilityFormulaOpCode.Max:
					{
						float right = stack[--top];
						stack[top - 1] = Mathf.Max(stack[top - 1], right);
						break;
					}
					case UtilityFormulaOpCode.Step:
					{
						float value = stack[--top];
						stack[top - 1] = value >= stack[top - 1] ? 1f : 0f;
						break;
					}
					case UtilityFormulaOpCode.Clamp:
					{
						float max = stack[--top];
						float min = stack[--top];
						stack[top - 1] = Mathf.Clamp(stack[top - 1], min, max);
						break;
					}
					case UtilityFormulaOpCode.Clamp01:
					{
						stack[top - 1] = Mathf.Clamp01(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Absolute:
					{
						stack[top - 1] = Mathf.Abs(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.SquareRoot:
					{
						float value = stack[top - 1];
						stack[top - 1] = value < 0f ? 0f : Mathf.Sqrt(value);
						break;
					}
					case UtilityFormulaOpCode.Floor:
					{
						stack[top - 1] = Mathf.Floor(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Ceiling:
					{
						stack[top - 1] = Mathf.Ceil(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Round:
					{
						stack[top - 1] = Mathf.Round(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Sign:
					{
						stack[top - 1] = Mathf.Sign(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Exponent:
					{
						stack[top - 1] = Mathf.Exp(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Logarithm:
					{
						float value = stack[top - 1];
						stack[top - 1] = value <= 0f ? 0f : Mathf.Log(value);
						break;
					}
					case UtilityFormulaOpCode.Sine:
					{
						stack[top - 1] = Mathf.Sin(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Cosine:
					{
						stack[top - 1] = Mathf.Cos(stack[top - 1]);
						break;
					}
					case UtilityFormulaOpCode.Lerp:
					{
						float time = stack[--top];
						float to = stack[--top];
						stack[top - 1] = Mathf.Lerp(stack[top - 1], to, time);
						break;
					}
					case UtilityFormulaOpCode.InverseLerp:
					{
						float value = stack[--top];
						float to = stack[--top];
						stack[top - 1] = Mathf.InverseLerp(stack[top - 1], to, value);
						break;
					}
					case UtilityFormulaOpCode.SmoothStep:
					{
						float time = stack[--top];
						float to = stack[--top];
						stack[top - 1] = Mathf.SmoothStep(stack[top - 1], to, time);
						break;
					}
					case UtilityFormulaOpCode.Condition:
					{
						float otherwise = stack[--top];
						float then = stack[--top];
						stack[top - 1] = stack[top - 1] != 0f ? then : otherwise;
						break;
					}
					default:
					{
						throw new ArgumentOutOfRangeException(nameof(instruction), instruction.Code, null);
					}
				}
			}

			return top > 0 ? stack[0] : 0f;
		}
	}
}
