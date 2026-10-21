using System;
using System.Collections.Generic;
using DTech.Blackboard;

namespace DTech.Quorum.Editor
{
	internal static class FormulaSchema
	{
		private const string Separator = ", ";

		private static readonly List<string> _unknown = new();

		public static bool HasNames(BlackboardAsset schema) =>
			schema != null && schema.Variables != null && schema.Variables.Count > 0;

		public static bool IsKnown(BlackboardAsset schema, string name)
		{
			if (!HasNames(schema))
			{
				return true;
			}

			IReadOnlyList<BlackboardVariable> variables = schema.Variables;
			for (int i = 0; i < variables.Count; i++)
			{
				BlackboardVariable variable = variables[i];
				if (variable != null && Matches(variable.Name, name))
				{
					return true;
				}
			}

			return false;
		}

		public static string GetUnknownNames(BlackboardAsset schema, IReadOnlyList<string> names)
		{
			if (!HasNames(schema) || names == null)
			{
				return null;
			}

			_unknown.Clear();
			for (int i = 0; i < names.Count; i++)
			{
				Collect(schema, names[i]);
			}

			return Join();
		}

		public static string GetUnknownNames(BlackboardAsset schema, FormulaHighlightSpan[] spans)
		{
			if (!HasNames(schema) || spans == null)
			{
				return null;
			}

			_unknown.Clear();
			for (int i = 0; i < spans.Length; i++)
			{
				Collect(schema, spans[i].VariableName);
			}

			return Join();
		}

		private static bool Matches(string left, string right) =>
			string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

		private static string Normalize(string value) =>
			string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

		private static void Collect(BlackboardAsset schema, string name)
		{
			if (string.IsNullOrEmpty(name) || IsKnown(schema, name))
			{
				return;
			}

			for (int i = 0; i < _unknown.Count; i++)
			{
				if (Matches(_unknown[i], name))
				{
					return;
				}
			}

			_unknown.Add(name);
		}

		private static string Join() =>
			_unknown.Count <= 0 ? null : string.Join(Separator, _unknown);
	}
}
