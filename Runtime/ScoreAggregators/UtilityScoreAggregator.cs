using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	[Serializable]
	public abstract class UtilityScoreAggregator
	{
		/// <summary>
		/// Creates the per-brain copy of this aggregator. The default is a shallow field copy.
		/// Override it when a reference field holds state that two brains must not share.
		/// </summary>
		/// <returns>Copy of this aggregator for one brain.</returns>
		public virtual UtilityScoreAggregator Clone() => (UtilityScoreAggregator)MemberwiseClone();

		public abstract int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context);
	}
}
