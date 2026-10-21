using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityManagedReferenceListEditor
	{
		public static object GetValue(in UtilityManagedReferenceListTarget target, int index)
		{
			object value = null;
			target.TryEdit(listProperty =>
			{
				if (index < 0 || index >= listProperty.arraySize)
				{
					return;
				}

				value = listProperty.GetArrayElementAtIndex(index).managedReferenceValue;
			});

			return value;
		}

		public static List<object> GetValues(in UtilityManagedReferenceListTarget target)
		{
			var values = new List<object>();
			target.TryEdit(listProperty =>
			{
				for (int i = 0; i < listProperty.arraySize; i++)
				{
					values.Add(listProperty.GetArrayElementAtIndex(i).managedReferenceValue);
				}
			});

			return values;
		}

		public static void InsertType(in UtilityManagedReferenceListTarget target, int index, Type type)
		{
			if (type == null)
			{
				return;
			}

			InsertValue(target, index, Activator.CreateInstance(type));
		}

		public static void InsertValue(in UtilityManagedReferenceListTarget target, int index, object value)
		{
			if (value == null)
			{
				return;
			}

			target.TryEdit(listProperty =>
			{
				int insertIndex = Mathf.Clamp(index, 0, listProperty.arraySize);
				if (insertIndex >= listProperty.arraySize)
				{
					listProperty.arraySize++;
				}
				else
				{
					listProperty.InsertArrayElementAtIndex(insertIndex);
				}

				listProperty.GetArrayElementAtIndex(insertIndex).managedReferenceValue = value;
			});
		}

		public static void InsertValues(in UtilityManagedReferenceListTarget target, int index, IReadOnlyList<object> values)
		{
			if (values == null)
			{
				return;
			}

			for (int i = 0; i < values.Count; i++)
			{
				InsertValue(target, index + i, values[i]);
			}
		}

		public static void SetValue(in UtilityManagedReferenceListTarget target, int index, object value)
		{
			target.TryEdit(listProperty =>
			{
				if (index < 0 || index >= listProperty.arraySize)
				{
					return;
				}

				listProperty.GetArrayElementAtIndex(index).managedReferenceValue = value;
			});
		}

		public static void Duplicate(in UtilityManagedReferenceListTarget target, int index)
		{
			object clone = UtilityManagedReferenceSerializer.Clone(GetValue(target, index));
			InsertValue(target, index + 1, clone);
		}

		public static void Move(in UtilityManagedReferenceListTarget target, int index, int targetIndex)
		{
			target.TryEdit(listProperty =>
			{
				if (index < 0 || index >= listProperty.arraySize)
				{
					return;
				}

				listProperty.MoveArrayElement(index, Mathf.Clamp(targetIndex, 0, listProperty.arraySize - 1));
			});
		}

		public static void Remove(in UtilityManagedReferenceListTarget target, int index)
		{
			target.TryEdit(listProperty =>
			{
				if (index < 0 || index >= listProperty.arraySize)
				{
					return;
				}

				listProperty.GetArrayElementAtIndex(index).managedReferenceValue = null;
				listProperty.DeleteArrayElementAtIndex(index);
			});
		}

		public static void Clear(in UtilityManagedReferenceListTarget target)
		{
			target.TryEdit(listProperty => listProperty.ClearArray());
		}
	}
}
