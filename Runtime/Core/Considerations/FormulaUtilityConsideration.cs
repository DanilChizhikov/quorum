using System;
using DTech.Blackboard;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.UtilityAI
{
	/// <summary>
	/// Weight computed from a math formula typed in the inspector. Identifiers in the formula are
	/// blackboard variable names, so a designer can shape a score without writing a consideration.
	/// The formula is compiled once and its variables are resolved once per agent, in Initialize.
	/// The score is 0 when Initialize never ran, when the formula does not compile, when a variable is
	/// not on the blackboard, or when the result is not finite.
	/// </summary>
	[Serializable]
	[UtilityMenu("Formula")]
	[Preserve]
	public sealed class FormulaUtilityConsideration : UtilityConsideration
	{
		private const string DefaultExpression = "1";
		private const float Fallback = 0f;

		[SerializeField, UtilityFormula]
		private string _expression = DefaultExpression;

		private UtilityFormulaEvaluator _evaluator;

		internal string UnresolvedVariables => _evaluator?.UnresolvedVariables;

		public FormulaUtilityConsideration() { }

		public FormulaUtilityConsideration(string expression)
		{
			_expression = string.IsNullOrEmpty(expression) ? DefaultExpression : expression;
		}

		public override void Initialize(IUtilityContext context)
		{
			Bind(context.Blackboard);
		}

		public override UtilityConsideration Clone()
		{
			var clone = (FormulaUtilityConsideration)base.Clone();
			clone._evaluator = null;
			return clone;
		}

		public override float Evaluate(IUtilityContext context)
		{
			return _evaluator?.Evaluate(Fallback) ?? Fallback;
		}

		private void Bind(RuntimeBlackboard blackboard)
		{
			_evaluator ??= new UtilityFormulaEvaluator();
			_evaluator.TrySetExpression(_expression);
			_evaluator.Bind(blackboard);
		}
	}
}
