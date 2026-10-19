using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	[CustomEditor(typeof(BrainConfig), true)]
	public class BrainConfigEditor : UnityEditor.Editor
	{
		private const string UpdateTypePropertyName = "_updateType";
		private const string SwitchActionTypePropertyName = "_switchActionType";
		private const string ActionsPropertyName = "_actionEntries";
		private const string ConsiderationPropertyName = "_consideration";
		private const string ScoreAggregatorPropertyName = "_scoreAggregator";
		private const string LegacyMessage = "This config still stores actions as ScriptableObject assets. Upgrade them to config entries to keep editing them.";
		private const string UpgradeLabel = "Upgrade Legacy Actions";
		private const string UpgradeUndoName = "Upgrade Utility AI Actions";
		
		private UtilityActionListDrawProvider _actionsDrawProvider;
		private SerializedProperty _updateTypeProperty;
		private SerializedProperty _switchActionTypeProperty;
		private SerializedProperty _considerationProperty;
		private SerializedProperty _scoreAggregatorProperty;
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawLegacyActionsUpgrade();
			using (new EditorGUI.DisabledScope(Application.isPlaying))
			{
				EditorGUILayout.PropertyField(_updateTypeProperty);
				EditorGUILayout.PropertyField(_switchActionTypeProperty);
				EditorGUILayout.Space();
				_actionsDrawProvider.Draw();
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(_considerationProperty);
				EditorGUILayout.PropertyField(_scoreAggregatorProperty);
			}
			
			serializedObject.ApplyModifiedProperties();
		}
		
		private void OnEnable()
		{
			_updateTypeProperty = serializedObject.FindProperty(UpdateTypePropertyName);
			_switchActionTypeProperty = serializedObject.FindProperty(SwitchActionTypePropertyName);
			_considerationProperty = serializedObject.FindProperty(ConsiderationPropertyName);
			_scoreAggregatorProperty = serializedObject.FindProperty(ScoreAggregatorPropertyName);
			SerializedProperty actionsProperty = serializedObject.FindProperty(ActionsPropertyName);
			_actionsDrawProvider = new UtilityActionListDrawProvider(serializedObject, actionsProperty);
		}
		
		private void OnDisable()
		{
			_actionsDrawProvider?.Dispose();
			_actionsDrawProvider = null;
		}
		
		private void DrawLegacyActionsUpgrade()
		{
			if (target is not BrainConfig config || !config.HasLegacyActions)
			{
				return;
			}

			EditorGUILayout.HelpBox(LegacyMessage, MessageType.Warning);
			if (!GUILayout.Button(UpgradeLabel))
			{
				return;
			}

			Undo.RecordObject(config, UpgradeUndoName);
			config.UpgradeLegacyActions();
			EditorUtility.SetDirty(config);
			AssetDatabase.SaveAssetIfDirty(config);
			serializedObject.Update();
		}
	}
}
