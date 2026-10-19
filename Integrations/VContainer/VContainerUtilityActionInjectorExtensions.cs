using VContainer;

namespace DTech.UtilityAI.DI
{
	public static class VContainerUtilityActionInjectorExtensions
	{
		public static void RegisterUtilityActionInjector(this IContainerBuilder builder)
		{
			builder.Register<IUtilityActionInjector, VContainerUtilityActionInjector>(Lifetime.Singleton);
		}
	}
}
