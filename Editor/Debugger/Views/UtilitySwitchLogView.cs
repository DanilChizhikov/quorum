using System.Collections.Generic;
using System.Globalization;
using DTech.UtilityAI.Diagnostics;
using UnityEngine.UIElements;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilitySwitchLogView : VisualElement
	{
		private const string TimeFormat = "0.00";
		private const string ScoreFormat = "0.000";
		private const string EmptyMessage = "No action switches recorded yet.";
		private const int MaxRows = 16;

		private int _recordCount = -1;

		public UtilitySwitchLogView()
		{
			AddToClassList("uai-log");
		}

		public void Refresh(IUtilityBrainDebugInfo brain)
		{
			IReadOnlyList<UtilityBrainSwitchRecord> log = brain?.SwitchLog;
			int count = log?.Count ?? 0;
			if (count == _recordCount)
			{
				return;
			}

			_recordCount = count;
			Clear();
			if (count <= 0)
			{
				var empty = new Label(EmptyMessage);
				empty.AddToClassList("uai-empty");
				Add(empty);
				return;
			}

			int firstIndex = count > MaxRows ? count - MaxRows : 0;
			for (int i = count - 1; i >= firstIndex; i--)
			{
				Add(CreateRow(brain, log[i]));
			}
		}

		private static VisualElement CreateRow(IUtilityBrainDebugInfo brain, in UtilityBrainSwitchRecord record)
		{
			var row = new VisualElement();
			row.AddToClassList("uai-log__row");

			var time = new Label(record.Time.ToString(TimeFormat, CultureInfo.InvariantCulture));
			time.AddToClassList("uai-log__time");
			row.Add(time);

			string fromName = GetActionName(brain, record.FromIndex);
			string toName = GetActionName(brain, record.ToIndex);
			var message = new Label($"{fromName} -> {toName}");
			message.AddToClassList("uai-log__message");
			row.Add(message);

			var score = new Label(record.Score.ToString(ScoreFormat, CultureInfo.InvariantCulture));
			score.AddToClassList("uai-log__score");
			row.Add(score);

			return row;
		}

		private static string GetActionName(IUtilityBrainDebugInfo brain, int index)
		{
			IReadOnlyList<IUtilityAction> actions = brain.Actions;
			return index >= 0 && index < actions.Count ? UtilityActionLabel.Get(actions[index]) : "None";
		}
	}
}
