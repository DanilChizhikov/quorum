using DTech.Blackboard;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityFormulaHighlightRequest
	{
		public Rect Position { get; }
		public string Expression { get; }
		public UtilityFormulaHighlightSpan[] Spans { get; }
		public BlackboardAsset Schema { get; }

		public UtilityFormulaHighlightRequest(
			Rect position,
			string expression,
			UtilityFormulaHighlightSpan[] spans,
			BlackboardAsset schema)
		{
			Position = position;
			Expression = expression;
			Spans = spans;
			Schema = schema;
		}
	}
}
