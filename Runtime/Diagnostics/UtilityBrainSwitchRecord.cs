#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace DTech.UtilityAI.Diagnostics
{
	internal readonly struct UtilityBrainSwitchRecord
	{
		public double Time { get; }
		public int FromIndex { get; }
		public int ToIndex { get; }
		public float Score { get; }

		public UtilityBrainSwitchRecord(double time, int fromIndex, int toIndex, float score)
		{
			Time = time;
			FromIndex = fromIndex;
			ToIndex = toIndex;
			Score = score;
		}
	}
}
#endif
