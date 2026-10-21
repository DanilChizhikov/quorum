using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTech.Quorum.Editor
{
	internal readonly struct ManagedReferenceListTarget
	{
		public Object TargetObject { get; }
		public string PropertyPath { get; }

		public ManagedReferenceListTarget(Object targetObject, string propertyPath)
		{
			TargetObject = targetObject;
			PropertyPath = propertyPath;
		}

		public bool TryEdit(Action<SerializedProperty> edit)
		{
			if (TargetObject == null || string.IsNullOrEmpty(PropertyPath) || edit == null)
			{
				return false;
			}

			using var serializedObject = new SerializedObject(TargetObject);
			SerializedProperty listProperty = serializedObject.FindProperty(PropertyPath);
			if (listProperty == null)
			{
				return false;
			}

			edit.Invoke(listProperty);
			serializedObject.ApplyModifiedProperties();
			return true;
		}
	}
}
