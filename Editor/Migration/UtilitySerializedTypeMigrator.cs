using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilitySerializedTypeMigrator
	{
		private const string AssetFilter = "t:BrainConfig t:ScriptableUtilityAction";
		private const string TextModeWarning = "[UtilityAI] Serialized type migration reads and rewrites the asset text, " +
												"which needs Project Settings > Editor > Asset Serialization set to Force Text.";
		private const string ReferencePattern = @"type:\s*\{class:\s*(?<class>[^,}]+),\s*ns:\s*(?<ns>[^,}]*),\s*asm:\s*(?<asm>[^}]+)\}";
		private const string ReferenceFormat = "type: {{class: {0}, ns: {1}, asm: {2}}}";

		public static List<UtilityTypeMigration> Collect()
		{
			var migrations = new List<UtilityTypeMigration>();
			if (!IsTextSerialization())
			{
				return migrations;
			}

			Dictionary<string, Type> formerNames = CollectFormerNames();
			if (formerNames.Count <= 0)
			{
				return migrations;
			}

			var referenceRegex = new Regex(ReferencePattern);
			string[] guids = AssetDatabase.FindAssets(AssetFilter);
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (!File.Exists(path))
				{
					continue;
				}

				CollectAssetMigrations(migrations, formerNames, referenceRegex, path);
			}

			return migrations;
		}

		public static int Apply(IReadOnlyList<UtilityTypeMigration> migrations)
		{
			if (migrations == null || migrations.Count <= 0 || !IsTextSerialization())
			{
				return 0;
			}

			AssetDatabase.SaveAssets();
			Dictionary<string, Type> formerNames = CollectFormerNames();
			var referenceRegex = new Regex(ReferencePattern);
			var paths = new HashSet<string>();
			for (int i = 0; i < migrations.Count; i++)
			{
				paths.Add(migrations[i].AssetPath);
			}

			int changedFiles = 0;
			foreach (string path in paths)
			{
				if (!RewriteAsset(path, formerNames, referenceRegex))
				{
					continue;
				}

				changedFiles++;
			}

			if (changedFiles > 0)
			{
				AssetDatabase.Refresh();
			}

			return changedFiles;
		}

		public static string Describe(IReadOnlyList<UtilityTypeMigration> migrations)
		{
			var builder = new StringBuilder();
			for (int i = 0; i < migrations.Count; i++)
			{
				UtilityTypeMigration migration = migrations[i];
				builder
					.Append(migration.AssetPath)
					.Append('\n')
					.Append("    ")
					.Append(migration.FormerTypeName)
					.Append(" -> ")
					.Append(migration.TargetType.FullName)
					.Append(" x")
					.Append(migration.Count)
					.Append('\n');
			}

			return builder.ToString();
		}

		private static Dictionary<string, Type> CollectFormerNames()
		{
			var formerNames = new Dictionary<string, Type>(StringComparer.Ordinal);
			TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<UtilityFormerlySerializedTypeAttribute>();
			foreach (Type type in types)
			{
				var attributes = type.GetCustomAttributes<UtilityFormerlySerializedTypeAttribute>(false);
				foreach (UtilityFormerlySerializedTypeAttribute attribute in attributes)
				{
					string key = Normalize(attribute.TypeName);
					if (string.IsNullOrEmpty(key) || formerNames.ContainsKey(key))
					{
						continue;
					}

					formerNames[key] = type;
				}
			}

			return formerNames;
		}

		private static void CollectAssetMigrations(
			List<UtilityTypeMigration> migrations,
			Dictionary<string, Type> formerNames,
			Regex referenceRegex,
			string path)
		{
			string text = File.ReadAllText(path);
			var counts = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (Match match in referenceRegex.Matches(text))
			{
				string key = GetKey(match);
				if (!formerNames.ContainsKey(key))
				{
					continue;
				}

				counts.TryGetValue(key, out int count);
				counts[key] = count + 1;
			}

			foreach (KeyValuePair<string, int> pair in counts)
			{
				migrations.Add(new UtilityTypeMigration(path, pair.Key, formerNames[pair.Key], pair.Value));
			}
		}

		private static bool RewriteAsset(string path, Dictionary<string, Type> formerNames, Regex referenceRegex)
		{
			string text = File.ReadAllText(path);
			bool changed = false;
			string result = referenceRegex.Replace(text, match =>
			{
				string key = GetKey(match);
				if (!formerNames.TryGetValue(key, out Type type))
				{
					return match.Value;
				}

				changed = true;
				return string.Format(
					ReferenceFormat,
					GetSerializedClassName(type),
					type.Namespace ?? string.Empty,
					type.Assembly.GetName().Name);
			});

			if (!changed)
			{
				return false;
			}

			File.WriteAllText(path, result);
			Debug.Log($"[UtilityAI] Migrated serialized types in {path}.");
			return true;
		}

		private static bool IsTextSerialization()
		{
			if (EditorSettings.serializationMode == SerializationMode.ForceText)
			{
				return true;
			}

			Debug.LogWarning(TextModeWarning);
			return false;
		}

		/// <summary>
		/// Unity writes a nested type as Outer/Inner in the class field of a SerializeReference entry,
		/// so the outer type cannot be dropped: a bare Name breaks the reference instead of migrating it.
		/// </summary>
		private static string GetSerializedClassName(Type type)
		{
			string className = type.FullName ?? type.Name;
			string namespaceName = type.Namespace;
			if (!string.IsNullOrEmpty(namespaceName) &&
				className.StartsWith(string.Concat(namespaceName, "."), StringComparison.Ordinal))
			{
				className = className.Substring(namespaceName.Length + 1);
			}

			return className.Replace('+', '/');
		}

		private static string GetKey(Match match)
		{
			string className = match.Groups["class"].Value.Trim();
			string namespaceName = match.Groups["ns"].Value.Trim();
			string assemblyName = match.Groups["asm"].Value.Trim();
			string fullName = string.IsNullOrEmpty(namespaceName)
				? className
				: string.Concat(namespaceName, ".", className);

			return string.Concat(fullName, ", ", assemblyName);
		}

		private static string Normalize(string typeName)
		{
			if (string.IsNullOrWhiteSpace(typeName))
			{
				return null;
			}

			int separatorIndex = typeName.IndexOf(',');
			if (separatorIndex < 0)
			{
				return typeName.Trim();
			}

			string fullName = typeName.Substring(0, separatorIndex).Trim();
			string assemblyName = typeName.Substring(separatorIndex + 1).Trim();
			return string.Concat(fullName, ", ", assemblyName);
		}
	}
}
