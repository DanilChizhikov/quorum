using System.Collections.Generic;

namespace DTech.UtilityAI.Tests.EditorMode
{
	internal sealed class RecordingUtilityActionInjector : IUtilityActionInjector
	{
		private readonly List<object> _targets = new();
		private readonly List<int> _initializeCounts = new();
		
		public IReadOnlyList<object> Targets => _targets;
		public IReadOnlyList<int> InitializeCountsOnInject => _initializeCounts;
		
		public void Inject(object target)
		{
			_targets.Add(target);
			_initializeCounts.Add(target is TestUtilityAction action ? action.InitializeCount : -1);
		}
	}
}
