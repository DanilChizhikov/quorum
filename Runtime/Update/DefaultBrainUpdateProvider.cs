using System;

namespace DTech.UtilityAI
{
	internal sealed class DefaultBrainUpdateProvider : BrainUpdateProvider
	{
		public override void AddUpdate(Action updateAction)
		{
			UtilityUpdateService.Instance.RegisterUpdate(updateAction);
		}
		
		public override void RemoveUpdate(Action updateAction)
		{
			UtilityUpdateService.Instance.UnregisterUpdate(updateAction);
		}
	}
}
