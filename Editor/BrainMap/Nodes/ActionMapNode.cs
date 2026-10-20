namespace DTech.UtilityAI.Editor
{
	internal sealed class ActionMapNode : BrainMapNode
	{
		private const string CurrentClass = "uai-map__node--current";

		public IUtilityAction Action { get; }

		public ActionMapNode(IUtilityAction action) : base(UtilityActionLabel.Get(action), true, true)
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
