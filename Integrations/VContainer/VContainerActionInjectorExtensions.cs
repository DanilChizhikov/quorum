using VContainer;

namespace DTech.Quorum.DI
{
	public static class VContainerActionInjectorExtensions
	{
		public static void RegisterQuorumActionInjector(this IContainerBuilder builder)
		{
			builder.Register<IQuorumActionInjector, VContainerActionInjector>(Lifetime.Singleton);
		}
	}
}
