using System.Collections.Generic;

namespace DTech.Quorum.Editor
{
	internal sealed class ScoringMatrix
	{
		private readonly List<ScoringRow> _rows = new();

		public IReadOnlyList<string> ActionNames { get; }
		public IReadOnlyList<ScoringRow> Rows => _rows;
		public string Error { get; }

		public ScoringMatrix(IReadOnlyList<string> actionNames, string error = null)
		{
			ActionNames = actionNames;
			Error = error;
		}

		public void Add(ScoringRow row)
		{
			if (row == null)
			{
				return;
			}

			_rows.Add(row);
		}
	}
}
