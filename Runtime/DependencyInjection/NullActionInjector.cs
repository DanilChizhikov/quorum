namespace DTech.Quorum
{
	internal sealed class NullActionInjector : IQuorumActionInjector
	{
		public void Inject(object target) { }
	}
}
