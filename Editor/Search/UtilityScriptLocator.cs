using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityScriptLocator
	{
		private const string ScriptFilter = "t:MonoScript";
		private const string NameFilterFormat = "{0} t:MonoScript";
		private const string DeclarationPattern = @"\b(class|struct)\s+{0}\b";

		private static readonly Dictionary<Type, MonoScript> _scripts = new();

		private static bool _isBuilt;

		public static MonoScript Find(Type type)
		{
			if (type == null)
			{
				return null;
			}

			Build();
			if (_scripts.TryGetValue(type, out MonoScript script))
			{
				return script;
			}

			script = FindByName(type);
			_scripts[type] = script;
			return script;
		}

		public static string FindPath(Type type)
		{
			MonoScript script = Find(type);
			return script != null ? AssetDatabase.GetAssetPath(script) : null;
		}

		public static Regex CreateDeclarationRegex(Type type) =>
			new(string.Format(DeclarationPattern, Regex.Escape(type.Name)));

		[InitializeOnLoadMethod]
		private static void ResetCache()
		{
			_scripts.Clear();
			_isBuilt = false;
		}

		private static void Build()
		{
			if (_isBuilt)
			{
				return;
			}

			_isBuilt = true;
			string[] guids = AssetDatabase.FindAssets(ScriptFilter);
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				Type scriptType = script != null ? script.GetClass() : null;
				if (scriptType == null)
				{
					continue;
				}

				_scripts[scriptType] = script;
			}
		}

		private static MonoScript FindByName(Type type)
		{
			string[] guids = AssetDatabase.FindAssets(string.Format(NameFilterFormat, type.Name));
			Regex declarationRegex = CreateDeclarationRegex(type);
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (!string.Equals(Path.GetFileNameWithoutExtension(path), type.Name, StringComparison.Ordinal))
				{
					continue;
				}

				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				if (script == null || !declarationRegex.IsMatch(script.text))
				{
					continue;
				}

				return script;
			}

			return null;
		}
	}
}
