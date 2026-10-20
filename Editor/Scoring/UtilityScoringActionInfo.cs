using System.Collections.Generic;

namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityScoringActionInfo
	{
		public IReadOnlyList<string> Names { get; }
		public IReadOnlyList<string> TypeNames { get; }

		public UtilityScoringActionInfo(IReadOnlyList<string> names, IReadOnlyList<string> typeNames)
		{
			Names = names;
			TypeNames = typeNames;
		}
	}
}
