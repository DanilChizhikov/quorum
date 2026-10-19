using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	[Serializable]
	public abstract class UtilityScoreAggregator
	{
		public abstract int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context);
	}
}
