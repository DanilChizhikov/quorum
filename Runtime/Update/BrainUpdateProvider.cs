using System;

namespace DTech.UtilityAI
{
	internal abstract class BrainUpdateProvider : IBrainUpdateProvider
	{
		public abstract void AddUpdate(Action updateAction);
		
		public abstract void RemoveUpdate(Action updateAction);
	}
}
