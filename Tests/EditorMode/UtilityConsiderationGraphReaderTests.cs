using DTech.UtilityAI.Editor;
using NUnit.Framework;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class UtilityConsiderationGraphReaderTests
	{
		[Test]
		public void ReadAction_WithoutConsideration_ReturnsNull()
		{
			var action = new TestUtilityAction(1f);

			Assert.IsNull(UtilityConsiderationGraphReader.ReadAction(action));
		}

		[Test]
		public void ReadAction_ReadsNestedComposites()
		{
			var nested = new CompositeUtilityConsideration(
				ConsiderationCombineMode.Min,
				new TestUtilityConsideration(0.2f),
				new TestUtilityConsideration(0.4f));

			var root = new CompositeUtilityConsideration(
				ConsiderationCombineMode.Multiply,
				new TestUtilityConsideration(0.5f),
				nested);

			var action = new TestUtilityAction(1f);
			action.SetConsideration(root);

			UtilityConsiderationNode rootNode = UtilityConsiderationGraphReader.ReadAction(action);

			Assert.IsNotNull(rootNode);
			Assert.AreSame(root, rootNode.Consideration);
			Assert.AreEqual(2, rootNode.Children.Count);
			Assert.AreSame(nested, rootNode.Children[1].Consideration);
			Assert.AreEqual(2, rootNode.Children[1].Children.Count);
		}

		[Test]
		public void ReadAction_NullChild_BecomesEmptyNode()
		{
			var root = new CompositeUtilityConsideration(
				ConsiderationCombineMode.Multiply,
				new TestUtilityConsideration(0.5f),
				null);

			var action = new TestUtilityAction(1f);
			action.SetConsideration(root);

			UtilityConsiderationNode rootNode = UtilityConsiderationGraphReader.ReadAction(action);

			Assert.AreEqual(2, rootNode.Children.Count);
			Assert.IsNull(rootNode.Children[1].Consideration);
		}
	}
}
