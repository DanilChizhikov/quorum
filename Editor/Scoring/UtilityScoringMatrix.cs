using System.Collections.Generic;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityScoringMatrix
	{
		private readonly List<UtilityScoringRow> _rows = new();

		public IReadOnlyList<string> ActionNames { get; }
		public IReadOnlyList<UtilityScoringRow> Rows => _rows;
		public string Error { get; }

		public UtilityScoringMatrix(IReadOnlyList<string> actionNames, string error = null)
		{
			ActionNames = actionNames;
			Error = error;
		}

		public void Add(UtilityScoringRow row)
		{
			if (row == null)
			{
				return;
			}

			_rows.Add(row);
		}
	}
}
