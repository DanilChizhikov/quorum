namespace DTech.Quorum
{
	internal readonly struct FormulaInstruction
	{
		public FormulaOpCode Code { get; }
		public float Value { get; }
		public int VariableIndex { get; }

		private FormulaInstruction(FormulaOpCode code, float value, int variableIndex)
		{
			Code = code;
			Value = value;
			VariableIndex = variableIndex;
		}

		public static FormulaInstruction Constant(float value) =>
			new(FormulaOpCode.PushConstant, value, -1);

		public static FormulaInstruction Variable(int variableIndex) =>
			new(FormulaOpCode.PushVariable, 0f, variableIndex);

		public static FormulaInstruction Operation(FormulaOpCode code) =>
			new(code, 0f, -1);
	}
}
