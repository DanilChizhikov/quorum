using Zenject;

namespace DTech.UtilityAI.DI
{
	public sealed class ZenjectUtilityActionInjector : IUtilityActionInjector
	{
		private readonly DiContainer _container;

		public ZenjectUtilityActionInjector(DiContainer container)
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
