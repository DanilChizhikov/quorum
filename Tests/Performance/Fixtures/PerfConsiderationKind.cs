namespace DTech.UtilityAI.Tests.Performance
{
	internal enum PerfConsiderationKind : byte
	{
		None = 0,
		Constant = 1,
		CompositeConstants = 2,
		CompositeBlackboard = 3,
		CompositeBlackboardRaw = 4,
		CompositeCurve = 5,
		CompositeBlackboardCached = 6,
	}
}
