using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilitySerializableFieldCache
	{
		private const BindingFlags DeclaredFieldFlags = BindingFlags.Instance |
														BindingFlags.Public |
														BindingFlags.NonPublic |
														BindingFlags.DeclaredOnly;

		private static readonly Dictionary<Type, FieldInfo[]> _fields = new();

		public static IReadOnlyList<FieldInfo> GetFields(Type type)
		{
			if (type == null)
			{
				return Array.Empty<FieldInfo>();
			}

			if (_fields.TryGetValue(type, out FieldInfo[] cachedFields))
			{
				return cachedFields;
			}

			var fields = new List<FieldInfo>();
			for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
			{
				FieldInfo[] declaredFields = current.GetFields(DeclaredFieldFlags);
				for (int i = 0; i < declaredFields.Length; i++)
				{
					FieldInfo field = declaredFields[i];
					if (!IsSerializable(field))
					{
						continue;
					}

					fields.Add(field);
				}
			}

			FieldInfo[] result = fields.ToArray();
			_fields[type] = result;
			return result;
		}

		public static FieldInfo GetField(Type type, string name)
		{
			IReadOnlyList<FieldInfo> fields = GetFields(type);
			for (int i = 0; i < fields.Count; i++)
			{
				if (!string.Equals(fields[i].Name, name, StringComparison.Ordinal))
				{
					continue;
				}

				return fields[i];
			}

			return null;
		}

		public static bool IsManagedReference(FieldInfo field) =>
			field != null && field.GetCustomAttribute<SerializeReference>(true) != null;

		public static bool IsCollection(Type type) =>
			type != null && (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)));

		public static Type GetElementType(Type type)
		{
			if (type == null)
			{
				return null;
			}

			if (type.IsArray)
			{
				return type.GetElementType();
			}

			return type.IsGenericType ? type.GetGenericArguments()[0] : null;
		}

		public static int GetCollectionCount(object collection) =>
			collection is IList list ? list.Count : 0;

		[InitializeOnLoadMethod]
		private static void ResetCache()
		{
			_fields.Clear();
		}

		private static bool IsSerializable(FieldInfo field)
		{
			if (field.IsStatic || field.IsLiteral || field.IsInitOnly)
			{
				return false;
			}

			if (field.GetCustomAttribute<NonSerializedAttribute>(true) != null)
			{
				return false;
			}

			return field.IsPublic ||
					field.GetCustomAttribute<SerializeField>(true) != null ||
					field.GetCustomAttribute<SerializeReference>(true) != null;
		}
	}
}
