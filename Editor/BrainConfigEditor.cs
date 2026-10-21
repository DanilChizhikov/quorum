using System.Collections.Generic;
using System.Globalization;
using DTech.Quorum.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	[CustomEditor(typeof(BrainConfig), true)]
	public class BrainConfigEditor : UnityEditor.Editor
	{
		private const string UpdateTypePropertyName = "_updateType";
		private const string SwitchActionTypePropertyName = "_switchActionType";
		private const string ActionsPropertyName = "_actionEntries";
		private const string ScoreAggregatorPropertyName = "_scoreAggregator";
		private const string BlackboardSchemaPropertyName = "_blackboardSchema";
		private const string FixLabel = "Fix";
		private const float FixButtonWidth = 60f;
		private const string LiveTitle = "Live";
		private const string OpenDebuggerLabel = "Open In Debugger";
		private const string ScoreFormat = "0.000";
		private const float DebuggerButtonWidth = 140f;

		private readonly List<BrainConfigIssue> _issues = new();
		private readonly List<IBrainDebugInfo> _brains = new();
		private readonly Dictionary<int, ScoreSampler> _samplers = new();

		private ActionListDrawProvider _actionsDrawProvider;
		private SerializedProperty _updateTypeProperty;
		private SerializedProperty _switchActionTypeProperty;
		private SerializedProperty _scoreAggregatorProperty;
		private SerializedProperty _blackboardSchemaProperty;
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
				EditorGUILayout.PropertyField(_blackboardSchemaProperty);
				EditorGUILayout.Space();
				_actionsDrawProvider.Draw();
				EditorGUILayout.Space();
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

		private void DrawLiveBrain(IBrainDebugInfo brain, int index)
		{
			string state = brain.IsEnabled ? "Enabled" : "Disabled";
			EditorGUILayout.LabelField(
				$"#{index}",
				$"{state}    current: {ActionLabel.Get(brain.CurrentAction)} ({brain.CurrentState})");

			ScoreSampler sampler = GetSampler(brain.DebugToken);
			sampler.Sample(brain, false);
			IReadOnlyList<IQuorumAction> actions = brain.Actions;
			using (new EditorGUI.IndentLevelScope())
			{
				for (int i = 0; i < actions.Count; i++)
				{
					string error = sampler.GetError(i);
					string value = error ?? sampler.GetScore(i).ToString(ScoreFormat, CultureInfo.InvariantCulture);
					EditorGUILayout.LabelField(ActionLabel.Get(actions[i]), value);
				}
			}
		}

		private ScoreSampler GetSampler(int token)
		{
			if (_samplers.TryGetValue(token, out ScoreSampler sampler))
			{
				return sampler;
			}

			sampler = new ScoreSampler();
			_samplers.Add(token, sampler);
			return sampler;
		}

		private void OnEnable()
		{
			_updateTypeProperty = serializedObject.FindProperty(UpdateTypePropertyName);
			_switchActionTypeProperty = serializedObject.FindProperty(SwitchActionTypePropertyName);
			_scoreAggregatorProperty = serializedObject.FindProperty(ScoreAggregatorPropertyName);
			_blackboardSchemaProperty = serializedObject.FindProperty(BlackboardSchemaPropertyName);
			SerializedProperty actionsProperty = serializedObject.FindProperty(ActionsPropertyName);
			_actionsDrawProvider = new ActionListDrawProvider(serializedObject, actionsProperty);
			_issuesDirty = true;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			BrainDebugRegistry.AddObserver();
		}

		private void OnDisable()
		{
			BrainDebugRegistry.RemoveObserver();
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			_samplers.Clear();
			_brains.Clear();
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
				BrainConfigIssue issue = _issues[i];
				DrawIssue(issue);
			}
		}

		private void DrawIssue(in BrainConfigIssue issue)
		{
			MessageType messageType = GetMessageType(issue.Severity);
			EditorGUILayout.HelpBox(issue.Message, messageType);
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

			BrainDebugRegistry.GetBrainsNonAlloc(_brains);
			int shownCount = 0;
			for (int i = 0; i < _brains.Count; i++)
			{
				IBrainDebugInfo brain = _brains[i];
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
					BrainDebuggerWindow.Open(config);
				}
			}

			EditorGUILayout.Space();
		}
	}
}
