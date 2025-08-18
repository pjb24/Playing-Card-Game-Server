using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// 키가 추가된 순서를 보장하는 스레드에 안전한 제네릭 딕셔너리 클래스입니다.
/// </summary>
/// <typeparam name="TKey">키의 타입</typeparam>
/// <typeparam name="TValue">값의 타입</typeparam>
public class CustomOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private readonly object _lock = new object();
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
    private readonly List<TKey> _orderedKeys;

    public CustomOrderedDictionary()
    {
        _dictionary = new ConcurrentDictionary<TKey, TValue>();
        _orderedKeys = new List<TKey>();
    }
    
    public CustomOrderedDictionary(int concurrencyLevel, int capacity)
    {
        _dictionary = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity);
        _orderedKeys = new List<TKey>(capacity);
    }

    /// <summary>
    /// 인덱서: 키를 사용하여 값에 접근합니다.
    /// </summary>
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            lock (_lock)
            {
                if (!_dictionary.ContainsKey(key))
                {
                    _orderedKeys.Add(key);
                }
                _dictionary[key] = value;
            }
        }
    }

    /// <summary>
    /// 순서가 보장된 키 컬렉션의 스냅샷을 반환합니다.
    /// </summary>
    public ICollection<TKey> Keys
    {
        get
        {
            lock (_lock)
            {
                return _orderedKeys.ToList();
            }
        }
    }

    /// <summary>
    /// 순서가 보장된 값 컬렉션의 스냅샷을 반환합니다.
    /// </summary>
    public ICollection<TValue> Values
    {
        get
        {
            lock (_lock)
            {
                return _orderedKeys.Select(key => _dictionary[key]).ToList();
            }
        }
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// 딕셔너리에 요소를 추가합니다.
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_dictionary.TryAdd(key, value))
            {
                _orderedKeys.Add(key);
            }
            else
            {
                throw new ArgumentException("키가 이미 존재합니다.", nameof(key));
            }
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// 딕셔너리의 모든 요소를 제거합니다.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _dictionary.Clear();
            _orderedKeys.Clear();
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.Contains(item);
    }

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("복사 대상 배열이 충분히 크지 않습니다.", nameof(array));

        lock (_lock)
        {
            int i = 0;
            foreach (var key in _orderedKeys)
            {
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
                i++;
            }
        }
    }

    /// <summary>
    /// 순서가 보장된 열거자를 반환합니다. (스냅샷 기반)
    /// </summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        List<TKey> keysSnapshot;
        lock (_lock)
        {
            keysSnapshot = _orderedKeys.ToList();
        }

        foreach (var key in keysSnapshot)
        {
            yield return new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
        }
    }

    /// <summary>
    /// 딕셔너리에서 특정 키를 가진 요소를 제거합니다.
    /// </summary>
    public bool Remove(TKey key)
    {
        lock (_lock)
        {
            if (_dictionary.TryRemove(key, out _))
            {
                _orderedKeys.Remove(key);
                return true;
            }
            return false;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (_lock)
        {
            if (_dictionary.TryGetValue(item.Key, out TValue? value) && EqualityComparer<TValue>.Default.Equals(value, item.Value))
            {
                if (_dictionary.TryRemove(item.Key, out _))
                {
                    _orderedKeys.Remove(item.Key);
                    return true;
                }
            }
            return false;
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
