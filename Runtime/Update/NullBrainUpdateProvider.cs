using System;

namespace DTech.Quorum
{
	internal sealed class NullBrainUpdateProvider : IBrainUpdateProvider
	{
		public void AddUpdate(Action updateAction) { }
		
		public void RemoveUpdate(Action updateAction) { }
	}
}
