using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilitySerializableAttributeInserter
	{
		private const string AttributeLine = "[Serializable]";
		private const string SystemUsingLine = "using System;";
		private const string UsingPattern = @"^\s*using\s+";
		private const string SystemUsingPattern = @"^\s*using\s+System\s*;\s*$";
		private const string DialogTitle = "Add [Serializable]";
		private const string DialogOk = "Write File";
		private const string DialogCancel = "Cancel";
		private const string MissingScriptTitle = "Script Not Found";
		private const string MissingScriptOk = "OK";

		public static bool CanInsert(Type type) =>
			type != null && !string.IsNullOrEmpty(UtilityScriptLocator.FindPath(type));

		public static bool TryInsert(Type type)
		{
			string path = UtilityScriptLocator.FindPath(type);
			if (string.IsNullOrEmpty(path))
			{
				EditorUtility.DisplayDialog(
					MissingScriptTitle,
					$"Cannot find a script file for {type?.FullName}. Add [Serializable] by hand.",
					MissingScriptOk);

				return false;
			}

			string text = File.ReadAllText(path);
			string newLine = text.Contains("\r\n") ? "\r\n" : "\n";
			var lines = new List<string>(text.Split(new[] { newLine }, StringSplitOptions.None));
			int declarationIndex = FindDeclarationIndex(lines, type);
			if (declarationIndex < 0)
			{
				EditorUtility.DisplayDialog(
					MissingScriptTitle,
					$"Cannot find the declaration of {type.Name} in {path}.",
					MissingScriptOk);

				return false;
			}

			if (HasAttribute(lines, declarationIndex))
			{
				return false;
			}

			string indent = GetIndent(lines[declarationIndex]);
			string attribute = string.Concat(indent, AttributeLine);
			bool needsUsing = !HasSystemUsing(lines);
			string preview = needsUsing
				? $"{path}\n\n+ {SystemUsingLine}\n+ {attribute.Trim()} (line {declarationIndex + 1})"
				: $"{path}\n\n+ {attribute.Trim()} (line {declarationIndex + 1})";

			if (!EditorUtility.DisplayDialog(DialogTitle, preview, DialogOk, DialogCancel))
			{
				return false;
			}

			lines.Insert(declarationIndex, attribute);
			if (needsUsing)
			{
				lines.Insert(GetUsingInsertIndex(lines), SystemUsingLine);
			}

			File.WriteAllText(path, string.Join(newLine, lines));
			AssetDatabase.ImportAsset(path);
			return true;
		}

		private static int FindDeclarationIndex(List<string> lines, Type type)
		{
			Regex declarationRegex = UtilityScriptLocator.CreateDeclarationRegex(type);
			for (int i = 0; i < lines.Count; i++)
			{
				if (!declarationRegex.IsMatch(lines[i]))
				{
					continue;
				}

				return i;
			}

			return -1;
		}

		private static bool HasAttribute(List<string> lines, int declarationIndex)
		{
			for (int i = declarationIndex - 1; i >= 0; i--)
			{
				string line = lines[i].Trim();
				if (line.Length <= 0)
				{
					continue;
				}

				if (!line.StartsWith("[", StringComparison.Ordinal))
				{
					return false;
				}

				if (line.Contains("Serializable"))
				{
					return true;
				}
			}

			return false;
		}

		private static bool HasSystemUsing(List<string> lines)
		{
			var systemUsingRegex = new Regex(SystemUsingPattern);
			for (int i = 0; i < lines.Count; i++)
			{
				if (!systemUsingRegex.IsMatch(lines[i]))
				{
					continue;
				}

				return true;
			}

			return false;
		}

		private static int GetUsingInsertIndex(List<string> lines)
		{
			var usingRegex = new Regex(UsingPattern);
			for (int i = 0; i < lines.Count; i++)
			{
				if (!usingRegex.IsMatch(lines[i]))
				{
					continue;
				}

				return i;
			}

			return 0;
		}

		private static string GetIndent(string line)
		{
			int count = 0;
			while (count < line.Length && (line[count] == ' ' || line[count] == '\t'))
			{
				count++;
			}

			return line.Substring(0, count);
		}
	}
}
