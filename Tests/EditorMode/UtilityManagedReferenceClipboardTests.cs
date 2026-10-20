using System.Collections.Generic;
using System.Text.RegularExpressions;
using DTech.UtilityAI.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class UtilityManagedReferenceClipboardTests
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
			var action = new TestUtilityAction(0.75f);
			UtilityManagedReferenceClipboard.Copy(action);

			Assert.IsTrue(UtilityManagedReferenceClipboard.CanPaste(typeof(UtilityAction)));
			Assert.IsTrue(UtilityManagedReferenceClipboard.TryPaste(typeof(UtilityAction), out object value));

			var pastedAction = value as TestUtilityAction;
			Assert.IsNotNull(pastedAction);
			Assert.AreNotSame(action, pastedAction);
			Assert.AreEqual(0.75f, pastedAction.Utility);
		}

		[Test]
		public void Copy_Paste_RestoresNestedConsideration()
		{
			var action = new TestUtilityAction(0.5f);
			action.SetConsideration(new TestUtilityConsideration(0.25f));
			UtilityManagedReferenceClipboard.Copy(action);

			Assert.IsTrue(UtilityManagedReferenceClipboard.TryPaste(typeof(UtilityAction), out object value));

			var pastedAction = value as TestUtilityAction;
			Assert.IsNotNull(pastedAction);

			var pastedConsideration = pastedAction.ActionConsideration as TestUtilityConsideration;
			Assert.IsNotNull(pastedConsideration);
			Assert.AreNotSame(action.ActionConsideration, pastedConsideration);
			Assert.AreEqual(0.25f, pastedConsideration.Score);
		}

		[Test]
		public void Clone_RestoresCompositeChildren()
		{
			var composite = new CompositeUtilityConsideration(
				ConsiderationCombineMode.Multiply,
				new TestUtilityConsideration(0.5f),
				new TestUtilityConsideration(0.5f));

			var clone = UtilityManagedReferenceSerializer.Clone(composite) as CompositeUtilityConsideration;

			Assert.IsNotNull(clone);
			Assert.AreNotSame(composite, clone);
			Assert.AreEqual(0.25f, clone.Evaluate(null));
		}

		[Test]
		public void CopyMany_PasteMany_RestoresEveryEntry()
		{
			var values = new List<object>
			{
				new TestUtilityAction(0.1f),
				new TestUtilityAction(0.2f),
			};

			UtilityManagedReferenceClipboard.CopyMany(values);

			Assert.IsTrue(UtilityManagedReferenceClipboard.TryPasteMany(typeof(UtilityAction), out List<object> pasted));
			Assert.AreEqual(2, pasted.Count);
			Assert.AreEqual(0.1f, ((TestUtilityAction)pasted[0]).Utility);
			Assert.AreEqual(0.2f, ((TestUtilityAction)pasted[1]).Utility);
		}

		[Test]
		public void Paste_OtherBaseType_Fails()
		{
			UtilityManagedReferenceClipboard.Copy(new TestUtilityConsideration(1f));

			Assert.IsFalse(UtilityManagedReferenceClipboard.CanPaste(typeof(UtilityAction)));

			LogAssert.Expect(LogType.Warning, new Regex(WarningPattern));
			Assert.IsFalse(UtilityManagedReferenceClipboard.TryPaste(typeof(UtilityAction), out object value));
			Assert.IsNull(value);
		}

		[Test]
		public void Paste_ForeignBuffer_Fails()
		{
			EditorGUIUtility.systemCopyBuffer = "not a utility payload";

			Assert.IsFalse(UtilityManagedReferenceClipboard.CanPaste(typeof(UtilityAction)));
			Assert.IsFalse(UtilityManagedReferenceClipboard.TryPaste(typeof(UtilityAction), out object value));
			Assert.IsNull(value);
		}
	}
}
