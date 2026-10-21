using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace DTech.Quorum.Editor
{
	internal static class ConsiderationGraphReader
	{
		private const string ConsiderationFieldName = "_consideration";
		private const string EmptyLabel = "None";

		public static ConsiderationNode ReadAction(IQuorumAction action)
		{
			if (action is not QuorumAction typedAction)
			{
				return null;
			}

			FieldInfo field = SerializableFieldCache.GetField(typedAction.GetType(), ConsiderationFieldName);
			return field?.GetValue(typedAction) is Consideration consideration ? Read(consideration) : null;
		}

		public static ConsiderationNode Read(Consideration consideration)
		{
			if (consideration == null)
			{
				return null;
			}

			var visited = new HashSet<object>();
			return ReadNode(consideration, TypeRegistry.GetDisplayName(consideration.GetType()), visited);
		}

		private static ConsiderationNode ReadNode(Consideration consideration, string label, HashSet<object> visited)
		{
			var node = new ConsiderationNode(consideration, label);
			if (!visited.Add(consideration))
			{
				return node;
			}

			IReadOnlyList<FieldInfo> fields = SerializableFieldCache.GetFields(consideration.GetType());
			for (int i = 0; i < fields.Count; i++)
			{
				FieldInfo field = fields[i];
				if (!SerializableFieldCache.IsManagedReference(field))
				{
					continue;
				}

				object value = field.GetValue(consideration);
				if (value is Consideration child)
				{
					node.Add(ReadNode(child, TypeRegistry.GetDisplayName(child.GetType()), visited));
					continue;
				}

				if (value is not IList list)
				{
					continue;
				}

				for (int elementIndex = 0; elementIndex < list.Count; elementIndex++)
				{
					if (list[elementIndex] is not Consideration element)
					{
						node.Add(new ConsiderationNode(null, GetElementLabel(elementIndex, EmptyLabel)));
						continue;
					}

					string elementLabel = GetElementLabel(elementIndex, TypeRegistry.GetDisplayName(element.GetType()));
					node.Add(ReadNode(element, elementLabel, visited));
				}
			}

			return node;
		}

		private static string GetElementLabel(int index, string name) =>
			string.Concat(index.ToString(CultureInfo.InvariantCulture), ": ", name);
	}
}
