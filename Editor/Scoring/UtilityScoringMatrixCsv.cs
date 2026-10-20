using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityScoringMatrixCsv
	{
		private const string ScoreFormat = "0.0000";
		private const string Separator = ",";
		private const string CaseColumn = "Case";
		private const string WinnerColumn = "Winner";
		private const string ExpectedColumn = "Expected";
		private const string SavePanelTitle = "Export Scoring Matrix";
		private const string DefaultFileName = "utility-scoring-matrix";
		private const string Extension = "csv";

		public static string Build(UtilityScoringMatrix matrix)
		{
			var builder = new StringBuilder();
			IReadOnlyList<string> names = matrix.ActionNames;
			builder.Append(CaseColumn);
			for (int i = 0; i < names.Count; i++)
			{
				builder.Append(Separator).Append(Escape(names[i]));
			}

			builder.Append(Separator).Append(WinnerColumn).Append(Separator).Append(ExpectedColumn).Append('\n');
			IReadOnlyList<UtilityScoringRow> rows = matrix.Rows;
			for (int i = 0; i < rows.Count; i++)
			{
				AppendRow(builder, rows[i], names.Count);
			}

			return builder.ToString();
		}

		public static void Save(UtilityScoringMatrix matrix)
		{
			string path = EditorUtility.SaveFilePanel(SavePanelTitle, string.Empty, DefaultFileName, Extension);
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			File.WriteAllText(path, Build(matrix));
		}

		private static void AppendRow(StringBuilder builder, UtilityScoringRow row, int columnCount)
		{
			builder.Append(Escape(row.Label));
			for (int i = 0; i < columnCount; i++)
			{
				string error = i < row.Errors.Count ? row.Errors[i] : null;
				string value = error ?? (i < row.Scores.Count
					? row.Scores[i].ToString(ScoreFormat, CultureInfo.InvariantCulture)
					: string.Empty);

				builder.Append(Separator).Append(Escape(value));
			}

			builder.Append(Separator).Append(row.WinnerIndex);
			builder.Append(Separator).Append(row.ExpectedIndex).Append('\n');
		}

		private static string Escape(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			if (!value.Contains(Separator) && !value.Contains("\""))
			{
				return value;
			}

			return string.Concat("\"", value.Replace("\"", "\"\""), "\"");
		}
	}
}
