using System;
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
		
		private void DrawHeaderCallback(Rect rect)
		{
			EditorGUI.LabelField(rect, HeaderLabel);
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
		}
		
		private float ElementHeightCallback(int index)
		{
			SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing * 2f;
		}
		
		private void AddDropdownCallback(Rect buttonRect, ReorderableList list)
		{
			SerializedObject serializedObject = list.serializedProperty.serializedObject;
			string propertyPath = list.serializedProperty.propertyPath;
			UtilityTypeSearchProvider provider = UtilityTypeSearchProvider.Create(
				SearchTitle,
				UtilityTypeRegistry.GetOptions(typeof(UtilityAction)),
				type => AddAction(serializedObject, propertyPath, type));
			
			var searchContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax)));
			SearchWindow.Open(searchContext, provider);
		}
		
		private static void AddAction(SerializedObject serializedObject, string propertyPath, Type type)
		{
			if (type == null)
			{
				return;
			}

			serializedObject.Update();
			SerializedProperty listProperty = serializedObject.FindProperty(propertyPath);
			if (listProperty == null)
			{
				return;
			}
			
			int elementIndex = listProperty.arraySize;
			listProperty.arraySize++;
			SerializedProperty element = listProperty.GetArrayElementAtIndex(elementIndex);
			element.managedReferenceValue = Activator.CreateInstance(type);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
