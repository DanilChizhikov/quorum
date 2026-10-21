namespace DTech.UtilityAI
{
	internal readonly struct UtilityFormulaInstruction
	{
		public UtilityFormulaOpCode Code { get; }
		public float Value { get; }
		public int VariableIndex { get; }

		private UtilityFormulaInstruction(UtilityFormulaOpCode code, float value, int variableIndex)
		{
			Code = code;
			Value = value;
			VariableIndex = variableIndex;
		}

		public static UtilityFormulaInstruction Constant(float value) =>
			new(UtilityFormulaOpCode.PushConstant, value, -1);

		public static UtilityFormulaInstruction Variable(int variableIndex) =>
			new(UtilityFormulaOpCode.PushVariable, 0f, variableIndex);

		public static UtilityFormulaInstruction Operation(UtilityFormulaOpCode code) =>
			new(code, 0f, -1);
	}
}
