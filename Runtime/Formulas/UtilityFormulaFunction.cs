namespace DTech.UtilityAI
{
	internal readonly struct UtilityFormulaFunction
	{
		public string Name { get; }
		public int Arity { get; }
		public UtilityFormulaOpCode Code { get; }

		public UtilityFormulaFunction(string name, int arity, UtilityFormulaOpCode code)
		{
			Name = name;
			Arity = arity;
			Code = code;
		}
	}
}
