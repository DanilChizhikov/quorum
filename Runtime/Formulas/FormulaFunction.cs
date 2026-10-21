namespace DTech.Quorum
{
	internal readonly struct FormulaFunction
	{
		public string Name { get; }
		public int Arity { get; }
		public FormulaOpCode Code { get; }

		public FormulaFunction(string name, int arity, FormulaOpCode code)
		{
			Name = name;
			Arity = arity;
			Code = code;
		}
	}
}
