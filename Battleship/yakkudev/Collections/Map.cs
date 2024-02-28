using System.Collections;
using System.Collections.Generic;

namespace yakkudev.Collections {
	/// <summary>
	/// A bidirectional map implemented using two Dictionaries. Might break when changing values/keys that are primitives. Might fix.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class Map<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
		private Dictionary<TKey, TValue> forward = new Dictionary<TKey, TValue>();
		private Dictionary<TValue, TKey> reverse = new Dictionary<TValue, TKey>();

		public void Add(TKey key, TValue value) {
			forward[key] = value;
			reverse[value] = key;
		}

		public void Remove(TKey key) {
			reverse.Remove(forward[key]);
			forward.Remove(key);
		}

		public bool TryGetForward(TKey key, out TValue value) {
			return forward.TryGetValue(key, out value);
		}

		public bool TryGetReverse(TValue value, out TKey key) {
			return reverse.TryGetValue(value, out key);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return forward.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
