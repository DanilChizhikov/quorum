namespace DTech.UtilityAI
{
	internal readonly struct UtilityFormulaToken
	{
		public UtilityFormulaTokenType Type { get; }
		public string Text { get; }
		public float Value { get; }
		public int Position { get; }
		public int Length { get; }

		private UtilityFormulaToken(UtilityFormulaTokenType type, string text, float value, int position, int length)
		{
			Type = type;
			Text = text;
			Value = value;
			Position = position;
			Length = length;
		}

		public static UtilityFormulaToken Number(float value, int position, int length) =>
			new(UtilityFormulaTokenType.Number, null, value, position, length);

		public static UtilityFormulaToken Identifier(string text, int position, int length) =>
			new(UtilityFormulaTokenType.Identifier, text, 0f, position, length);

		public static UtilityFormulaToken Name(string text, int position, int length) =>
			new(UtilityFormulaTokenType.Name, text, 0f, position, length);

		public static UtilityFormulaToken Operator(string text, int position, int length) =>
			new(UtilityFormulaTokenType.Operator, text, 0f, position, length);

		public static UtilityFormulaToken Symbol(UtilityFormulaTokenType type, int position) =>
			new(type, null, 0f, position, 1);
	}
}
