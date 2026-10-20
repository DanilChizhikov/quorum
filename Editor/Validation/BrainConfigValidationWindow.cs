using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DTech.UtilityAI.Editor
{
	internal sealed class BrainConfigValidationWindow : EditorWindow
	{
		private const string MenuPath = "Window/DTech/Utility AI/Validator";
		private const string WindowTitle = "Utility AI Validator";
		private const string StyleSheetName = "BrainConfigValidationWindow";
		private const string ScanLabel = "Scan";
		private const string FixAllLabel = "Fix All";
		private const string SelectLabel = "Select";
		private const string FixLabel = "Fix";
		private const string ErrorsOnlyLabel = "Errors Only";
		private const string EmptyMessage = "No issues found.";
		private const string MigrateLabel = "Migrate Types";
		private const string MigrateTitle = "Migrate Renamed Types";
		private const string MigrateOk = "Write Files";
		private const string MigrateCancel = "Cancel";
		private const string MigrateNothingMessage = "No asset references a former type name declared with [UtilityFormerlySerializedType].";
		private const string MigrateConfirmFormat = "These asset files will be rewritten on disk:\n\n{0}";

		private readonly List<BrainConfigIssue> _issues = new();

		private ScrollView _list;
		private Label _summary;
		private bool _errorsOnly;

		[MenuItem(MenuPath)]
		private static void Open()
		{
			var window = GetWindow<BrainConfigValidationWindow>();
			window.titleContent = new GUIContent(WindowTitle);
			window.Show();
		}

		private static void SelectConfig(BrainConfig config)
		{
			Selection.activeObject = config;
			EditorGUIUtility.PingObject(config);
		}

		private static string GetSeverityLabel(BrainConfigIssueSeverity severity) =>
			severity switch
			{
				BrainConfigIssueSeverity.Error => "ERROR",
				BrainConfigIssueSeverity.Warning => "WARNING",
				_ => "INFO",
			};

		private static string GetSeverityClass(BrainConfigIssueSeverity severity) =>
			severity switch
			{
				BrainConfigIssueSeverity.Error => "uai-row__severity--error",
				BrainConfigIssueSeverity.Warning => "uai-row__severity--warning",
				_ => "uai-row__severity--info",
			};

		private void CreateGUI()
		{
			rootVisualElement.AddToClassList("uai-root");
			var styleSheet = Resources.Load<StyleSheet>(StyleSheetName);
			if (styleSheet != null)
			{
				rootVisualElement.styleSheets.Add(styleSheet);
			}

			var toolbar = new Toolbar();
			toolbar.Add(new ToolbarButton(Scan) { text = ScanLabel });
			toolbar.Add(new ToolbarButton(FixAll) { text = FixAllLabel });
			toolbar.Add(new ToolbarButton(Migrate) { text = MigrateLabel });
			var errorsOnlyToggle = new ToolbarToggle { text = ErrorsOnlyLabel, value = _errorsOnly };
			errorsOnlyToggle.RegisterValueChangedCallback(OnErrorsOnlyChanged);
			toolbar.Add(errorsOnlyToggle);
			rootVisualElement.Add(toolbar);

			_summary = new Label();
			_summary.AddToClassList("uai-summary");
			rootVisualElement.Add(_summary);

			_list = new ScrollView();
			_list.AddToClassList("uai-list");
			rootVisualElement.Add(_list);

			Scan();
		}

		private void OnErrorsOnlyChanged(ChangeEvent<bool> changeEvent)
		{
			_errorsOnly = changeEvent.newValue;
			Rebuild();
		}

		private void Scan()
		{
			_issues.Clear();
			_issues.AddRange(BrainConfigValidationScanner.ScanProject());
			Rebuild();
		}

		private void FixAll()
		{
			if (BrainConfigIssueFixer.FixAll(_issues) <= 0)
			{
				return;
			}

			Scan();
		}

		private void Migrate()
		{
			List<UtilityTypeMigration> migrations = UtilitySerializedTypeMigrator.Collect();
			if (migrations.Count <= 0)
			{
				EditorUtility.DisplayDialog(MigrateTitle, MigrateNothingMessage, MigrateCancel);
				return;
			}

			string message = string.Format(MigrateConfirmFormat, UtilitySerializedTypeMigrator.Describe(migrations));
			if (!EditorUtility.DisplayDialog(MigrateTitle, message, MigrateOk, MigrateCancel))
			{
				return;
			}

			if (UtilitySerializedTypeMigrator.Apply(migrations) <= 0)
			{
				return;
			}

			Scan();
		}

		private void Rebuild()
		{
			if (_list == null)
			{
				return;
			}

			_list.Clear();
			int shownCount = 0;
			for (int i = 0; i < _issues.Count; i++)
			{
				BrainConfigIssue issue = _issues[i];
				if (_errorsOnly && issue.Severity != BrainConfigIssueSeverity.Error)
				{
					continue;
				}

				_list.Add(CreateRow(issue));
				shownCount++;
			}

			if (shownCount <= 0)
			{
				var empty = new Label(EmptyMessage);
				empty.AddToClassList("uai-empty");
				_list.Add(empty);
			}

			_summary.text = $"{_issues.Count} issue(s) in project, {shownCount} shown.";
		}

		private VisualElement CreateRow(in BrainConfigIssue issue)
		{
			BrainConfigIssue capturedIssue = issue;
			var row = new VisualElement();
			row.AddToClassList("uai-row");

			var severity = new Label(GetSeverityLabel(issue.Severity));
			severity.AddToClassList("uai-row__severity");
			severity.AddToClassList(GetSeverityClass(issue.Severity));
			row.Add(severity);

			var body = new VisualElement();
			body.AddToClassList("uai-row__body");
			var message = new Label(issue.Message);
			message.AddToClassList("uai-row__message");
			body.Add(message);
			var configName = new Label(issue.Config != null ? issue.Config.name : string.Empty);
			configName.AddToClassList("uai-row__config");
			body.Add(configName);
			row.Add(body);

			var select = new Button(() => SelectConfig(capturedIssue.Config)) { text = SelectLabel };
			select.AddToClassList("uai-row__button");
			row.Add(select);
			if (!BrainConfigIssueFixer.CanFix(issue.Code))
			{
				return row;
			}

			var fix = new Button(() => Fix(capturedIssue)) { text = FixLabel };
			fix.AddToClassList("uai-row__button");
			row.Add(fix);
			return row;
		}

		private void Fix(in BrainConfigIssue issue)
		{
			if (!BrainConfigIssueFixer.TryFix(issue))
			{
				return;
			}

			Scan();
		}
	}
}
