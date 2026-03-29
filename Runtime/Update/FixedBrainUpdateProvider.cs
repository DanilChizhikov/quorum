using System;

namespace DTech.UtilityAI
{
	internal sealed class FixedBrainUpdateProvider : BrainUpdateProvider
	{
		public override void AddUpdate(Action updateAction)
		{
			UtilityUpdateService.Instance.RegisterFixUpdate(updateAction);
		}
	}
}