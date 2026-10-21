using System;
using UnityEngine.Scripting;

namespace DTech.UtilityAI
{
	[Serializable]
	[Preserve]
	public abstract class UtilityConsideration
	{
		/// <summary>
		/// Called once per brain, before the first evaluation. Resolve and cache everything the score
		/// depends on here, instead of looking it up on every Evaluate call.
		/// Not called for a consideration that lives on a ScriptableUtilityAction asset: that asset is
		/// shared by every brain, so per-agent data cached in it would leak between agents.
		/// </summary>
		public virtual void Initialize(IUtilityContext context) { }

		/// <summary>
		/// Creates the per-brain copy of this consideration. The default is a shallow field copy,
		/// which is what config data needs: value fields are duplicated and references to shared
		/// assets stay shared. Override it when a reference field holds per-agent state that two
		/// brains must not share.
		/// </summary>
		public virtual UtilityConsideration Clone() => (UtilityConsideration)MemberwiseClone();

		public abstract float Evaluate(IUtilityContext context);
	}
}
