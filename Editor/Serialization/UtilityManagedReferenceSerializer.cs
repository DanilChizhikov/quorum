using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityManagedReferenceSerializer
	{
		private const string IndexOpen = "[";
		private const string IndexClose = "]";

		public static string Serialize(object value) =>
			value == null ? string.Empty : SerializeMany(new[] { value });

		public static string SerializeMany(IReadOnlyList<object> values)
		{
			if (values == null || values.Count <= 0)
			{
				return string.Empty;
			}

			var nodes = new List<UtilityManagedReferenceNode>();
			for (int i = 0; i < values.Count; i++)
			{
				object value = values[i];
				if (value == null)
				{
					continue;
				}

				AppendNode(nodes, value, -1, string.Empty);
			}

			if (nodes.Count <= 0)
			{
				return string.Empty;
			}

			var payload = new UtilityManagedReferencePayload
			{
				Nodes = nodes.ToArray(),
			};

			return JsonUtility.ToJson(payload);
		}

		public static object Deserialize(string json, Type baseType)
		{
			List<object> roots = DeserializeAll(json, baseType);
			return roots.Count > 0 ? roots[0] : null;
		}

		public static List<object> DeserializeAll(string json, Type baseType)
		{
			var roots = new List<object>();
			UtilityManagedReferencePayload payload = ReadPayload(json);
			if (payload == null || payload.Nodes.Length <= 0)
			{
				return roots;
			}

			var instances = new object[payload.Nodes.Length];
			for (int i = 0; i < payload.Nodes.Length; i++)
			{
				UtilityManagedReferenceNode node = payload.Nodes[i];
				Type type = ResolveType(node.Type);
				if (type == null)
				{
					Debug.LogWarning($"[UtilityAI] Cannot paste: type {node.Type} is not available in this project.");
					continue;
				}

				object instance;
				try
				{
					instance = Activator.CreateInstance(type);
				}
				catch (Exception exception)
				{
					Debug.LogWarning($"[UtilityAI] Cannot paste: type {node.Type} has no usable parameterless constructor. {exception.Message}");
					continue;
				}

				EditorJsonUtility.FromJsonOverwrite(node.Json, instance);
				instances[i] = instance;
				if (node.Parent >= 0 && node.Parent < i)
				{
					AssignChild(instances[node.Parent], node.Field, instance);
					continue;
				}

				if (baseType != null && !baseType.IsInstanceOfType(instance))
				{
					Debug.LogWarning($"[UtilityAI] Cannot paste: {instance.GetType().Name} is not a {baseType.Name}.");
					continue;
				}

				roots.Add(instance);
			}

			return roots;
		}

		public static Type PeekRootType(string json)
		{
			UtilityManagedReferencePayload payload = ReadPayload(json);
			if (payload == null)
			{
				return null;
			}

			for (int i = 0; i < payload.Nodes.Length; i++)
			{
				if (payload.Nodes[i].Parent >= 0)
				{
					continue;
				}

				return ResolveType(payload.Nodes[i].Type);
			}

			return null;
		}

		public static object Clone(object value) =>
			value == null ? null : Deserialize(Serialize(value), value.GetType());

		private static UtilityManagedReferencePayload ReadPayload(string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			try
			{
				return JsonUtility.FromJson<UtilityManagedReferencePayload>(json);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static int AppendNode(List<UtilityManagedReferenceNode> nodes, object instance, int parent, string field)
		{
			Type type = instance.GetType();
			var node = new UtilityManagedReferenceNode
			{
				Parent = parent,
				Field = field,
				Type = GetTypeName(type),
				Json = EditorJsonUtility.ToJson(instance),
			};

			int index = nodes.Count;
			nodes.Add(node);
			IReadOnlyList<FieldInfo> fields = UtilitySerializableFieldCache.GetFields(type);
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo fieldInfo = fields[i];
				if (!UtilitySerializableFieldCache.IsManagedReference(fieldInfo))
				{
					continue;
				}

				object fieldValue = fieldInfo.GetValue(instance);
				if (fieldValue == null)
				{
					continue;
				}

				if (!UtilitySerializableFieldCache.IsCollection(fieldInfo.FieldType))
				{
					AppendNode(nodes, fieldValue, index, fieldInfo.Name);
					continue;
				}

				if (fieldValue is not IList list)
				{
					continue;
				}

				for (int elementIndex = 0; elementIndex < list.Count; elementIndex++)
				{
					object element = list[elementIndex];
					if (element == null)
					{
						continue;
					}

					string elementField = string.Concat(
						fieldInfo.Name,
						IndexOpen,
						elementIndex.ToString(CultureInfo.InvariantCulture),
						IndexClose);

					AppendNode(nodes, element, index, elementField);
				}
			}

			return index;
		}

		private static void AssignChild(object parent, string field, object child)
		{
			if (parent == null || string.IsNullOrEmpty(field))
			{
				return;
			}

			string fieldName = field;
			int elementIndex = -1;
			int openIndex = field.IndexOf(IndexOpen, StringComparison.Ordinal);
			if (openIndex > 0 && field.EndsWith(IndexClose, StringComparison.Ordinal))
			{
				fieldName = field.Substring(0, openIndex);
				string indexText = field.Substring(openIndex + 1, field.Length - openIndex - 2);
				if (!int.TryParse(indexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out elementIndex))
				{
					return;
				}
			}

			FieldInfo fieldInfo = UtilitySerializableFieldCache.GetField(parent.GetType(), fieldName);
			if (fieldInfo == null)
			{
				return;
			}

			if (elementIndex < 0)
			{
				fieldInfo.SetValue(parent, child);
				return;
			}

			IList list = EnsureCollection(parent, fieldInfo, elementIndex);
			if (list == null)
			{
				return;
			}

			list[elementIndex] = child;
		}

		private static IList EnsureCollection(object parent, FieldInfo fieldInfo, int elementIndex)
		{
			Type elementType = UtilitySerializableFieldCache.GetElementType(fieldInfo.FieldType);
			if (elementType == null)
			{
				return null;
			}

			object value = fieldInfo.GetValue(parent);
			if (fieldInfo.FieldType.IsArray)
			{
				var array = value as Array;
				if (array == null || array.Length <= elementIndex)
				{
					Array grown = Array.CreateInstance(elementType, elementIndex + 1);
					array?.CopyTo(grown, 0);
					array = grown;
					fieldInfo.SetValue(parent, array);
				}

				return array as IList;
			}

			if (value is not IList list)
			{
				list = Activator.CreateInstance(fieldInfo.FieldType) as IList;
				if (list == null)
				{
					return null;
				}

				fieldInfo.SetValue(parent, list);
			}

			while (list.Count <= elementIndex)
			{
				list.Add(null);
			}

			return list;
		}

		private static Type ResolveType(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			Type type = Type.GetType(name, false);
			if (type != null)
			{
				return type;
			}

			int separatorIndex = name.IndexOf(',');
			string fullName = separatorIndex > 0 ? name.Substring(0, separatorIndex).Trim() : name;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type = assemblies[i].GetType(fullName, false);
				if (type != null)
				{
					return type;
				}
			}

			return null;
		}

		private static string GetTypeName(Type type) =>
			string.Concat(type.FullName, ", ", type.Assembly.GetName().Name);
	}
}
