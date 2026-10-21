using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal static class ManagedReferenceListEditor
	{
		public static object GetValue(in ManagedReferenceListTarget target, int index)
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

		public static List<object> GetValues(in ManagedReferenceListTarget target)
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

		public static void InsertType(in ManagedReferenceListTarget target, int index, Type type)
		{
			if (type == null)
			{
				return;
			}

			InsertValue(target, index, Activator.CreateInstance(type));
		}

		public static void InsertValue(in ManagedReferenceListTarget target, int index, object value)
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

		public static void InsertValues(in ManagedReferenceListTarget target, int index, IReadOnlyList<object> values)
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

		public static void SetValue(in ManagedReferenceListTarget target, int index, object value)
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

		public static void Duplicate(in ManagedReferenceListTarget target, int index)
		{
			object clone = ManagedReferenceSerializer.Clone(GetValue(target, index));
			InsertValue(target, index + 1, clone);
		}

		public static void Move(in ManagedReferenceListTarget target, int index, int targetIndex)
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

		public static void Remove(in ManagedReferenceListTarget target, int index)
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

		public static void Clear(in ManagedReferenceListTarget target)
		{
			target.TryEdit(listProperty => listProperty.ClearArray());
		}
	}
}
