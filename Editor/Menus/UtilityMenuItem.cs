using UnityEditor;

namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityMenuItem
	{
		public string Label { get; }
		public bool Enabled { get; }
		public GenericMenu.MenuFunction Action { get; }

		public UtilityMenuItem(string label, bool enabled, GenericMenu.MenuFunction action)
		{
			Label = label;
			Enabled = enabled;
			Action = action;
		}
	}
}
