using UnityEditor;
using UnityEngine;

namespace DTech.UtilityAI.Editor
{
	internal static class UtilityGenericMenuExtensions
	{
		public static void AddEntry(this GenericMenu menu, in UtilityMenuItem item)
		{
			var content = new GUIContent(item.Label);
			if (!item.Enabled || item.Action == null)
			{
				menu.AddDisabledItem(content);
				return;
			}

			menu.AddItem(content, false, item.Action);
		}
	}
}
