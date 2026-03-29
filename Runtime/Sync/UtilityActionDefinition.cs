using System;
using System.Collections.Generic;

namespace DTech.UtilityAI
{
	public readonly struct UtilityActionDefinition
	{
		public UtilityActionDefinition(IUtilityAction action, IReadOnlyList<IUtilityConsideration> considerations)
		{
			Action = action ?? throw new ArgumentNullException(nameof(action));
			Considerations = considerations ?? throw new ArgumentNullException(nameof(considerations));
		}

		public IUtilityAction Action { get; }
		public IReadOnlyList<IUtilityConsideration> Considerations { get; }
	}
}
