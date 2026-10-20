using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityManagedReferenceListTarget
	{
		public SerializedObject SerializedObject { get; }
		public string PropertyPath { get; }

		public UtilityManagedReferenceListTarget(SerializedObject serializedObject, string propertyPath)
		{
			SerializedObject = serializedObject;
			PropertyPath = propertyPath;
		}

		public SerializedProperty FindListProperty()
		{
			if (SerializedObject == null || string.IsNullOrEmpty(PropertyPath))
			{
				return null;
			}

			SerializedObject.Update();
			return SerializedObject.FindProperty(PropertyPath);
		}
	}
}
