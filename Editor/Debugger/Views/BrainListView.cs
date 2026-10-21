using System;
using System.Collections.Generic;
using DTech.Quorum.Diagnostics;
using UnityEngine.UIElements;

namespace DTech.Quorum.Editor
{
	internal sealed class BrainListView : VisualElement
	{
		public event Action SelectionChanged;

		private const string EmptyMessage = "No live brains. Enter play mode and create a brain.";

		private readonly ScrollView _scroll = new();
		private readonly List<IBrainDebugInfo> _brains = new();

		public IBrainDebugInfo Selected { get; private set; }

		public BrainListView()
		{
			AddToClassList("uai-brains");
			_scroll.AddToClassList("uai-brains__scroll");
			Add(_scroll);
		}

		public void Refresh(IReadOnlyList<IBrainDebugInfo> brains)
		{
			bool changed = HasChanged(brains);
			_brains.Clear();
			if (brains != null)
			{
				_brains.AddRange(brains);
			}

			if (!_brains.Contains(Selected))
			{
				Selected = _brains.Count > 0 ? _brains[0] : null;
				changed = true;
			}

			if (!changed)
			{
				return;
			}

			Rebuild();
		}

		public bool TrySelect(IBrainDebugInfo brain)
		{
			if (brain == null || !_brains.Contains(brain))
			{
				return false;
			}

			Select(brain);
			return true;
		}

		private bool HasChanged(IReadOnlyList<IBrainDebugInfo> brains)
		{
			int count = brains?.Count ?? 0;
			if (count != _brains.Count)
			{
				return true;
			}

			for (int i = 0; i < count; i++)
			{
				if (ReferenceEquals(brains[i], _brains[i]))
				{
					continue;
				}

				return true;
			}

			return false;
		}

		private void Rebuild()
		{
			_scroll.Clear();
			if (_brains.Count <= 0)
			{
				var empty = new Label(EmptyMessage);
				empty.AddToClassList("uai-empty");
				_scroll.Add(empty);
				return;
			}

			for (int i = 0; i < _brains.Count; i++)
			{
				_scroll.Add(CreateRow(_brains[i]));
			}
		}

		private VisualElement CreateRow(IBrainDebugInfo brain)
		{
			var row = new Button(() => Select(brain))
			{
				text = brain.ConfigName,
			};

			row.AddToClassList("uai-brains__row");
			if (ReferenceEquals(brain, Selected))
			{
				row.AddToClassList("uai-brains__row--selected");
			}

			if (!brain.IsEnabled)
			{
				row.AddToClassList("uai-brains__row--disabled");
			}

			return row;
		}

		private void Select(IBrainDebugInfo brain)
		{
			if (ReferenceEquals(brain, Selected))
			{
				return;
			}

			Selected = brain;
			Rebuild();
			SelectionChanged?.Invoke();
		}
	}
}
