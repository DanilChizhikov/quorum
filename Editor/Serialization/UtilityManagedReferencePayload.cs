using System;

namespace DTech.UtilityAI.Editor
{
	[Serializable]
	internal sealed class UtilityManagedReferencePayload
	{
		public UtilityManagedReferenceNode[] Nodes = Array.Empty<UtilityManagedReferenceNode>();
	}
}
