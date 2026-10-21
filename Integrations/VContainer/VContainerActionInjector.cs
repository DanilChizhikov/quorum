using VContainer;

namespace DTech.Quorum.DI
{
	public sealed class VContainerActionInjector : IQuorumActionInjector
	{
		private readonly IObjectResolver _resolver;

		public VContainerActionInjector(IObjectResolver resolver)
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
