using VContainer;

namespace DTech.UtilityAI.DI
{
	public sealed class VContainerUtilityActionInjector : IUtilityActionInjector
	{
		private readonly IObjectResolver _resolver;

		public VContainerUtilityActionInjector(IObjectResolver resolver)
		{
			_resolver = resolver;
		}

		public void Inject(object target)
		{
			if (target == null)
			{
				return;
			}

			_resolver.Inject(target);
		}
	}
}
