using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace DTech.Quorum.Editor
{
	internal sealed class ConsiderationTreeView : VisualElement
	{
		private const string ScoreFormat = "0.000";
		private const string EmptyMessage = "Current action has no consideration tree.";
		private const float IndentWidth = 14f;

		private readonly List<NodeRow> _rows = new();

		private IQuorumAction _action;

		public ConsiderationTreeView()
		{
			AddToClassList("uai-tree");
		}

		public void Refresh(IQuorumAction action, IQuorumContext context)
		{
			if (!ReferenceEquals(action, _action))
			{
				Rebuild(action);
			}

			for (int i = 0; i < _rows.Count; i++)
			{
				UpdateRow(_rows[i], context);
			}
		}

		private static void UpdateRow(NodeRow row, IQuorumContext context)
		{
			if (row.Node.Consideration == null)
			{
				row.Score.text = string.Empty;
				return;
			}

			float score = ScoreSampler.Evaluate(row.Node.Consideration, context, out string error);
			row.Score.text = error ?? score.ToString(ScoreFormat, CultureInfo.InvariantCulture);
			row.Score.EnableInClassList("uai-tree__score--error", error != null);
		}

		private void Rebuild(IQuorumAction action)
		{
			Clear();
			_rows.Clear();
			_action = action;
			ConsiderationNode root = ConsiderationGraphReader.ReadAction(action);
			if (root == null)
			{
				var empty = new Label(EmptyMessage);
				empty.AddToClassList("uai-empty");
				Add(empty);
				return;
			}

			AddNode(root, 0);
		}

		private void AddNode(ConsiderationNode node, int depth)
		{
			NodeRow row = CreateRow(node, depth);
			_rows.Add(row);
			Add(row.Root);
			IReadOnlyList<ConsiderationNode> children = node.Children;
			for (int i = 0; i < children.Count; i++)
			{
				AddNode(children[i], depth + 1);
			}
		}

		private NodeRow CreateRow(ConsiderationNode node, int depth)
		{
			var root = new VisualElement();
			root.AddToClassList("uai-tree__row");
			root.style.paddingLeft = depth * IndentWidth;

			var name = new Label(node.Label);
			name.AddToClassList("uai-tree__name");
			root.Add(name);

			var score = new Label();
			score.AddToClassList("uai-tree__score");
			root.Add(score);

			return new NodeRow(root, score, node);
		}

		private sealed class NodeRow
		{
			public VisualElement Root { get; }
			public Label Score { get; }
			public ConsiderationNode Node { get; }

			public NodeRow(VisualElement root, Label score, ConsiderationNode node)
			{
				Root = root;
				Score = score;
				Node = node;
			}
		}
	}
}
