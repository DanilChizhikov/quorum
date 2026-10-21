using System.Collections.Generic;

namespace DTech.Quorum.Editor
{
	internal readonly struct ScoringActionInfo
	{
		public IReadOnlyList<string> Names { get; }
		public IReadOnlyList<string> TypeNames { get; }

		public ScoringActionInfo(IReadOnlyList<string> names, IReadOnlyList<string> typeNames)
		{
			Names = names;
			TypeNames = typeNames;
		}
	}
}
