using System.Collections.Generic;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal static class BrainConfigIssueFixer
	{
		private const string ActionsPropertyName = "_actionEntries";
		private const string ScoreAggregatorPropertyName = "_scoreAggregator";
		private const string UpgradeUndoName = "Upgrade Utility AI Actions";

		public static bool CanFix(BrainConfigIssueCode code) =>
			code switch
			{
				BrainConfigIssueCode.MissingScoreAggregator => true,
				BrainConfigIssueCode.NullActionEntry => true,
				BrainConfigIssueCode.MissingManagedType => true,
				BrainConfigIssueCode.LegacyActionsNotUpgraded => true,
				_ => false,
			};

		public static bool TryFix(in BrainConfigIssue issue)
		{
			if (issue.Config == null || !CanFix(issue.Code))
			{
				return false;
			}

			return issue.Code switch
			{
				BrainConfigIssueCode.MissingScoreAggregator => AssignDefaultAggregator(issue.Config),
				BrainConfigIssueCode.NullActionEntry => RemoveEmptyActions(issue.Config),
				BrainConfigIssueCode.MissingManagedType => ClearProperty(issue.Config, issue.PropertyPath),
				BrainConfigIssueCode.LegacyActionsNotUpgraded => UpgradeLegacyActions(issue.Config),
				_ => false,
			};
		}

		public static int FixAll(IReadOnlyList<BrainConfigIssue> issues)
		{
			if (issues == null)
			{
				return 0;
			}

			int fixedCount = 0;
			for (int i = 0; i < issues.Count; i++)
			{
				if (!TryFix(issues[i]))
				{
					continue;
				}

				fixedCount++;
			}

			return fixedCount;
		}

		private static bool AssignDefaultAggregator(BrainConfig config)
		{
			using var serializedObject = new SerializedObject(config);
			SerializedProperty property = serializedObject.FindProperty(ScoreAggregatorPropertyName);
			if (property == null)
			{
				return false;
			}

			property.managedReferenceValue = new MaxUtilityScoreAggregator();
			serializedObject.ApplyModifiedProperties();
			return true;
		}

		private static bool RemoveEmptyActions(BrainConfig config)
		{
			using var serializedObject = new SerializedObject(config);
			SerializedProperty listProperty = serializedObject.FindProperty(ActionsPropertyName);
			if (listProperty == null)
			{
				return false;
			}

			bool changed = false;
			for (int i = listProperty.arraySize - 1; i >= 0; i--)
			{
				SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
				if (element.managedReferenceValue != null || !string.IsNullOrEmpty(element.managedReferenceFullTypename))
				{
					continue;
				}

				listProperty.DeleteArrayElementAtIndex(i);
				changed = true;
			}

			if (!changed)
			{
				return false;
			}

			serializedObject.ApplyModifiedProperties();
			return true;
		}

		private static bool ClearProperty(BrainConfig config, string propertyPath)
		{
			if (string.IsNullOrEmpty(propertyPath))
			{
				return false;
			}

			using var serializedObject = new SerializedObject(config);
			SerializedProperty property = serializedObject.FindProperty(propertyPath);
			if (property == null || property.propertyType != SerializedPropertyType.ManagedReference)
			{
				return false;
			}

			property.managedReferenceValue = null;
			serializedObject.ApplyModifiedProperties();
			return true;
		}

		private static bool UpgradeLegacyActions(BrainConfig config)
		{
			if (!config.HasLegacyActions)
			{
				return false;
			}

			Undo.RecordObject(config, UpgradeUndoName);
			config.UpgradeLegacyActions();
			EditorUtility.SetDirty(config);
			AssetDatabase.SaveAssetIfDirty(config);
			return true;
		}
	}
}
