using System.Collections.Generic;
using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class UtilityScoreAggregator : ScriptableObject
	{
		public abstract int Aggregate(IReadOnlyList<IUtilityAction> actions, IUtilityContext context);
	}
}
