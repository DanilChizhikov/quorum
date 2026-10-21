namespace DTech.Quorum.Editor
{
	internal sealed class AggregatorMapNode : BrainMapNode
	{
		private const string EmptyLabel = "No Score Aggregator";

		public AggregatorMapNode(ScoreAggregator aggregator)
			: base(aggregator != null ? TypeRegistry.GetDisplayName(aggregator.GetType()) : EmptyLabel, true, false)
		{
			AddToClassList("uai-map__node");
			AddToClassList("uai-map__node--aggregator");
		}
	}
}
