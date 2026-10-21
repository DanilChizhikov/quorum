using System;

namespace DTech.UtilityAI
{
	internal static class UpdateUtility
	{
		private static readonly IBrainUpdateProvider _updateProvider = new DefaultBrainUpdateProvider();
		private static readonly IBrainUpdateProvider _fixedUpdateProvider = new FixedBrainUpdateProvider();
		private static readonly IBrainUpdateProvider _nullProvider = new NullBrainUpdateProvider();

		public static IBrainUpdateProvider GetProvider(BrainUpdateType type) =>
			type switch
			{
				BrainUpdateType.Update => _updateProvider,
				BrainUpdateType.FixedUpdate => _fixedUpdateProvider,
				BrainUpdateType.Custom => _nullProvider,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
	}
}