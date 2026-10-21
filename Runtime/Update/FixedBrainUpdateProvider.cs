using System;

namespace DTech.Quorum
{
	internal sealed class FixedBrainUpdateProvider : BrainUpdateProvider
	{
		public override void AddUpdate(Action updateAction)
		{
			BrainUpdateService.Instance.RegisterFixUpdate(updateAction);
		}
		
		public override void RemoveUpdate(Action updateAction)
		{
			BrainUpdateService.Instance.UnregisterFixUpdate(updateAction);
		}
	}
}
