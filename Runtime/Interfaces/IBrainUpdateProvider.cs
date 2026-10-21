using System;

namespace DTech.Quorum
{
	public interface IBrainUpdateProvider
	{
		void AddUpdate(Action updateAction);
		void RemoveUpdate(Action updateAction);
	}
}
