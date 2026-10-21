using System;

namespace DTech.Quorum.Editor.Update
{
	[Serializable]
	internal sealed class GitHubRelease
	{
		public string tag_name;
		public string name;
		public string body;
		public string html_url;
		public bool draft;
		public bool prerelease;
	}
}
