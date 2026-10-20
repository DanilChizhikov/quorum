using System.Collections.Generic;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityScoringRow
	{
		public string Label { get; }
		public IReadOnlyList<float> Scores { get; }
		public IReadOnlyList<string> Errors { get; }
		public int WinnerIndex { get; }
		public int ExpectedIndex { get; }
		public bool HasMismatch => ExpectedIndex >= 0 && ExpectedIndex != WinnerIndex;

		public UtilityScoringRow(
			string label,
			IReadOnlyList<float> scores,
			IReadOnlyList<string> errors,
			int winnerIndex,
			int expectedIndex)
		{
			Label = label;
			Scores = scores;
			Errors = errors;
			WinnerIndex = winnerIndex;
			ExpectedIndex = expectedIndex;
		}
	}
}
