using System.Collections.Generic;
using System.Text.RegularExpressions;
using DTech.Quorum.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DTech.Quorum.Tests.EditorMode
{
	internal sealed class ManagedReferenceClipboardTests
	{
		private const string WarningPattern = "Cannot paste";

		private string _buffer;

		[SetUp]
		public void SetUp()
		{
			_buffer = EditorGUIUtility.systemCopyBuffer;
		}

		[TearDown]
		public void TearDown()
		{
			EditorGUIUtility.systemCopyBuffer = _buffer;
		}

		[Test]
		public void Copy_Paste_RestoresSerializedFields()
		{
			var action = new TestQuorumAction(0.75f);
			ManagedReferenceClipboard.Copy(action);

			Assert.IsTrue(ManagedReferenceClipboard.CanPaste(typeof(QuorumAction)));
			Assert.IsTrue(ManagedReferenceClipboard.TryPaste(typeof(QuorumAction), out object value));

			var pastedAction = value as TestQuorumAction;
			Assert.IsNotNull(pastedAction);
			Assert.AreNotSame(action, pastedAction);
			Assert.AreEqual(0.75f, pastedAction.Utility);
		}

		[Test]
		public void Copy_Paste_RestoresNestedConsideration()
		{
			var action = new TestQuorumAction(0.5f);
			action.SetConsideration(new TestFixedConsideration(0.25f));
			ManagedReferenceClipboard.Copy(action);

			Assert.IsTrue(ManagedReferenceClipboard.TryPaste(typeof(QuorumAction), out object value));

			var pastedAction = value as TestQuorumAction;
			Assert.IsNotNull(pastedAction);

			var pastedConsideration = pastedAction.ActionConsideration as TestFixedConsideration;
			Assert.IsNotNull(pastedConsideration);
			Assert.AreNotSame(action.ActionConsideration, pastedConsideration);
			Assert.AreEqual(0.25f, pastedConsideration.Score);
		}

		[Test]
		public void Clone_RestoresCompositeChildren()
		{
			var composite = new CompositeConsideration(
				ConsiderationCombineMode.Multiply,
				new TestFixedConsideration(0.5f),
				new TestFixedConsideration(0.5f));

			var clone = ManagedReferenceSerializer.Clone(composite) as CompositeConsideration;

			Assert.IsNotNull(clone);
			Assert.AreNotSame(composite, clone);
			Assert.AreEqual(0.25f, clone.Evaluate(null));
		}

		[Test]
		public void CopyMany_PasteMany_RestoresEveryEntry()
		{
			var values = new List<object>
			{
				new TestQuorumAction(0.1f),
				new TestQuorumAction(0.2f),
			};

			ManagedReferenceClipboard.CopyMany(values);

			Assert.IsTrue(ManagedReferenceClipboard.TryPasteMany(typeof(QuorumAction), out List<object> pasted));
			Assert.AreEqual(2, pasted.Count);
			Assert.AreEqual(0.1f, ((TestQuorumAction)pasted[0]).Utility);
			Assert.AreEqual(0.2f, ((TestQuorumAction)pasted[1]).Utility);
		}

		[Test]
		public void Paste_OtherBaseType_Fails()
		{
			ManagedReferenceClipboard.Copy(new TestFixedConsideration(1f));

			Assert.IsFalse(ManagedReferenceClipboard.CanPaste(typeof(QuorumAction)));

			LogAssert.Expect(LogType.Warning, new Regex(WarningPattern));
			Assert.IsFalse(ManagedReferenceClipboard.TryPaste(typeof(QuorumAction), out object value));
			Assert.IsNull(value);
		}

		[Test]
		public void Paste_ForeignBuffer_Fails()
		{
			EditorGUIUtility.systemCopyBuffer = "not a utility payload";

			Assert.IsFalse(ManagedReferenceClipboard.CanPaste(typeof(QuorumAction)));
			Assert.IsFalse(ManagedReferenceClipboard.TryPaste(typeof(QuorumAction), out object value));
			Assert.IsNull(value);
		}
	}
}
