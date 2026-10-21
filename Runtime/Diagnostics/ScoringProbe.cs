using DTech.Blackboard;
using UnityEngine;

namespace DTech.Quorum.Diagnostics
{
	/// <summary>
	/// Builds a context for offline scoring, where no agent exists yet.
	/// Create one asset per agent kind and point a scoring scenario at it.
	/// </summary>
	public abstract class ScoringProbe : ScriptableObject
	{
		/// <summary>
		/// Creates the context the scored actions receive.
		/// </summary>
		/// <param name="blackboard">Blackboard built from the scenario case.</param>
		/// <returns>Context for one scoring case.</returns>
		public abstract IQuorumContext CreateContext(RuntimeBlackboard blackboard);

		/// <summary>
		/// Creates the injector used on the scored actions.
		/// Return null when the actions need no dependencies.
		/// </summary>
		/// <returns>Injector for the scored actions, or null.</returns>
		public virtual IQuorumActionInjector CreateInjector() => null;
	}
}
