using System.Collections.Generic;
using System.Globalization;
using DTech.UtilityAI.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityActionTableView : VisualElement
	{
		private const string ScoreFormat = "0.000";
		private const string EmptyMessage = "Brain has no actions.";
		private const float MinimumScale = 0.0001f;

		private readonly List<ActionRow> _rows = new();

		private int _actionCount = -1;

		public UtilityActionTableView()
		{
			AddToClassList("uai-actions");
		}

		public void Refresh(IUtilityBrainDebugInfo brain, UtilityScoreSampler sampler)
		{
			IReadOnlyList<IUtilityAction> actions = brain?.Actions;
			int count = actions?.Count ?? 0;
			if (count != _actionCount)
			{
				Rebuild(actions, count);
			}

			if (count <= 0)
			{
				return;
			}

			float maxScore = sampler.GetMaxScore();
			for (int i = 0; i < _rows.Count; i++)
			{
				UpdateRow(_rows[i], brain, sampler, maxScore);
			}
		}

		private static void UpdateRow(ActionRow row, IUtilityBrainDebugInfo brain, UtilityScoreSampler sampler, float maxScore)
		{
			float score = sampler.GetScore(row.Index);
			string error = sampler.GetError(row.Index);
			row.Score.text = error != null ? error : score.ToString(ScoreFormat, CultureInfo.InvariantCulture);
			row.Fill.style.width = Length.Percent(GetFillPercent(score, maxScore));
			bool isCurrent = ReferenceEquals(brain.Actions[row.Index], brain.CurrentAction);
			row.Root.EnableInClassList("uai-actions__row--current", isCurrent);
			row.State.text = isCurrent ? brain.CurrentState.ToString() : string.Empty;
			row.Score.EnableInClassList("uai-actions__score--error", error != null);
		}

		private static float GetFillPercent(float score, float maxScore)
		{
			if (float.IsNaN(score) || score <= 0f || maxScore <= MinimumScale)
			{
				return 0f;
			}

			return Mathf.Clamp01(score / maxScore) * 100f;
		}

		private void Rebuild(IReadOnlyList<IUtilityAction> actions, int count)
		{
			Clear();
			_rows.Clear();
			_actionCount = count;
			if (count <= 0)
			{
				var empty = new Label(EmptyMessage);
				empty.AddToClassList("uai-empty");
				Add(empty);
				return;
			}

			for (int i = 0; i < count; i++)
			{
				ActionRow row = CreateRow(actions[i], i);
				_rows.Add(row);
				Add(row.Root);
			}
		}

		private ActionRow CreateRow(IUtilityAction action, int index)
		{
			var root = new VisualElement();
			root.AddToClassList("uai-actions__row");

			var name = new Label($"{index}. {UtilityActionLabel.Get(action)}");
			name.AddToClassList("uai-actions__name");
			root.Add(name);

			var bar = new VisualElement();
			bar.AddToClassList("uai-actions__bar");
			var fill = new VisualElement();
			fill.AddToClassList("uai-actions__bar-fill");
			bar.Add(fill);
			root.Add(bar);

			var score = new Label();
			score.AddToClassList("uai-actions__score");
			root.Add(score);

			var state = new Label();
			state.AddToClassList("uai-actions__state");
			root.Add(state);

			return new ActionRow(root, fill, score, state, index);
		}

		private sealed class ActionRow
		{
			public VisualElement Root { get; }
			public VisualElement Fill { get; }
			public Label Score { get; }
			public Label State { get; }
			public int Index { get; }

			public ActionRow(VisualElement root, VisualElement fill, Label score, Label state, int index)
			{
				Root = root;
				Fill = fill;
				Score = score;
				State = state;
				Index = index;
			}
		}
	}
}
