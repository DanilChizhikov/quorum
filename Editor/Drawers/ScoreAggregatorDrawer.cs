using System;
using UnityEditor;

namespace DTech.Quorum.Editor
{
	[CustomPropertyDrawer(typeof(ScoreAggregator), true)]
	internal sealed class ScoreAggregatorDrawer : ReferenceDrawer
	{
		protected override Type BaseType => typeof(ScoreAggregator);
		protected override string Title => "Score Aggregators";
	}
}
