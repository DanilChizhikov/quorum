using DTech.Blackboard;

namespace DTech.UtilityAI
{
	internal readonly struct UtilityFormulaBinding
	{
		private readonly UtilityFormulaValueKind _kind;
		private readonly BlackboardVariable<float> _floatVariable;
		private readonly BlackboardVariable<int> _intVariable;
		private readonly BlackboardVariable<bool> _boolVariable;

		public bool IsResolved => _kind != UtilityFormulaValueKind.None;

		private UtilityFormulaBinding(
			UtilityFormulaValueKind kind,
			BlackboardVariable<float> floatVariable,
			BlackboardVariable<int> intVariable,
			BlackboardVariable<bool> boolVariable)
		{
			_kind = kind;
			_floatVariable = floatVariable;
			_intVariable = intVariable;
			_boolVariable = boolVariable;
		}

		public static UtilityFormulaBinding Float(BlackboardVariable<float> variable) =>
			new(UtilityFormulaValueKind.Float, variable, null, null);

		public static UtilityFormulaBinding Int(BlackboardVariable<int> variable) =>
			new(UtilityFormulaValueKind.Int, null, variable, null);

		public static UtilityFormulaBinding Bool(BlackboardVariable<bool> variable) =>
			new(UtilityFormulaValueKind.Bool, null, null, variable);

		public float ReadValue() =>
			_kind switch
			{
				UtilityFormulaValueKind.Float => _floatVariable.Value,
				UtilityFormulaValueKind.Int => _intVariable.Value,
				UtilityFormulaValueKind.Bool => _boolVariable.Value ? 1f : 0f,
				_ => 0f,
			};
	}
}
