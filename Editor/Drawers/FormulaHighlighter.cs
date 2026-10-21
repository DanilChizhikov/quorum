using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal static class FormulaHighlighter
	{
		private const int UnmatchedBracketDepth = -1;

		private static readonly List<int> _openBrackets = new();
		private static readonly HashSet<int> _unmatchedBrackets = new();
		private static readonly GUIContent _measuredText = new();

		public static FormulaHighlightSpan[] Build(string expression, List<FormulaToken> tokens)
		{
			if (string.IsNullOrEmpty(expression) || tokens == null || tokens.Count <= 0)
			{
				return Array.Empty<FormulaHighlightSpan>();
			}

			MarkUnmatchedBrackets(tokens);
			var spans = new FormulaHighlightSpan[tokens.Count];
			int depth = 0;
			for (int i = 0; i < tokens.Count; i++)
			{
				FormulaToken token = tokens[i];
				FormulaTokenRole role = GetRole(tokens, i);
				spans[i] = new FormulaHighlightSpan(
					expression.Substring(token.Position, token.Length),
					role == FormulaTokenRole.Variable ? token.Text : null,
					token.Position,
					role,
					GetBracketDepth(token, i, ref depth));
			}

			return spans;
		}

		public static void Draw(in FormulaHighlightRequest request)
		{
			FormulaHighlightSpan[] spans = request.Spans;
			if (spans == null || spans.Length <= 0)
			{
				return;
			}

			Rect position = request.Position;
			GUIStyle fieldStyle = FormulaStyles.FieldStyle;
			GUIStyle textStyle = FormulaStyles.TextStyle;
			_measuredText.text = request.Expression ?? string.Empty;
			Color previousColor = GUI.contentColor;
			for (int i = 0; i < spans.Length; i++)
			{
				FormulaHighlightSpan span = spans[i];
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

		private static void MarkUnmatchedBrackets(List<FormulaToken> tokens)
		{
			_openBrackets.Clear();
			_unmatchedBrackets.Clear();
			for (int i = 0; i < tokens.Count; i++)
			{
				FormulaTokenType type = tokens[i].Type;
				if (type == FormulaTokenType.LeftParenthesis)
				{
					_openBrackets.Add(i);
					continue;
				}

				if (type != FormulaTokenType.RightParenthesis)
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

		private static FormulaTokenRole GetRole(List<FormulaToken> tokens, int index)
		{
			FormulaToken token = tokens[index];
			switch (token.Type)
			{
				case FormulaTokenType.Number:
				{
					return FormulaTokenRole.Number;
				}
				case FormulaTokenType.Name:
				{
					return FormulaTokenRole.Variable;
				}
				case FormulaTokenType.LeftParenthesis:
				case FormulaTokenType.RightParenthesis:
				{
					return FormulaTokenRole.Bracket;
				}
				case FormulaTokenType.Identifier:
				{
					return GetIdentifierRole(tokens, index);
				}
				default:
				{
					return FormulaTokenRole.Operator;
				}
			}
		}

		private static FormulaTokenRole GetIdentifierRole(List<FormulaToken> tokens, int index)
		{
			string text = tokens[index].Text;
			bool isCall = index + 1 < tokens.Count &&
						tokens[index + 1].Type == FormulaTokenType.LeftParenthesis;

			if (isCall && FormulaFunctions.TryGetFunction(text, out _))
			{
				return FormulaTokenRole.Function;
			}

			return FormulaFunctions.TryGetConstant(text, out _)
				? FormulaTokenRole.Constant
				: FormulaTokenRole.Variable;
		}

		private static int GetBracketDepth(in FormulaToken token, int index, ref int depth)
		{
			if (token.Type == FormulaTokenType.LeftParenthesis)
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

			if (token.Type != FormulaTokenType.RightParenthesis)
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

		private static Color GetColor(in FormulaHighlightSpan span, Blackboard.BlackboardAsset schema)
		{
			switch (span.Role)
			{
				case FormulaTokenRole.Number:
				{
					return FormulaStyles.Number;
				}
				case FormulaTokenRole.Function:
				{
					return FormulaStyles.Function;
				}
				case FormulaTokenRole.Constant:
				{
					return FormulaStyles.Constant;
				}
				case FormulaTokenRole.Bracket:
				{
					return span.BracketDepth < 0
						? FormulaStyles.Invalid
						: FormulaStyles.GetBracketColor(span.BracketDepth);
				}
				case FormulaTokenRole.Variable:
				{
					return FormulaSchema.IsKnown(schema, span.VariableName)
						? FormulaStyles.Variable
						: FormulaStyles.Warning;
				}
				default:
				{
					return FormulaStyles.Operator;
				}
			}
		}
	}
}
