using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DTech.Quorum.Editor
{
	internal sealed class ScoringMatrixWindow : EditorWindow
	{
		public const string MenuPath = "Window/DTech/Quorum/Scoring Matrix";

		private const string WindowTitle = "Quorum Scoring Matrix";
		private const string StyleSheetName = "QuorumScoringMatrixWindow";
		private const string ScenarioLabel = "Scenario";
		private const string RunLabel = "Run";
		private const string ExportLabel = "Export CSV";
		private const string EmptyMessage = "Pick a scoring scenario and press Run.";
		private const string CaseColumnLabel = "Case";
		private const string ScoreFormat = "0.000";

		private ObjectField _scenarioField;
		private ScrollView _content;
		private Label _status;
		private ScoringMatrix _matrix;

		[MenuItem(MenuPath)]
		private static void Open()
		{
			var window = GetWindow<ScoringMatrixWindow>();
			window.titleContent = new GUIContent(WindowTitle);
			window.Show();
		}

		private static Label CreateCell(string text, string className)
		{
			var cell = new Label(text);
			cell.AddToClassList("uai-matrix__cell");
			if (!string.IsNullOrEmpty(className))
			{
				cell.AddToClassList(className);
			}

			return cell;
		}

		private void CreateGUI()
		{
			rootVisualElement.AddToClassList("uai-root");
			var styleSheet = Resources.Load<StyleSheet>(StyleSheetName);
			if (styleSheet != null)
			{
				rootVisualElement.styleSheets.Add(styleSheet);
			}

			var toolbar = new Toolbar();
			_scenarioField = new ObjectField(ScenarioLabel)
			{
				objectType = typeof(ScoringScenario),
				allowSceneObjects = false,
			};

			_scenarioField.AddToClassList("uai-matrix__scenario");
			toolbar.Add(_scenarioField);
			toolbar.Add(new ToolbarButton(Run) { text = RunLabel });
			toolbar.Add(new ToolbarButton(Export) { text = ExportLabel });
			rootVisualElement.Add(toolbar);

			_status = new Label(EmptyMessage);
			_status.AddToClassList("uai-matrix__status");
			rootVisualElement.Add(_status);

			_content = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
			_content.AddToClassList("uai-matrix");
			rootVisualElement.Add(_content);
		}

		private void Run()
		{
			var scenario = _scenarioField.value as ScoringScenario;
			_matrix = ScoringMatrixRunner.Run(scenario);
			Rebuild();
		}

		private void Export()
		{
			if (_matrix == null || _matrix.Rows.Count <= 0)
			{
				return;
			}

			ScoringMatrixCsv.Save(_matrix);
		}

		private void Rebuild()
		{
			_content.Clear();
			if (_matrix == null)
			{
				_status.text = EmptyMessage;
				return;
			}

			if (!string.IsNullOrEmpty(_matrix.Error))
			{
				_status.text = _matrix.Error;
				_status.AddToClassList("uai-matrix__status--error");
				return;
			}

			_status.RemoveFromClassList("uai-matrix__status--error");
			int mismatchCount = CountMismatches();
			_status.text = $"{_matrix.Rows.Count} case(s), {_matrix.ActionNames.Count} action(s), {mismatchCount} mismatch(es).";
			_content.Add(CreateHeaderRow());
			IReadOnlyList<ScoringRow> rows = _matrix.Rows;
			for (int i = 0; i < rows.Count; i++)
			{
				_content.Add(CreateRow(rows[i]));
			}
		}

		private int CountMismatches()
		{
			int count = 0;
			IReadOnlyList<ScoringRow> rows = _matrix.Rows;
			for (int i = 0; i < rows.Count; i++)
			{
				if (!rows[i].HasMismatch)
				{
					continue;
				}

				count++;
			}

			return count;
		}

		private VisualElement CreateHeaderRow()
		{
			var row = new VisualElement();
			row.AddToClassList("uai-matrix__row");
			row.AddToClassList("uai-matrix__row--header");
			row.Add(CreateCell(CaseColumnLabel, "uai-matrix__cell--case"));
			IReadOnlyList<string> names = _matrix.ActionNames;
			for (int i = 0; i < names.Count; i++)
			{
				row.Add(CreateCell(names[i], null));
			}

			return row;
		}

		private VisualElement CreateRow(ScoringRow scoringRow)
		{
			var row = new VisualElement();
			row.AddToClassList("uai-matrix__row");
			if (scoringRow.HasMismatch)
			{
				row.AddToClassList("uai-matrix__row--mismatch");
			}

			row.Add(CreateCell(scoringRow.Label, "uai-matrix__cell--case"));
			for (int i = 0; i < _matrix.ActionNames.Count; i++)
			{
				string error = i < scoringRow.Errors.Count ? scoringRow.Errors[i] : null;
				string text = error ?? (i < scoringRow.Scores.Count
					? scoringRow.Scores[i].ToString(ScoreFormat, CultureInfo.InvariantCulture)
					: string.Empty);

				string className = error != null
					? "uai-matrix__cell--error"
					: i == scoringRow.WinnerIndex
						? "uai-matrix__cell--winner"
						: null;

				row.Add(CreateCell(text, className));
			}

			return row;
		}
	}
}
