using UnityEditor;

namespace DTech.Quorum.Editor
{
	internal readonly struct MenuEntry
	{
		public string Label { get; }
		public bool Enabled { get; }
		public GenericMenu.MenuFunction Action { get; }

		public MenuEntry(string label, bool enabled, GenericMenu.MenuFunction action)
		{
			Label = label;
			Enabled = enabled;
			Action = action;
		}
	}
}
