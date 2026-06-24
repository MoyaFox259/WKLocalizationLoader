using System;
using System.Collections.Generic;

namespace WKLocalizationLoader
{
    public class ValueCollection<TKey, TValue>
    {
        private Dictionary<TKey, List<TValue>> _dictionary;

        public void Add(TKey key, TValue value)
        {
            _dictionary ??= new Dictionary<TKey, List<TValue>>();
            if (key is null || value is null) return;
            if (
                TryGetValues(key, out List<TValue> values)
                && !values.Contains(value)
            )
            {
                values.Add(value);
            }
            else
            {
                _dictionary[key] = new List<TValue>() { value };
            }
        }

        public bool TryGetValues(TKey key, out List<TValue> values)
        {
            if (
                key is null
                || _dictionary is null
                || !_dictionary.TryGetValue(key, out values)
                || values is null
            )
            {
                values = null;
                return false;
            }
            return true;
        }
    }
}

