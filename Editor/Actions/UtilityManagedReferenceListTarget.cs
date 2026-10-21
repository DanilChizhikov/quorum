using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	/// <summary>
	/// Points at a managed reference list by its owning object and property path, not by a live
	/// <see cref="SerializedObject"/>: menu and search window callbacks run a frame later, where a
	/// captured SerializedObject may already belong to a destroyed target.
	/// </summary>
	internal readonly struct UtilityManagedReferenceListTarget
	{
		public Object TargetObject { get; }
		public string PropertyPath { get; }

		public UtilityManagedReferenceListTarget(Object targetObject, string propertyPath)
		{
			TargetObject = targetObject;
			PropertyPath = propertyPath;
		}

		public SerializedProperty FindListProperty()
		{
			if (TargetObject == null || string.IsNullOrEmpty(PropertyPath))
			{
				return null;
			}

			var serializedObject = new SerializedObject(TargetObject);
			return serializedObject.FindProperty(PropertyPath);
		}
	}
}
