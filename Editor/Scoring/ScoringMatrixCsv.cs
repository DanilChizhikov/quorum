using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;

namespace DTech.Quorum.Editor
{
	internal static class ScoringMatrixCsv
	{
		private const string ScoreFormat = "0.0000";
		private const string Separator = ",";
		private const string CaseColumn = "Case";
		private const string WinnerColumn = "Winner";
		private const string ExpectedColumn = "Expected";
		private const string SavePanelTitle = "Export Scoring Matrix";
		private const string DefaultFileName = "utility-scoring-matrix";
		private const string Extension = "csv";

		public static string Build(ScoringMatrix matrix)
		{
			var builder = new StringBuilder();
			IReadOnlyList<string> names = matrix.ActionNames;
			builder.Append(CaseColumn);
			for (int i = 0; i < names.Count; i++)
			{
				builder.Append(Separator).Append(Escape(names[i]));
			}

			builder.Append(Separator).Append(WinnerColumn).Append(Separator).Append(ExpectedColumn).Append('\n');
			IReadOnlyList<ScoringRow> rows = matrix.Rows;
			for (int i = 0; i < rows.Count; i++)
			{
				AppendRow(builder, rows[i], names.Count);
			}

			return builder.ToString();
		}

		public static void Save(ScoringMatrix matrix)
		{
			string path = EditorUtility.SaveFilePanel(SavePanelTitle, string.Empty, DefaultFileName, Extension);
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			// With a BOM so Excel reads non-ASCII labels instead of mojibake.
			File.WriteAllText(path, Build(matrix), new UTF8Encoding(true));
		}

		private static void AppendRow(StringBuilder builder, ScoringRow row, int columnCount)
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

			// Error columns carry exception messages, which can be multi-line: an unquoted line break
			// would split one row into two.
			if (!value.Contains(Separator) &&
				!value.Contains("\"") &&
				!value.Contains('\n') &&
				!value.Contains('\r'))
			{
				return value;
			}

			return string.Concat("\"", value.Replace("\"", "\"\""), "\"");
		}
	}
}
