using System;

namespace DTech.Quorum.Editor
{
	internal sealed class TypeOption
	{
		public Type Type { get; }
		public string Path { get; }
		public string IconName { get; }
		public string Name => System.IO.Path.GetFileName(Path);

		public TypeOption(Type type, string path, string iconName = null)
		{
			Type = type;
			Path = path;
			IconName = iconName;
		}
	}
}
