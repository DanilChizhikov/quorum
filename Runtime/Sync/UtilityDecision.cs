namespace DTech.UtilityAI
{
	public readonly struct UtilityDecision<TAction>
		where TAction : class
	{
		public static UtilityDecision<TAction> None => new(null, float.NegativeInfinity, -1);

		public UtilityDecision(TAction action, float score, int index)
		{
			Action = action;
			Score = score;
			Index = index;
		}

		public TAction Action { get; }
		public float Score { get; }
		public int Index { get; }
		public bool HasDecision => Action != null;
	}
}
