using Zenject;

namespace DTech.UtilityAI.DI
{
	public static class ZenjectUtilityActionInjectorExtensions
	{
		public static void BindUtilityActionInjector(this DiContainer container)
		{
			container.Bind<IUtilityActionInjector>()
				.To<ZenjectUtilityActionInjector>()
				.AsSingle();
		}
	}
}
