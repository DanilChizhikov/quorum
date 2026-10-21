namespace DTech.Quorum.Editor
{
	internal readonly struct FormulaHighlightSpan
	{
		public string Text { get; }
		public string VariableName { get; }
		public int Position { get; }
		public FormulaTokenRole Role { get; }
		public int BracketDepth { get; }

		public FormulaHighlightSpan(
			string text,
			string variableName,
			int position,
			FormulaTokenRole role,
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
