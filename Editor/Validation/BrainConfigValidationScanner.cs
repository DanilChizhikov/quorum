using System.Collections.Generic;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class BrainConfigValidationScanner
	{
		private const string ConfigFilter = "t:BrainConfig";

		public static List<BrainConfigIssue> ScanProject()
		{
			var issues = new List<BrainConfigIssue>();
			string[] guids = AssetDatabase.FindAssets(ConfigFilter);
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var config = AssetDatabase.LoadAssetAtPath<BrainConfig>(path);
				if (config == null)
				{
					continue;
				}

				BrainConfigValidator.Validate(config, issues);
			}

			return issues;
		}

		public static List<BrainConfigIssue> Scan(BrainConfig config)
		{
			var issues = new List<BrainConfigIssue>();
			BrainConfigValidator.Validate(config, issues);
			return issues;
		}
	}
}
