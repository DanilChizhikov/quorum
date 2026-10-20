namespace DTech.UtilityAI.Editor
{
	internal sealed class AggregatorMapNode : BrainMapNode
	{
		private const string EmptyLabel = "No Score Aggregator";

		public AggregatorMapNode(UtilityScoreAggregator aggregator)
			: base(aggregator != null ? UtilityTypeRegistry.GetDisplayName(aggregator.GetType()) : EmptyLabel, true, false)
		{
			AddToClassList("uai-map__node");
			AddToClassList("uai-map__node--aggregator");
		}
	}
}
