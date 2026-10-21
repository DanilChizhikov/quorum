using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal readonly struct FormulaHighlightRequest
	{
		public Rect Position { get; }
		public string Expression { get; }
		public FormulaHighlightSpan[] Spans { get; }
		public BlackboardAsset Schema { get; }

		public FormulaHighlightRequest(
			Rect position,
			string expression,
			FormulaHighlightSpan[] spans,
			BlackboardAsset schema)
		{
			Position = position;
			Expression = expression;
			Spans = spans;
			Schema = schema;
		}
	}
}
