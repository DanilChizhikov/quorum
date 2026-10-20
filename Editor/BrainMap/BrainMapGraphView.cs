using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DTech.UtilityAI.Editor
{
	internal sealed class BrainMapGraphView : GraphView
	{
		private readonly List<ActionMapNode> _actionNodes = new();
		private readonly List<ConsiderationMapNode> _considerationNodes = new();

		public BrainMapGraphView()
		{
			style.flexGrow = 1f;
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			Insert(0, new GridBackground());
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) => new();

		public void Build(IReadOnlyList<IUtilityAction> actions, UtilityScoreAggregator aggregator)
		{
			ClearGraph();
			float nextY = 0f;
			for (int i = 0; i < (actions?.Count ?? 0); i++)
			{
				nextY = AddAction(actions[i], nextY);
			}

			var aggregatorNode = new AggregatorMapNode(aggregator);
			BrainMapLayout.Place(
				aggregatorNode,
				BrainMapLayout.ActionColumnX + BrainMapLayout.ColumnWidth,
				nextY * 0.5f);

			AddElement(aggregatorNode);
			for (int i = 0; i < _actionNodes.Count; i++)
			{
				Connect(_actionNodes[i].OutputPort, aggregatorNode.InputPort);
			}
		}

		public void UpdateScores(IUtilityContext context, IUtilityAction currentAction)
		{
			for (int i = 0; i < _actionNodes.Count; i++)
			{
				ActionMapNode node = _actionNodes[i];
				node.SetCurrent(ReferenceEquals(node.Action, currentAction));
				if (context == null)
				{
					node.ClearScore();
					continue;
				}

				float score = UtilityScoreSampler.Evaluate(node.Action, context, out string error);
				node.SetScore(score, error);
			}

			for (int i = 0; i < _considerationNodes.Count; i++)
			{
				ConsiderationMapNode node = _considerationNodes[i];
				if (context == null || node.Consideration == null)
				{
					node.ClearScore();
					continue;
				}

				float score = UtilityScoreSampler.Evaluate(node.Consideration, context, out string error);
				node.SetScore(score, error);
			}
		}

		public void ClearGraph()
		{
			_actionNodes.Clear();
			_considerationNodes.Clear();
			List<GraphElement> elements = graphElements.ToList();
			DeleteElements(elements);
		}

		private void Connect(Port output, Port input)
		{
			if (output == null || input == null)
			{
				return;
			}

			var edge = new Edge
			{
				output = output,
				input = input,
			};

			edge.capabilities &= ~Capabilities.Deletable;
			output.Connect(edge);
			input.Connect(edge);
			AddElement(edge);
		}

		private float AddAction(IUtilityAction action, float startY)
		{
			var actionNode = new ActionMapNode(action);
			_actionNodes.Add(actionNode);
			AddElement(actionNode);

			UtilityConsiderationNode root = UtilityConsiderationGraphReader.ReadAction(action);
			float rowY = startY;
			if (root != null)
			{
				ConsiderationMapNode rootNode = AddConsideration(root, 0, ref rowY);
				Connect(rootNode.OutputPort, actionNode.InputPort);
			}

			float usedHeight = Mathf.Max(rowY - startY, BrainMapLayout.RowHeight);
			BrainMapLayout.Place(
				actionNode,
				BrainMapLayout.ActionColumnX,
				startY + (usedHeight - BrainMapLayout.RowHeight) * 0.5f);

			return startY + usedHeight + BrainMapLayout.ActionGap;
		}

		private ConsiderationMapNode AddConsideration(UtilityConsiderationNode node, int depth, ref float rowY)
		{
			var mapNode = new ConsiderationMapNode(node);
			_considerationNodes.Add(mapNode);
			AddElement(mapNode);
			BrainMapLayout.Place(mapNode, BrainMapLayout.GetConsiderationColumnX(depth), rowY);
			rowY += BrainMapLayout.RowHeight;
			IReadOnlyList<UtilityConsiderationNode> children = node.Children;
			for (int i = 0; i < children.Count; i++)
			{
				ConsiderationMapNode childNode = AddConsideration(children[i], depth + 1, ref rowY);
				Connect(childNode.OutputPort, mapNode.InputPort);
			}

			return mapNode;
		}
	}
}
