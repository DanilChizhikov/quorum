using System.Collections.Generic;
using System.Globalization;

namespace DTech.UtilityAI
{
	internal static class UtilityFormulaTokenizer
	{
		private const string EmptyFormulaMessage = "Formula is empty";
		private const string UnterminatedNameMessage = "Expected '}' to close the variable name";
		private const string EmptyNameMessage = "Variable name is empty";

		public static bool TryTokenize(string expression, List<UtilityFormulaToken> tokens, out UtilityFormulaError error)
		{
			tokens.Clear();
			error = default;
			if (string.IsNullOrWhiteSpace(expression))
			{
				error = new UtilityFormulaError(EmptyFormulaMessage, 0);
				return false;
			}

			int index = 0;
			while (index < expression.Length)
			{
				char symbol = expression[index];
				if (char.IsWhiteSpace(symbol))
				{
					index++;
					continue;
				}

				if (char.IsDigit(symbol) || (symbol == '.' && IsDigitAt(expression, index + 1)))
				{
					if (!TryReadNumber(expression, tokens, ref index, out error))
					{
						return false;
					}

					continue;
				}

				if (symbol == '_' || char.IsLetter(symbol))
				{
					ReadIdentifier(expression, tokens, ref index);
					continue;
				}

				if (symbol == '{')
				{
					if (!TryReadName(expression, tokens, ref index, out error))
					{
						return false;
					}

					continue;
				}

				if (TryReadSymbol(expression, tokens, ref index))
				{
					continue;
				}

				if (!TryReadOperator(expression, tokens, ref index, out error))
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsDigitAt(string expression, int index) =>
			index < expression.Length && char.IsDigit(expression[index]);

		private static bool TryReadNumber(string expression, List<UtilityFormulaToken> tokens, ref int index, out UtilityFormulaError error)
		{
			error = default;
			int start = index;
			bool hasSeparator = false;
			while (index < expression.Length)
			{
				char symbol = expression[index];
				if (char.IsDigit(symbol))
				{
					index++;
					continue;
				}

				if (symbol != '.' || hasSeparator)
				{
					break;
				}

				hasSeparator = true;
				index++;
			}

			string text = expression.Substring(start, index - start);
			if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
			{
				error = new UtilityFormulaError($"'{text}' is not a number", start);
				return false;
			}

			tokens.Add(UtilityFormulaToken.Number(value, start, index - start));
			return true;
		}

		private static void ReadIdentifier(string expression, List<UtilityFormulaToken> tokens, ref int index)
		{
			int start = index;
			while (index < expression.Length)
			{
				char symbol = expression[index];
				if (symbol != '_' && !char.IsLetterOrDigit(symbol))
				{
					break;
				}

				index++;
			}

			tokens.Add(UtilityFormulaToken.Identifier(expression.Substring(start, index - start), start, index - start));
		}

		private static bool TryReadName(string expression, List<UtilityFormulaToken> tokens, ref int index, out UtilityFormulaError error)
		{
			error = default;
			int start = index;
			int close = expression.IndexOf('}', index + 1);
			if (close < 0)
			{
				error = new UtilityFormulaError(UnterminatedNameMessage, start);
				return false;
			}

			string name = expression.Substring(index + 1, close - index - 1).Trim();
			if (string.IsNullOrEmpty(name))
			{
				error = new UtilityFormulaError(EmptyNameMessage, start);
				return false;
			}

			tokens.Add(UtilityFormulaToken.Name(name, start, close + 1 - start));
			index = close + 1;
			return true;
		}

		private static bool TryReadSymbol(string expression, List<UtilityFormulaToken> tokens, ref int index)
		{
			UtilityFormulaTokenType type;
			switch (expression[index])
			{
				case '(':
				{
					type = UtilityFormulaTokenType.LeftParenthesis;
					break;
				}
				case ')':
				{
					type = UtilityFormulaTokenType.RightParenthesis;
					break;
				}
				case ',':
				{
					type = UtilityFormulaTokenType.Comma;
					break;
				}
				default:
				{
					return false;
				}
			}

			tokens.Add(UtilityFormulaToken.Symbol(type, index));
			index++;
			return true;
		}

		private static bool TryReadOperator(string expression, List<UtilityFormulaToken> tokens, ref int index, out UtilityFormulaError error)
		{
			error = default;
			int start = index;
			string pair = TryReadOperatorPair(expression, index);
			if (pair != null)
			{
				tokens.Add(UtilityFormulaToken.Operator(pair, start, 2));
				index += 2;
				return true;
			}

			string single = TryReadOperatorSymbol(expression[index]);
			if (single != null)
			{
				tokens.Add(UtilityFormulaToken.Operator(single, start, 1));
				index++;
				return true;
			}

			error = new UtilityFormulaError(GetUnexpectedSymbolMessage(expression[index]), start);
			return false;
		}

		private static string TryReadOperatorPair(string expression, int index)
		{
			if (index + 1 >= expression.Length)
			{
				return null;
			}

			char first = expression[index];
			char second = expression[index + 1];
			if (second == '=')
			{
				return first switch
				{
					'<' => "<=",
					'>' => ">=",
					'=' => "==",
					'!' => "!=",
					_ => null,
				};
			}

			if (first == '&' && second == '&')
			{
				return "&&";
			}

			return first == '|' && second == '|' ? "||" : null;
		}

		private static string TryReadOperatorSymbol(char symbol) =>
			symbol switch
			{
				'+' => "+",
				'-' => "-",
				'*' => "*",
				'/' => "/",
				'%' => "%",
				'^' => "^",
				'<' => "<",
				'>' => ">",
				'!' => "!",
				_ => null,
			};

		private static string GetUnexpectedSymbolMessage(char symbol) =>
			symbol switch
			{
				'=' => "Unexpected '=', use '==' to compare",
				'&' => "Unexpected '&', use '&&' for logical and",
				'|' => "Unexpected '|', use '||' for logical or",
				'}' => "Unexpected '}' without a matching '{'",
				_ => $"Unexpected character '{symbol}'",
			};
	}
}
