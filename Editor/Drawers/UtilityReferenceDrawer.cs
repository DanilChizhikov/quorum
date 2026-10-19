using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal abstract class UtilityReferenceDrawer : PropertyDrawer
	{
		private const string NoneLabel = "None";
		private const string ClearLabel = "Clear";
		private const float ClearButtonWidth = 60f;
		
		protected abstract Type BaseType { get; }
		protected abstract string Title { get; }
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;
			bool isManagedReference = IsManagedReference(property);
			if (isManagedReference && IsMissingType(property))
			{
				return height + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
			}
			
			if (isManagedReference && property.managedReferenceValue == null)
			{
				return height;
			}
			
			float childrenHeight = GetChildrenHeight(property);
			return childrenHeight > 0f
				? height + EditorGUIUtility.standardVerticalSpacing + childrenHeight
				: height;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool isManagedReference = IsManagedReference(property);
			var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			if (isManagedReference)
			{
				Rect typeRect = EditorGUI.PrefixLabel(headerRect, label);
				DrawTypeField(typeRect, property);
			}
			else
			{
				EditorGUI.LabelField(headerRect, label);
			}
			
			float offset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			var contentRect = new Rect(position.x, position.y + offset, position.width, position.height - offset);
			if (isManagedReference && IsMissingType(property))
			{
				DrawMissingType(contentRect, property);
				return;
			}
			
			if (isManagedReference && property.managedReferenceValue == null)
			{
				return;
			}
			
			DrawChildren(contentRect, property);
		}
		
		private static bool IsManagedReference(SerializedProperty property) =>
			property.propertyType == SerializedPropertyType.ManagedReference;
		
		private static bool IsMissingType(SerializedProperty property) =>
			!string.IsNullOrEmpty(property.managedReferenceFullTypename) && property.managedReferenceValue == null;
		
		private static float GetChildrenHeight(SerializedProperty property)
		{
			float height = 0f;
			SerializedProperty iterator = property.Copy();
			SerializedProperty endProperty = property.GetEndProperty();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
			{
				enterChildren = false;
				height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
			}
			
			return height > 0f ? height - EditorGUIUtility.standardVerticalSpacing : 0f;
		}
		
		private static void DrawChildren(Rect position, SerializedProperty property)
		{
			SerializedProperty iterator = property.Copy();
			SerializedProperty endProperty = property.GetEndProperty();
			bool enterChildren = true;
			float y = position.y;
			EditorGUI.indentLevel++;
			while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
			{
				enterChildren = false;
				float height = EditorGUI.GetPropertyHeight(iterator, true);
				var propertyRect = new Rect(position.x, y, position.width, height);
				EditorGUI.PropertyField(propertyRect, iterator, true);
				y += height + EditorGUIUtility.standardVerticalSpacing;
			}
			
			EditorGUI.indentLevel--;
		}
		
		private static void DrawMissingType(Rect position, SerializedProperty property)
		{
			var messageRect = new Rect(position.x, position.y, position.width - ClearButtonWidth, EditorGUIUtility.singleLineHeight);
			var buttonRect = new Rect(position.xMax - ClearButtonWidth, position.y, ClearButtonWidth, EditorGUIUtility.singleLineHeight);
			Color previousColor = GUI.color;
			GUI.color = Color.red;
			EditorGUI.LabelField(messageRect, $"Missing type: {property.managedReferenceFullTypename}");
			GUI.color = previousColor;
			if (!GUI.Button(buttonRect, ClearLabel))
			{
				return;
			}
			
			property.managedReferenceValue = null;
			property.serializedObject.ApplyModifiedProperties();
		}
		
		private static void AssignType(SerializedObject serializedObject, string propertyPath, Type type)
		{
			if (serializedObject == null)
			{
				return;
			}
			
			serializedObject.Update();
			SerializedProperty property = serializedObject.FindProperty(propertyPath);
			if (property == null)
			{
				return;
			}
			
			property.managedReferenceValue = type != null ? Activator.CreateInstance(type) : null;
			serializedObject.ApplyModifiedProperties();
		}
		
		private void DrawTypeField(Rect position, SerializedProperty property)
		{
			object value = property.managedReferenceValue;
			string typeName = value != null ? UtilityTypeRegistry.GetDisplayName(value.GetType()) : NoneLabel;
			if (!GUI.Button(position, typeName, EditorStyles.popup))
			{
				return;
			}
			
			OpenTypeSearch(position, property);
		}
		
		private void OpenTypeSearch(Rect position, SerializedProperty property)
		{
			SerializedObject serializedObject = property.serializedObject;
			string propertyPath = property.propertyPath;
			UtilityTypeSearchProvider provider = UtilityTypeSearchProvider.Create(
				Title,
				UtilityTypeRegistry.GetOptions(BaseType),
				type => AssignType(serializedObject, propertyPath, type));
			
			var searchContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.yMax)));
			SearchWindow.Open(searchContext, provider);
		}
	}
}
