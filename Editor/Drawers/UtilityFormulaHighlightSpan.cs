namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityFormulaHighlightSpan
	{
		public string Text { get; }
		public string VariableName { get; }
		public int Position { get; }
		public UtilityFormulaTokenRole Role { get; }
		public int BracketDepth { get; }

		public UtilityFormulaHighlightSpan(
			string text,
			string variableName,
			int position,
			UtilityFormulaTokenRole role,
			int bracketDepth)
		{
			Text = text;
			VariableName = variableName;
			Position = position;
			Role = role;
			BracketDepth = bracketDepth;
		}
	}
}
