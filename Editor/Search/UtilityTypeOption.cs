using System;

namespace DTech.UtilityAI.Editor
{
	internal sealed class UtilityTypeOption
	{
		public Type Type { get; }
		public string Path { get; }
		public string IconName { get; }
		public string Name => System.IO.Path.GetFileName(Path);

		public UtilityTypeOption(Type type, string path, string iconName = null)
		{
			Type = type;
			Path = path;
			IconName = iconName;
		}
	}
}
