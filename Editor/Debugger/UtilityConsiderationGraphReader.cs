using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityConsiderationGraphReader
	{
		private const string ConsiderationFieldName = "_consideration";
		private const string EmptyLabel = "None";

		public static UtilityConsiderationNode ReadAction(IUtilityAction action)
		{
			if (action is not UtilityAction typedAction)
			{
				return null;
			}

			FieldInfo field = UtilitySerializableFieldCache.GetField(typedAction.GetType(), ConsiderationFieldName);
			return field?.GetValue(typedAction) is UtilityConsideration consideration ? Read(consideration) : null;
		}

		public static UtilityConsiderationNode Read(UtilityConsideration consideration)
		{
			if (consideration == null)
			{
				return null;
			}

			var visited = new HashSet<object>();
			return ReadNode(consideration, UtilityTypeRegistry.GetDisplayName(consideration.GetType()), visited);
		}

		private static UtilityConsiderationNode ReadNode(UtilityConsideration consideration, string label, HashSet<object> visited)
		{
			var node = new UtilityConsiderationNode(consideration, label);
			if (!visited.Add(consideration))
			{
				return node;
			}

			IReadOnlyList<FieldInfo> fields = UtilitySerializableFieldCache.GetFields(consideration.GetType());
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (!UtilitySerializableFieldCache.IsManagedReference(field))
				{
					continue;
				}

				object value = field.GetValue(consideration);
				if (value is UtilityConsideration child)
				{
					node.Add(ReadNode(child, UtilityTypeRegistry.GetDisplayName(child.GetType()), visited));
					continue;
				}

				if (value is not IList list)
				{
					continue;
				}

				for (int elementIndex = 0; elementIndex < list.Count; elementIndex++)
				{
					if (list[elementIndex] is not UtilityConsideration element)
					{
						node.Add(new UtilityConsiderationNode(null, GetElementLabel(elementIndex, EmptyLabel)));
						continue;
					}

					string elementLabel = GetElementLabel(elementIndex, UtilityTypeRegistry.GetDisplayName(element.GetType()));
					node.Add(ReadNode(element, elementLabel, visited));
				}
			}

			return node;
		}

		private static string GetElementLabel(int index, string name) =>
			string.Concat(index.ToString(CultureInfo.InvariantCulture), ": ", name);
	}
}
