using System;

namespace DTech.UtilityAI
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class UtilityMenuAttribute : Attribute
	{
		public string Path { get; }
		public string IconName { get; }

		public UtilityMenuAttribute(string path, string iconName = null)
		{
			Path = path;
			IconName = iconName;
		}
	}
}
