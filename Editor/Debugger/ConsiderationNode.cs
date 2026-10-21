using System.Collections.Generic;

namespace DTech.Quorum.Editor
{
	internal sealed class ConsiderationNode
	{
		private readonly List<ConsiderationNode> _children = new();

		public Consideration Consideration { get; }
		public string Label { get; }
		public IReadOnlyList<ConsiderationNode> Children => _children;

		public ConsiderationNode(Consideration consideration, string label)
		{
			Consideration = consideration;
			Label = label;
		}

		public void Add(ConsiderationNode child)
		{
			if (child == null)
			{
				return;
			}

			_children.Add(child);
		}
	}
}
