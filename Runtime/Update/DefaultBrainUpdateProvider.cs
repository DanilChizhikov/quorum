using System;

namespace DTech.UtilityAI
{
	internal sealed class DefaultBrainUpdateProvider : BrainUpdateProvider
	{
		public override void AddUpdate(Action updateAction)
		{
			UtilityUpdateService.Instance.RegisterUpdate(updateAction);
		}
	}
}