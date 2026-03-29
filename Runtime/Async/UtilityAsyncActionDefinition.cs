using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public readonly struct UtilityAsyncActionDefinition
	{
		public UtilityAsyncActionDefinition(IAsyncUtilityAction action, IReadOnlyList<IAsyncUtilityConsideration> considerations)
		{
			Action = action ?? throw new ArgumentNullException(nameof(action));
			Considerations = considerations ?? throw new ArgumentNullException(nameof(considerations));
		}

		public IAsyncUtilityAction Action { get; }
		public IReadOnlyList<IAsyncUtilityConsideration> Considerations { get; }
	}
}
