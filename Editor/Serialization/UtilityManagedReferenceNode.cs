using System;

namespace DTech.UtilityAI.Editor
{
	[Serializable]
	internal sealed class UtilityManagedReferenceNode
	{
		public int Parent = -1;
		public string Field = string.Empty;
		public string Type = string.Empty;
		public string Json = string.Empty;
	}
}
