#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
namespace DTech.Quorum.Diagnostics
{
	internal readonly struct BrainSwitchRecord
	{
		public double Time { get; }
		public int FromIndex { get; }
		public int ToIndex { get; }
		public float Score { get; }

		public BrainSwitchRecord(double time, int fromIndex, int toIndex, float score)
		{
			Time = time;
			FromIndex = fromIndex;
			ToIndex = toIndex;
			Score = score;
		}
	}
}
#endif
