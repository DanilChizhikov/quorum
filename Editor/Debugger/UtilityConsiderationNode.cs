using System.Collections.Generic;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityConsiderationNode
	{
		private readonly List<UtilityConsiderationNode> _children = new();

		public UtilityConsideration Consideration { get; }
		public string Label { get; }
		public IReadOnlyList<UtilityConsiderationNode> Children => _children;

		public UtilityConsiderationNode(UtilityConsideration consideration, string label)
		{
			Consideration = consideration;
			Label = label;
		}

		public void Add(UtilityConsiderationNode child)
		{
			if (child == null)
			{
				return;
			}

			_children.Add(child);
		}
	}
}
