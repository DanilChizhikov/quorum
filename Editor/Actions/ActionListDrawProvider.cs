using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal sealed class ActionListDrawProvider : IDisposable
	{
		private const string HeaderLabel = "Actions";
		private const string SearchTitle = "Actions";
		private const string DuplicateLabel = "Duplicate";
		private const string CopyLabel = "Copy";
		private const string PasteValuesLabel = "Paste Values";
		private const string PasteAsNewLabel = "Paste As New Entry";
		private const string InsertAboveLabel = "Insert Above";
		private const string InsertBelowLabel = "Insert Below";
		private const string MoveToTopLabel = "Move To Top";
		private const string MoveToBottomLabel = "Move To Bottom";
		private const string RemoveLabel = "Remove";
		private const string CopyAllLabel = "Copy All";
		private const string ClearLabel = "Clear";

		private readonly ReorderableList _list;

		public ActionListDrawProvider(SerializedObject serializedObject, SerializedProperty listProperty)
		{
			_list = new ReorderableList(serializedObject, listProperty, true, true, true, true);
			_list.drawHeaderCallback += DrawHeaderCallback;
			_list.drawElementCallback += DrawElementCallback;
			_list.elementHeightCallback += ElementHeightCallback;
			_list.onAddDropdownCallback += AddDropdownCallback;
		}

		public void Draw()
		{
			_list.DoLayoutList();
		}

		public void Dispose()
		{
			_list.drawHeaderCallback -= DrawHeaderCallback;
			_list.drawElementCallback -= DrawElementCallback;
			_list.elementHeightCallback -= ElementHeightCallback;
			_list.onAddDropdownCallback -= AddDropdownCallback;
		}

		private static void HandleContextClick(Rect rect, Action showMenu)
		{
			Event current = Event.current;
			if (current.type != EventType.ContextClick || !rect.Contains(current.mousePosition))
			{
				return;
			}

			showMenu.Invoke();
			current.Use();
		}

		private static void OpenTypeSearch(in ManagedReferenceListTarget target, int index, Vector2 screenPosition)
		{
			ManagedReferenceListTarget capturedTarget = target;
			TypeSearchProvider provider = TypeSearchProvider.Create(
				SearchTitle,
				TypeRegistry.GetOptions(typeof(QuorumAction)),
				type => ManagedReferenceListEditor.InsertType(capturedTarget, index, type));

			SearchWindow.Open(new SearchWindowContext(screenPosition), provider);
		}

		private static void PasteValues(in ManagedReferenceListTarget target, int index)
		{
			if (!ManagedReferenceClipboard.TryPaste(typeof(QuorumAction), out object value))
			{
				return;
			}

			ManagedReferenceListEditor.SetValue(target, index, value);
		}

		private static void PasteAsNew(in ManagedReferenceListTarget target, int index)
		{
			if (!ManagedReferenceClipboard.TryPasteMany(typeof(QuorumAction), out List<object> values))
			{
				return;
			}

			ManagedReferenceListEditor.InsertValues(target, index, values);
		}

		private void DrawHeaderCallback(Rect rect)
		{
			EditorGUI.LabelField(rect, HeaderLabel);
			HandleContextClick(rect, ShowHeaderMenu);
		}

		private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
			var elementRect = new Rect(
				rect.x,
				rect.y + EditorGUIUtility.standardVerticalSpacing,
				rect.width,
				rect.height - EditorGUIUtility.standardVerticalSpacing * 2f);

			EditorGUI.PropertyField(elementRect, element, new GUIContent(index.ToString()), true);
			HandleContextClick(rect, () => ShowElementMenu(index));
		}

		private float ElementHeightCallback(int index)
		{
			SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing * 2f;
		}

		private void AddDropdownCallback(Rect buttonRect, ReorderableList list)
		{
			var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax));
			OpenTypeSearch(GetTarget(), list.serializedProperty.arraySize, screenPosition);
		}

		private ManagedReferenceListTarget GetTarget() =>
			new(_list.serializedProperty.serializedObject.targetObject, _list.serializedProperty.propertyPath);

		private void ShowElementMenu(int index)
		{
			ManagedReferenceListTarget target = GetTarget();
			int lastIndex = _list.serializedProperty.arraySize - 1;
			var screenPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			bool canPaste = ManagedReferenceClipboard.CanPaste(typeof(QuorumAction));
			var menu = new GenericMenu();
			menu.AddEntry(new MenuEntry(DuplicateLabel, true,
				() => ManagedReferenceListEditor.Duplicate(target, index)));
			menu.AddEntry(new MenuEntry(CopyLabel, true,
				() => ManagedReferenceClipboard.Copy(ManagedReferenceListEditor.GetValue(target, index))));
			menu.AddEntry(new MenuEntry(PasteValuesLabel, canPaste, () => PasteValues(target, index)));
			menu.AddEntry(new MenuEntry(PasteAsNewLabel, canPaste, () => PasteAsNew(target, index + 1)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new MenuEntry(InsertAboveLabel, true,
				() => OpenTypeSearch(target, index, screenPosition)));
			menu.AddEntry(new MenuEntry(InsertBelowLabel, true,
				() => OpenTypeSearch(target, index + 1, screenPosition)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new MenuEntry(MoveToTopLabel, index > 0,
				() => ManagedReferenceListEditor.Move(target, index, 0)));
			menu.AddEntry(new MenuEntry(MoveToBottomLabel, index < lastIndex,
				() => ManagedReferenceListEditor.Move(target, index, lastIndex)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new MenuEntry(RemoveLabel, true,
				() => ManagedReferenceListEditor.Remove(target, index)));
			menu.ShowAsContext();
		}

		private void ShowHeaderMenu()
		{
			ManagedReferenceListTarget target = GetTarget();
			int count = _list.serializedProperty.arraySize;
			bool canPaste = ManagedReferenceClipboard.CanPaste(typeof(QuorumAction));
			var menu = new GenericMenu();
			menu.AddEntry(new MenuEntry(CopyAllLabel, count > 0,
				() => ManagedReferenceClipboard.CopyMany(ManagedReferenceListEditor.GetValues(target))));
			menu.AddEntry(new MenuEntry(PasteAsNewLabel, canPaste, () => PasteAsNew(target, count)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new MenuEntry(ClearLabel, count > 0,
				() => ManagedReferenceListEditor.Clear(target)));
			menu.ShowAsContext();
		}
	}
}
