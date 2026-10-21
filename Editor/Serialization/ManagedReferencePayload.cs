using System;

namespace DTech.Quorum.Editor
{
	[Serializable]
	internal sealed class ManagedReferencePayload
	{
		public ManagedReferenceNode[] Nodes = Array.Empty<ManagedReferenceNode>();
	}
}
