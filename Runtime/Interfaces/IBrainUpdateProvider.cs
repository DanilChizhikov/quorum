using System;

namespace DTech.UtilityAI
{
	public interface IBrainUpdateProvider
	{
		void AddUpdate(Action updateAction);
	}
}