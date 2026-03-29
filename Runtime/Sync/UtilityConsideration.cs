using UnityEngine;

namespace DTech.UtilityAI
{
	public abstract class UtilityConsideration : ScriptableObject
	{
		public abstract float Evaluate(IUtilityContext context);
	}
}