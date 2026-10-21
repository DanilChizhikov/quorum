namespace DTech.Quorum
{
	internal readonly struct FormulaToken
	{
		public FormulaTokenType Type { get; }
		public string Text { get; }
		public float Value { get; }
		public int Position { get; }
		public int Length { get; }

		private FormulaToken(FormulaTokenType type, string text, float value, int position, int length)
		{
			Type = type;
			Text = text;
			Value = value;
			Position = position;
			Length = length;
		}

		public static FormulaToken Number(float value, int position, int length) =>
			new(FormulaTokenType.Number, null, value, position, length);

		public static FormulaToken Identifier(string text, int position, int length) =>
			new(FormulaTokenType.Identifier, text, 0f, position, length);

		public static FormulaToken Name(string text, int position, int length) =>
			new(FormulaTokenType.Name, text, 0f, position, length);

		public static FormulaToken Operator(string text, int position, int length) =>
			new(FormulaTokenType.Operator, text, 0f, position, length);

		public static FormulaToken Symbol(FormulaTokenType type, int position) =>
			new(type, null, 0f, position, 1);
	}
}
