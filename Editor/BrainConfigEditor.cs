using System.Collections.Generic;
using System.Globalization;
using DTech.UtilityAI.Diagnostics;
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
		private const string FixLabel = "Fix";
		private const float FixButtonWidth = 60f;
		private const string LiveTitle = "Live";
		private const string OpenDebuggerLabel = "Open In Debugger";
		private const string ScoreFormat = "0.000";
		private const float DebuggerButtonWidth = 140f;

		private readonly List<BrainConfigIssue> _issues = new();
		private readonly List<IUtilityBrainDebugInfo> _brains = new();

		private UtilityActionListDrawProvider _actionsDrawProvider;
		private SerializedProperty _updateTypeProperty;
		private SerializedProperty _switchActionTypeProperty;
		private SerializedProperty _considerationProperty;
		private SerializedProperty _scoreAggregatorProperty;
		private bool _issuesDirty = true;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawIssues();
			DrawLiveBrains();
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

			if (serializedObject.ApplyModifiedProperties())
			{
				_issuesDirty = true;
			}
		}

		public override bool RequiresConstantRepaint() => Application.isPlaying;

		private static MessageType GetMessageType(BrainConfigIssueSeverity severity) =>
			severity switch
			{
				BrainConfigIssueSeverity.Error => MessageType.Error,
				BrainConfigIssueSeverity.Warning => MessageType.Warning,
				_ => MessageType.Info,
			};

		private static void DrawLiveBrain(IUtilityBrainDebugInfo brain, int index)
		{
			string state = brain.IsEnabled ? "Enabled" : "Disabled";
			EditorGUILayout.LabelField(
				$"#{index}",
				$"{state}    current: {UtilityActionLabel.Get(brain.CurrentAction)} ({brain.CurrentState})");

			IReadOnlyList<IUtilityAction> actions = brain.Actions;
			using (new EditorGUI.IndentLevelScope())
			{
				for (int i = 0; i < actions.Count; i++)
				{
					float score = UtilityScoreSampler.Evaluate(actions[i], brain.Context, out string error);
					string value = error ?? score.ToString(ScoreFormat, CultureInfo.InvariantCulture);
					EditorGUILayout.LabelField(UtilityActionLabel.Get(actions[i]), value);
				}
			}
		}

		private void OnEnable()
		{
			_updateTypeProperty = serializedObject.FindProperty(UpdateTypePropertyName);
			_switchActionTypeProperty = serializedObject.FindProperty(SwitchActionTypePropertyName);
			_considerationProperty = serializedObject.FindProperty(ConsiderationPropertyName);
			_scoreAggregatorProperty = serializedObject.FindProperty(ScoreAggregatorPropertyName);
			SerializedProperty actionsProperty = serializedObject.FindProperty(ActionsPropertyName);
			_actionsDrawProvider = new UtilityActionListDrawProvider(serializedObject, actionsProperty);
			_issuesDirty = true;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			UtilityBrainDebugRegistry.AddObserver();
		}

		private void OnDisable()
		{
			UtilityBrainDebugRegistry.RemoveObserver();
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			_actionsDrawProvider?.Dispose();
			_actionsDrawProvider = null;
		}

		private void OnUndoRedoPerformed()
		{
			_issuesDirty = true;
			Repaint();
		}

		private void DrawIssues()
		{
			if (target is not BrainConfig config)
			{
				return;
			}

			if (_issuesDirty)
			{
				_issues.Clear();
				BrainConfigValidator.Validate(config, _issues);
				_issuesDirty = false;
			}

			for (int i = 0; i < _issues.Count; i++)
			{
				DrawIssue(_issues[i]);
			}
		}

		private void DrawIssue(in BrainConfigIssue issue)
		{
			EditorGUILayout.HelpBox(issue.Message, GetMessageType(issue.Severity));
			if (!BrainConfigIssueFixer.CanFix(issue.Code))
			{
				return;
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (!GUILayout.Button(FixLabel, GUILayout.Width(FixButtonWidth)))
				{
					return;
				}
			}

			if (!BrainConfigIssueFixer.TryFix(issue))
			{
				return;
			}

			serializedObject.Update();
			_issuesDirty = true;
		}

		private void DrawLiveBrains()
		{
			if (!Application.isPlaying || target is not BrainConfig config)
			{
				return;
			}

			UtilityBrainDebugRegistry.GetBrainsNonAlloc(_brains);
			int shownCount = 0;
			for (int i = 0; i < _brains.Count; i++)
			{
				IUtilityBrainDebugInfo brain = _brains[i];
				if (brain.SourceConfig != config)
				{
					continue;
				}

				if (shownCount <= 0)
				{
					EditorGUILayout.Space();
					EditorGUILayout.LabelField(LiveTitle, EditorStyles.boldLabel);
				}

				shownCount++;
				DrawLiveBrain(brain, shownCount);
			}

			if (shownCount <= 0)
			{
				return;
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(OpenDebuggerLabel, GUILayout.Width(DebuggerButtonWidth)))
				{
					UtilityBrainDebuggerWindow.Open(config);
				}
			}

			EditorGUILayout.Space();
		}
	}
}
