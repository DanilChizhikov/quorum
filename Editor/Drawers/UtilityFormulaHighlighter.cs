using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityFormulaHighlighter
	{
		private const int UnmatchedBracketDepth = -1;

		private static readonly List<int> _openBrackets = new();
		private static readonly HashSet<int> _unmatchedBrackets = new();
		private static readonly GUIContent _measuredText = new();

		public static UtilityFormulaHighlightSpan[] Build(string expression, List<UtilityFormulaToken> tokens)
		{
			if (string.IsNullOrEmpty(expression) || tokens == null || tokens.Count <= 0)
			{
				return Array.Empty<UtilityFormulaHighlightSpan>();
			}

			MarkUnmatchedBrackets(tokens);
			var spans = new UtilityFormulaHighlightSpan[tokens.Count];
			int depth = 0;
			for (int i = 0; i < tokens.Count; i++)
			{
				UtilityFormulaToken token = tokens[i];
				UtilityFormulaTokenRole role = GetRole(tokens, i);
				spans[i] = new UtilityFormulaHighlightSpan(
					expression.Substring(token.Position, token.Length),
					role == UtilityFormulaTokenRole.Variable ? token.Text : null,
					token.Position,
					role,
					GetBracketDepth(token, i, ref depth));
			}

			return spans;
		}

		public static void Draw(in UtilityFormulaHighlightRequest request)
		{
			UtilityFormulaHighlightSpan[] spans = request.Spans;
			if (spans == null || spans.Length <= 0)
			{
				return;
			}

			Rect position = request.Position;
			GUIStyle fieldStyle = UtilityFormulaStyles.FieldStyle;
			GUIStyle textStyle = UtilityFormulaStyles.TextStyle;
			_measuredText.text = request.Expression ?? string.Empty;
			Color previousColor = GUI.contentColor;
			for (int i = 0; i < spans.Length; i++)
			{
				UtilityFormulaHighlightSpan span = spans[i];
				float x = fieldStyle.GetCursorPixelPosition(position, _measuredText, span.Position).x;
				if (x >= position.xMax)
				{
					break;
				}

				float end = fieldStyle.GetCursorPixelPosition(position, _measuredText, span.Position + span.Text.Length).x;
				float width = Mathf.Min(Mathf.Max(0f, end - x), position.xMax - x);
				if (width <= 0f)
				{
					continue;
				}

				GUI.contentColor = GetColor(span, request.Schema);
				GUI.Label(new Rect(x, position.y, width, position.height), span.Text, textStyle);
			}

			GUI.contentColor = previousColor;
			_measuredText.text = string.Empty;
		}

		private static void MarkUnmatchedBrackets(List<UtilityFormulaToken> tokens)
		{
			_openBrackets.Clear();
			_unmatchedBrackets.Clear();
			for (int i = 0; i < tokens.Count; i++)
			{
				UtilityFormulaTokenType type = tokens[i].Type;
				if (type == UtilityFormulaTokenType.LeftParenthesis)
				{
					_openBrackets.Add(i);
					continue;
				}

				if (type != UtilityFormulaTokenType.RightParenthesis)
				{
					continue;
				}

				if (_openBrackets.Count <= 0)
				{
					_unmatchedBrackets.Add(i);
					continue;
				}

				_openBrackets.RemoveAt(_openBrackets.Count - 1);
			}

			for (int i = 0; i < _openBrackets.Count; i++)
			{
				_unmatchedBrackets.Add(_openBrackets[i]);
			}
		}

		private static UtilityFormulaTokenRole GetRole(List<UtilityFormulaToken> tokens, int index)
		{
			UtilityFormulaToken token = tokens[index];
			switch (token.Type)
			{
				case UtilityFormulaTokenType.Number:
				{
					return UtilityFormulaTokenRole.Number;
				}
				case UtilityFormulaTokenType.Name:
				{
					return UtilityFormulaTokenRole.Variable;
				}
				case UtilityFormulaTokenType.LeftParenthesis:
				case UtilityFormulaTokenType.RightParenthesis:
				{
					return UtilityFormulaTokenRole.Bracket;
				}
				case UtilityFormulaTokenType.Identifier:
				{
					return GetIdentifierRole(tokens, index);
				}
				default:
				{
					return UtilityFormulaTokenRole.Operator;
				}
			}
		}

		private static UtilityFormulaTokenRole GetIdentifierRole(List<UtilityFormulaToken> tokens, int index)
		{
			string text = tokens[index].Text;
			bool isCall = index + 1 < tokens.Count &&
						tokens[index + 1].Type == UtilityFormulaTokenType.LeftParenthesis;

			if (isCall && UtilityFormulaFunctions.TryGetFunction(text, out _))
			{
				return UtilityFormulaTokenRole.Function;
			}

			return UtilityFormulaFunctions.TryGetConstant(text, out _)
				? UtilityFormulaTokenRole.Constant
				: UtilityFormulaTokenRole.Variable;
		}

		private static int GetBracketDepth(in UtilityFormulaToken token, int index, ref int depth)
		{
			if (token.Type == UtilityFormulaTokenType.LeftParenthesis)
			{
				if (_unmatchedBrackets.Contains(index))
				{
					depth++;
					return UnmatchedBracketDepth;
				}

				int result = depth;
				depth++;
				return result;
			}

			if (token.Type != UtilityFormulaTokenType.RightParenthesis)
			{
				return 0;
			}

			if (_unmatchedBrackets.Contains(index))
			{
				return UnmatchedBracketDepth;
			}

			depth = Mathf.Max(0, depth - 1);
			return depth;
		}

		private static Color GetColor(in UtilityFormulaHighlightSpan span, Blackboard.BlackboardAsset schema)
		{
			switch (span.Role)
			{
				case UtilityFormulaTokenRole.Number:
				{
					return UtilityFormulaStyles.Number;
				}
				case UtilityFormulaTokenRole.Function:
				{
					return UtilityFormulaStyles.Function;
				}
				case UtilityFormulaTokenRole.Constant:
				{
					return UtilityFormulaStyles.Constant;
				}
				case UtilityFormulaTokenRole.Bracket:
				{
					return span.BracketDepth < 0
						? UtilityFormulaStyles.Invalid
						: UtilityFormulaStyles.GetBracketColor(span.BracketDepth);
				}
				case UtilityFormulaTokenRole.Variable:
				{
					return UtilityFormulaSchema.IsKnown(schema, span.VariableName)
						? UtilityFormulaStyles.Variable
						: UtilityFormulaStyles.Warning;
				}
				default:
				{
					return UtilityFormulaStyles.Operator;
				}
			}
		}
	}
}
