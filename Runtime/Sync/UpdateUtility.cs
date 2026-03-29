using System;

namespace DTech.UtilityAI
{
	internal static class UpdateUtility
	{
		public static IBrainUpdateProvider GetProvider(BrainUpdateType type) =>
			type switch
			{
				BrainUpdateType.Update => new DefaultBrainUpdateProvider(),
				BrainUpdateType.FixedUpdate => new FixedBrainUpdateProvider(),
				BrainUpdateType.Custom => new NullBrainUpdateProvider(),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
	}
}