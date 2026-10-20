#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;

namespace DTech.UtilityAI.Diagnostics
{
	internal sealed class UtilityBrainSwitchLog : IReadOnlyList<UtilityBrainSwitchRecord>
	{
		private const int Capacity = 64;

		private readonly UtilityBrainSwitchRecord[] _records = new UtilityBrainSwitchRecord[Capacity];

		public int Count => _count;

		public UtilityBrainSwitchRecord this[int index] => _records[(_head - _count + index + Capacity) % Capacity];

		private int _count;
		private int _head;

		public void Add(in UtilityBrainSwitchRecord record)
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

		public IEnumerator<UtilityBrainSwitchRecord> GetEnumerator()
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
