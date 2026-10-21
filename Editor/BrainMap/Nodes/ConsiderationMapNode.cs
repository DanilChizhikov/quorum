namespace DTech.Quorum.Editor
{
	internal sealed class ConsiderationMapNode : BrainMapNode
	{
		public Consideration Consideration { get; }

		public ConsiderationMapNode(ConsiderationNode node) : base(node.Label, node.Children.Count > 0, true)
		{
			Consideration = node.Consideration;
			AddToClassList("uai-map__node");
			AddToClassList("uai-map__node--consideration");
		}
	}
}
