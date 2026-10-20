namespace DTech.UtilityAI.Editor
{
	internal sealed class ConsiderationMapNode : BrainMapNode
	{
		public UtilityConsideration Consideration { get; }

		public ConsiderationMapNode(UtilityConsiderationNode node) : base(node.Label, node.Children.Count > 0, true)
		{
			Consideration = node.Consideration;
			AddToClassList("uai-map__node");
			AddToClassList("uai-map__node--consideration");
		}
	}
}
