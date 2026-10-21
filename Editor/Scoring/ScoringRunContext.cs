using DTech.Quorum.Diagnostics;

namespace DTech.Quorum.Editor
{
	internal readonly struct ScoringRunContext
	{
		public BrainConfig Config { get; }
		public ScoringProbe Probe { get; }

		public ScoringRunContext(BrainConfig config, ScoringProbe probe)
		{
			Config = config;
			Probe = probe;
		}
	}
}
