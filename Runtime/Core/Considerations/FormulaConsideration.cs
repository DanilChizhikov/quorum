using System;
using DTech.Blackboard;
using UnityEngine;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	/// <summary>
	/// Weight computed from a math formula typed in the inspector. Identifiers in the formula are
	/// blackboard variable names, so a designer can shape a score without writing a consideration.
	/// The formula is compiled once and its variables are resolved once per agent, in Initialize.
	/// The score is 0 when Initialize never ran, when the formula does not compile, when a variable is
	/// not on the blackboard, or when the result is not finite.
	/// </summary>
	[Serializable]
	[QuorumMenu("Formula")]
	[Preserve]
	public sealed class FormulaConsideration : Consideration
	{
		private const string DefaultExpression = "1";
		private const float Fallback = 0f;

		[SerializeField, Formula]
		private string _expression = DefaultExpression;

		private FormulaEvaluator _evaluator;

		internal string UnresolvedVariables => _evaluator?.UnresolvedVariables;

		public FormulaConsideration() { }

		public FormulaConsideration(string expression)
		{
			_expression = string.IsNullOrEmpty(expression) ? DefaultExpression : expression;
		}

		public override void Initialize(IQuorumContext context)
		{
			Bind(context?.Blackboard);
		}

		public override Consideration Clone()
		{
			var clone = (FormulaConsideration)base.Clone();
			clone._evaluator = null;
			return clone;
		}

		public override float Evaluate(IQuorumContext context)
		{
			return _evaluator?.Evaluate(Fallback) ?? Fallback;
		}

		private void Bind(RuntimeBlackboard blackboard)
		{
			_evaluator ??= new FormulaEvaluator();
			_evaluator.TrySetExpression(_expression);
			_evaluator.Bind(blackboard);
		}
	}
}
