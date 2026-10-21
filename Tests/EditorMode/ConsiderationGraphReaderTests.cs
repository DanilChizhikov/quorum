using DTech.Quorum.Editor;
using NUnit.Framework;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class ConsiderationGraphReaderTests
	{
		[Test]
		public void ReadAction_WithoutConsideration_ReturnsNull()
		{
			var action = new TestQuorumAction(1f);

			Assert.IsNull(ConsiderationGraphReader.ReadAction(action));
		}

		[Test]
		public void ReadAction_ReadsNestedComposites()
		{
			var nested = new CompositeConsideration(
				ConsiderationCombineMode.Min,
				new TestFixedConsideration(0.2f),
				new TestFixedConsideration(0.4f));

			var root = new CompositeConsideration(
				ConsiderationCombineMode.Multiply,
				new TestFixedConsideration(0.5f),
				nested);

			var action = new TestQuorumAction(1f);
			action.SetConsideration(root);

			ConsiderationNode rootNode = ConsiderationGraphReader.ReadAction(action);

			Assert.IsNotNull(rootNode);
			Assert.AreSame(root, rootNode.Consideration);
			Assert.AreEqual(2, rootNode.Children.Count);
			Assert.AreSame(nested, rootNode.Children[1].Consideration);
			Assert.AreEqual(2, rootNode.Children[1].Children.Count);
		}

		[Test]
		public void ReadAction_NullChild_BecomesEmptyNode()
		{
			var root = new CompositeConsideration(
				ConsiderationCombineMode.Multiply,
				new TestFixedConsideration(0.5f),
				null);

			var action = new TestQuorumAction(1f);
			action.SetConsideration(root);

			ConsiderationNode rootNode = ConsiderationGraphReader.ReadAction(action);

			Assert.AreEqual(2, rootNode.Children.Count);
			Assert.IsNull(rootNode.Children[1].Consideration);
		}
	}
}
