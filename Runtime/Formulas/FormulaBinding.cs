using DTech.Blackboard;

namespace DTech.Quorum
{
	internal readonly struct FormulaBinding
	{
		private readonly FormulaValueKind _kind;
		private readonly BlackboardVariable<float> _floatVariable;
		private readonly BlackboardVariable<int> _intVariable;
		private readonly BlackboardVariable<bool> _boolVariable;

		public bool IsResolved => _kind != FormulaValueKind.None;

		private FormulaBinding(
			FormulaValueKind kind,
			BlackboardVariable<float> floatVariable,
			BlackboardVariable<int> intVariable,
			BlackboardVariable<bool> boolVariable)
		{
			_kind = kind;
			_floatVariable = floatVariable;
			_intVariable = intVariable;
			_boolVariable = boolVariable;
		}

		public static FormulaBinding Float(BlackboardVariable<float> variable) =>
			new(FormulaValueKind.Float, variable, null, null);

		public static FormulaBinding Int(BlackboardVariable<int> variable) =>
			new(FormulaValueKind.Int, null, variable, null);

		public static FormulaBinding Bool(BlackboardVariable<bool> variable) =>
			new(FormulaValueKind.Bool, null, null, variable);

		public float ReadValue() =>
			_kind switch
			{
				FormulaValueKind.Float => _floatVariable.Value,
				FormulaValueKind.Int => _intVariable.Value,
				FormulaValueKind.Bool => _boolVariable.Value ? 1f : 0f,
				_ => 0f,
			};
	}
}
