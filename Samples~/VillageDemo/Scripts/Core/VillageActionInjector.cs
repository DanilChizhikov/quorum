namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Implemented by actions that need the world service.
	/// </summary>
	public interface IVillageWorldConsumer
	{
		void Construct(VillageWorld world);
	}

	/// <summary>
	/// Minimal <see cref="IUtilityActionInjector"/>: the brain calls <see cref="Inject"/> once for every
	/// cloned action before <c>Initialize</c>. In a real project use the shipped adapters instead -
	/// <c>Container.BindUtilityActionInjector()</c> (Zenject) or <c>builder.RegisterUtilityActionInjector()</c>
	/// (VContainer) - and mark the action fields with <c>[Inject]</c>.
	/// </summary>
	public sealed class VillageActionInjector : IUtilityActionInjector
	{
		private readonly VillageWorld _world;

		public VillageActionInjector(VillageWorld world)
		{
			_world = world;
		}

		public void Inject(object target)
		{
			if (target is IVillageWorldConsumer consumer)
			{
				consumer.Construct(_world);
			}
		}
	}
}
