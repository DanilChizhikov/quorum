namespace DTech.UtilityAI.Samples.Village
{
	/// <summary>
	/// Blackboard variable names shared by agents (writers) and considerations (readers).
	/// Agents sense the world into the blackboard, considerations only read it - which is why
	/// the Scoring Matrix can reproduce every score from a BlackboardAsset snapshot.
	/// </summary>
	public static class VillageKeys
	{
		/// <summary>Villager hunger, 0 = fed, 1 = starving.</summary>
		public const string Hunger = "Hunger";

		/// <summary>Villager energy, 0 = exhausted, 1 = rested.</summary>
		public const string Energy = "Energy";

		/// <summary>Distance in metres from a villager to the closest wolf.</summary>
		public const string ThreatDistance = "ThreatDistance";

		/// <summary>True when at least one food source exists in the world.</summary>
		public const string FoodAvailable = "FoodAvailable";

		/// <summary>Wolf stamina, 0 = exhausted, 1 = fresh.</summary>
		public const string Stamina = "Stamina";

		/// <summary>Wolf satiation, 1 right after a catch, decays to 0.</summary>
		public const string Satiation = "Satiation";

		/// <summary>Distance in metres from a wolf to the closest villager.</summary>
		public const string PreyDistance = "PreyDistance";

		/// <summary>Distance written when nothing is sensed.</summary>
		public const float FarAway = 99f;
	}
}
