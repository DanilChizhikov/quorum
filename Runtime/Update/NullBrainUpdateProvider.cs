using System;

namespace DTech.UtilityAI
{
	internal sealed class NullBrainUpdateProvider : IBrainUpdateProvider
	{
		public void AddUpdate(Action updateAction) { }
		
		public void RemoveUpdate(Action updateAction) { }
	}
}
