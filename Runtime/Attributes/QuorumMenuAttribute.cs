using System;

namespace DTech.Quorum
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class QuorumMenuAttribute : Attribute
	{
		public string Path { get; }
		public string IconName { get; }

		public QuorumMenuAttribute(string path, string iconName = null)
		{
			Path = path;
			IconName = iconName;
		}
	}
}
