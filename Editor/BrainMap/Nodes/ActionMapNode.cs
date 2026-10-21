namespace DTech.Quorum.Editor
{
	internal sealed class ActionMapNode : BrainMapNode
	{
		private const string CurrentClass = "uai-map__node--current";

		public IQuorumAction Action { get; }

		public ActionMapNode(IQuorumAction action) : base(ActionLabel.Get(action), true, true)
		{
			Action = action;
			AddToClassList("uai-map__node");
			AddToClassList("uai-map__node--action");
		}

		public void SetCurrent(bool isCurrent)
		{
			EnableInClassList(CurrentClass, isCurrent);
		}
	}
}
