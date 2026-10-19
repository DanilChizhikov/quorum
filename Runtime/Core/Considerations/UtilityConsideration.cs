using System;

namespace DTech.UtilityAI
{
	[Serializable]
	public abstract class UtilityConsideration
	{
		public abstract float Evaluate(IUtilityContext context);
	}
}
