using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityActionListDrawProvider : IDisposable
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

		public UtilityActionListDrawProvider(SerializedObject serializedObject, SerializedProperty listProperty)
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

		private static void OpenTypeSearch(in UtilityManagedReferenceListTarget target, int index, Vector2 screenPosition)
		{
			UtilityManagedReferenceListTarget capturedTarget = target;
			UtilityTypeSearchProvider provider = UtilityTypeSearchProvider.Create(
				SearchTitle,
				UtilityTypeRegistry.GetOptions(typeof(UtilityAction)),
				type => UtilityManagedReferenceListEditor.InsertType(capturedTarget, index, type));

			SearchWindow.Open(new SearchWindowContext(screenPosition), provider);
		}

		private static void PasteValues(in UtilityManagedReferenceListTarget target, int index)
		{
			if (!UtilityManagedReferenceClipboard.TryPaste(typeof(UtilityAction), out object value))
			{
				return;
			}

			UtilityManagedReferenceListEditor.SetValue(target, index, value);
		}

		private static void PasteAsNew(in UtilityManagedReferenceListTarget target, int index)
		{
			if (!UtilityManagedReferenceClipboard.TryPasteMany(typeof(UtilityAction), out List<object> values))
			{
				return;
			}

			UtilityManagedReferenceListEditor.InsertValues(target, index, values);
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

		private UtilityManagedReferenceListTarget GetTarget() =>
			new(_list.serializedProperty.serializedObject.targetObject, _list.serializedProperty.propertyPath);

		private void ShowElementMenu(int index)
		{
			UtilityManagedReferenceListTarget target = GetTarget();
			int lastIndex = _list.serializedProperty.arraySize - 1;
			var screenPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			bool canPaste = UtilityManagedReferenceClipboard.CanPaste(typeof(UtilityAction));
			var menu = new GenericMenu();
			menu.AddEntry(new UtilityMenuItem(DuplicateLabel, true,
				() => UtilityManagedReferenceListEditor.Duplicate(target, index)));
			menu.AddEntry(new UtilityMenuItem(CopyLabel, true,
				() => UtilityManagedReferenceClipboard.Copy(UtilityManagedReferenceListEditor.GetValue(target, index))));
			menu.AddEntry(new UtilityMenuItem(PasteValuesLabel, canPaste, () => PasteValues(target, index)));
			menu.AddEntry(new UtilityMenuItem(PasteAsNewLabel, canPaste, () => PasteAsNew(target, index + 1)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new UtilityMenuItem(InsertAboveLabel, true,
				() => OpenTypeSearch(target, index, screenPosition)));
			menu.AddEntry(new UtilityMenuItem(InsertBelowLabel, true,
				() => OpenTypeSearch(target, index + 1, screenPosition)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new UtilityMenuItem(MoveToTopLabel, index > 0,
				() => UtilityManagedReferenceListEditor.Move(target, index, 0)));
			menu.AddEntry(new UtilityMenuItem(MoveToBottomLabel, index < lastIndex,
				() => UtilityManagedReferenceListEditor.Move(target, index, lastIndex)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new UtilityMenuItem(RemoveLabel, true,
				() => UtilityManagedReferenceListEditor.Remove(target, index)));
			menu.ShowAsContext();
		}

		private void ShowHeaderMenu()
		{
			UtilityManagedReferenceListTarget target = GetTarget();
			int count = _list.serializedProperty.arraySize;
			bool canPaste = UtilityManagedReferenceClipboard.CanPaste(typeof(UtilityAction));
			var menu = new GenericMenu();
			menu.AddEntry(new UtilityMenuItem(CopyAllLabel, count > 0,
				() => UtilityManagedReferenceClipboard.CopyMany(UtilityManagedReferenceListEditor.GetValues(target))));
			menu.AddEntry(new UtilityMenuItem(PasteAsNewLabel, canPaste, () => PasteAsNew(target, count)));
			menu.AddSeparator(string.Empty);
			menu.AddEntry(new UtilityMenuItem(ClearLabel, count > 0,
				() => UtilityManagedReferenceListEditor.Clear(target)));
			menu.ShowAsContext();
		}
	}
}
