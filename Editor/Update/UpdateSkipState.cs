using System;

namespace DTech.Quorum.Editor.Update
{
	[Serializable]
	internal sealed class UpdateSkipState
	{
		public bool SkipAlways;
		public string SkippedVersion;
	}
}
