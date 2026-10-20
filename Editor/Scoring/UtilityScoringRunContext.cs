using DTech.UtilityAI.Diagnostics;

namespace DTech.UtilityAI.Editor
{
	internal readonly struct UtilityScoringRunContext
	{
		public BrainConfig Config { get; }
		public UtilityScoringProbe Probe { get; }

		public UtilityScoringRunContext(BrainConfig config, UtilityScoringProbe probe)
		{
			Config = config;
			Probe = probe;
		}
	}
}
