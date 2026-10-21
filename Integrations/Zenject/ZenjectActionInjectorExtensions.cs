using Zenject;

namespace DTech.Quorum.DI
{
	public static class ZenjectActionInjectorExtensions
	{
		public static void BindQuorumActionInjector(this DiContainer container)
		{
			container.Bind<IQuorumActionInjector>()
				.To<ZenjectActionInjector>()
				.AsSingle();
		}
	}
}
