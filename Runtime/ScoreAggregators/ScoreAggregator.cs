using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace DTech.Quorum
{
	[Serializable]
	[Preserve]
	public abstract class ScoreAggregator
	{
		/// <summary>
		/// Creates the per-brain copy of this aggregator. The default is a shallow field copy.
		/// Override it when a reference field holds state that two brains must not share.
		/// </summary>
		public virtual ScoreAggregator Clone() => (ScoreAggregator)MemberwiseClone();

		public abstract int Aggregate(IReadOnlyList<IQuorumAction> actions, IQuorumContext context);
	}
}
