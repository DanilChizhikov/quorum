using UnityEditor;
using UnityEngine;

namespace DTech.Quorum.Editor
{
	internal static class GenericMenuExtensions
	{
		public static void AddEntry(this GenericMenu menu, in MenuEntry item)
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
