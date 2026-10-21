#if UNITY_EDITOR || QUORUM_DIAGNOSTICS
using System.Collections;
using System.Collections.Generic;

namespace DTech.Quorum.Diagnostics
{
	internal sealed class BrainSwitchLog : IReadOnlyList<BrainSwitchRecord>
	{
		private const int Capacity = 64;

		private readonly BrainSwitchRecord[] _records = new BrainSwitchRecord[Capacity];

		public int Count => _count;

		public BrainSwitchRecord this[int index] => _records[(_head - _count + index + Capacity) % Capacity];

		private int _count;
		private int _head;

		public void Add(in BrainSwitchRecord record)
		{
			_records[_head] = record;
			_head = (_head + 1) % Capacity;
			if (_count < Capacity)
			{
				_count++;
			}
		}

		public void Clear()
		{
			_count = 0;
			_head = 0;
		}

		public IEnumerator<BrainSwitchRecord> GetEnumerator()
		{
			for (int i = 0; i < _count; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
#endif
