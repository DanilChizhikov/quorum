using Zenject;

namespace DTech.Quorum.DI
{
	public sealed class ZenjectActionInjector : IQuorumActionInjector
	{
		private readonly DiContainer _container;

		public ZenjectActionInjector(DiContainer container)
		{
			_container = container;
		}

		public void Inject(object target)
		{
			if (target == null)
			{
				return;
			}

			_container.Inject(target);
		}
	}
}
