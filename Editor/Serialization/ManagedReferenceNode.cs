using System;

namespace DTech.Quorum.Editor
{
	[Serializable]
	internal sealed class ManagedReferenceNode
	{
		public int Parent = -1;
		public string Field = string.Empty;
		public string Type = string.Empty;
		public string Json = string.Empty;
	}
}
