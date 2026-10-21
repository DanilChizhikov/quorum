using System.Globalization;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DTech.Quorum.Editor
{
	internal abstract class BrainMapNode : Node
	{
		private const string ScoreFormat = "0.000";
		private const string PortName = "";

		private readonly Label _score = new();

		public Port InputPort { get; }
		public Port OutputPort { get; }

		protected BrainMapNode(string nodeTitle, bool hasInput, bool hasOutput)
		{
			title = nodeTitle;
			capabilities &= ~Capabilities.Deletable;
			capabilities &= ~Capabilities.Copiable;
			_score.AddToClassList("uai-map__score");
			mainContainer.Add(_score);
			if (hasInput)
			{
				InputPort = CreatePort(Direction.Input);
				inputContainer.Add(InputPort);
			}

			if (hasOutput)
			{
				OutputPort = CreatePort(Direction.Output);
				outputContainer.Add(OutputPort);
			}

			RefreshExpandedState();
			RefreshPorts();
		}

		public void SetScore(float score, string error)
		{
			if (error != null)
			{
				_score.text = error;
				_score.EnableInClassList("uai-map__score--error", true);
				return;
			}

			_score.EnableInClassList("uai-map__score--error", false);
			_score.text = float.IsNaN(score) ? string.Empty : score.ToString(ScoreFormat, CultureInfo.InvariantCulture);
		}

		public void ClearScore()
		{
			_score.text = string.Empty;
			_score.EnableInClassList("uai-map__score--error", false);
		}

		private static Port CreatePort(Direction direction)
		{
			Port port = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(float));
			port.portName = PortName;
			port.capabilities &= ~Capabilities.Deletable;
			return port;
		}
	}
}
