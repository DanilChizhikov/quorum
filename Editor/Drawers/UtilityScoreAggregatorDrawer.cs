using System;
using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	[CustomPropertyDrawer(typeof(UtilityScoreAggregator), true)]
	internal sealed class UtilityScoreAggregatorDrawer : UtilityReferenceDrawer
	{
		protected override Type BaseType => typeof(UtilityScoreAggregator);
		protected override string Title => "Score Aggregators";
	}
}
