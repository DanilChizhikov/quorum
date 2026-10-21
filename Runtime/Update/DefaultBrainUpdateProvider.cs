using System;

namespace DTech.Quorum
{
	internal sealed class DefaultBrainUpdateProvider : BrainUpdateProvider
	{
		public override void AddUpdate(Action updateAction)
		{
			BrainUpdateService.Instance.RegisterUpdate(updateAction);
		}
		
		public override void RemoveUpdate(Action updateAction)
		{
			BrainUpdateService.Instance.UnregisterUpdate(updateAction);
		}
	}
}
