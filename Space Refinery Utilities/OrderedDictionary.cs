using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;

// CREDIT: MIT, https://github.com/mattmc3/dotmore/blob/master/dotmore/Collections/Generic/OrderedDictionary.cs
// MODIFIED: for performance and specialized use cases

namespace Space_Refinery_Utilities;

/// <summary>
/// A dictionary object that allows rapid hash lookups using keys, but also
/// maintains the key insertion order so that values can be retrieved by
/// key index.
/// </summary>
/// <remarks>
/// Similar to the way a DataColumn is indexed by column position and by column name, this
/// advanced dictionary construct allows for a very natural and robust handling of indexed
/// structured data.
/// </remarks>
[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(OrderedDictionaryDebugView))]
public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
{
	#region Fields/Properties

	private KeyedCollection2<TKey, KeyValuePair<TKey, TValue>> _keyedCollection;

	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key associated with the value to get or set.</param>
	public TValue this[TKey key]
	{
		get
		{
			return GetValue(key);
		}
		set
		{
			SetValue(key, value);
		}
	}

	/// <summary>
	/// Gets or sets the value at the specified index.
	/// </summary>
	/// <param name="index">The index of the value to get or set.</param>
	public TValue this[int index]
	{
		get
		{
			return GetItem(index).Value;
		}
		set
		{
			SetItem(index, value);
		}
	}


	/// <summary>
	/// Gets the number of items in the dictionary
	/// </summary>
	public int Count
	{
		get { return _keyedCollection.Count; }
	}

	/// <summary>
	/// Gets all the keys in the ordered dictionary in their proper order.
	/// </summary>
	public ICollection<TKey> Keys
	{
		get
		{
			return _keyedCollection.Select(x => x.Key).ToList();
		}
	}

	/// <summary>
	/// Gets all the values in the ordered dictionary in their proper order.
	/// </summary>
	public ICollection<TValue> Values
	{
		get
		{
			return _keyedCollection.Select(x => x.Value).ToList();
		}
	}

	/*public TValue[] ValuesRawAccess
	{
		get
		{
			//return _keyedCollection.Items.Items;

			return _keyedCollection.Items.Items.
		}
	}*/

	/// <summary>
	/// Gets the key comparer for this dictionary
	/// </summary>
	public IEqualityComparer<TKey> Comparer
	{
		get;
		private set;
	}

	#endregion

	#region Constructors

	public OrderedDictionary()
	{
		Initialize();
	}

	public OrderedDictionary(IEqualityComparer<TKey> comparer)
	{
		Initialize(comparer);
	}

	public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary)
	{
		Initialize();
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			_keyedCollection.Add(pair);
		}
	}

	public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		Initialize(comparer);
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			_keyedCollection.Add(pair);
		}
	}

	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		Initialize();
		foreach (KeyValuePair<TKey, TValue> pair in items)
		{
			_keyedCollection.Add(pair);
		}
	}

	public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
	{
		Initialize(comparer);
		foreach (KeyValuePair<TKey, TValue> pair in items)
		{
			_keyedCollection.Add(pair);
		}
	}

	#endregion

	#region Methods

	private void Initialize(IEqualityComparer<TKey> comparer = null)
	{
		this.Comparer = comparer;
		if (comparer != null)
		{
			_keyedCollection = new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>(x => x.Key, comparer);
		}
		else
		{
			_keyedCollection = new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>(x => x.Key);
		}
	}

	/// <summary>
	/// Adds the specified key and value to the dictionary.
	/// </summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add.  The value can be null for reference types.</param>
	public void Add(TKey key, TValue value)
	{
		_keyedCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
	}

	/// <summary>
	/// Removes all keys and values from this object.
	/// </summary>
	public void Clear()
	{
		_keyedCollection.Clear();
	}

	/// <summary>
	/// Inserts a new key-value pair at the index specified.
	/// </summary>
	/// <param name="index">The insertion index.  This value must be between 0 and the count of items in this object.</param>
	/// <param name="key">A unique key for the element to add</param>
	/// <param name="value">The value of the element to add.  Can be null for reference types.</param>
	public void Insert(int index, TKey key, TValue value)
	{
		_keyedCollection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
	}

	/// <summary>
	/// Gets the index of the key specified.
	/// </summary>
	/// <param name="key">The key whose index will be located</param>
	/// <returns>Returns the index of the key specified if found.  Returns -1 if the key could not be located.</returns>
	public int IndexOf(TKey key)
	{
		if (_keyedCollection.Contains(key))
		{
			return _keyedCollection.IndexOf(_keyedCollection[key]);
		}
		else
		{
			return -1;
		}
	}

	/// <summary>
	/// Determines whether this object contains the specified value.
	/// </summary>
	/// <param name="value">The value to locate in this object.</param>
	/// <returns>True if the value is found.  False otherwise.</returns>
	public bool ContainsValue(TValue value)
	{
		return this.Values.Contains(value);
	}

	/// <summary>
	/// Determines whether this object contains the specified value.
	/// </summary>
	/// <param name="value">The value to locate in this object.</param>
	/// <param name="comparer">The equality comparer used to locate the specified value in this object.</param>
	/// <returns>True if the value is found.  False otherwise.</returns>
	public bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer)
	{
		return this.Values.Contains(value, comparer);
	}

	/// <summary>
	/// Determines whether this object contains the specified key.
	/// </summary>
	/// <param name="key">The key to locate in this object.</param>
	/// <returns>True if the key is found.  False otherwise.</returns>
	public bool ContainsKey(TKey key)
	{
		return _keyedCollection.Contains(key);
	}

	/// <summary>
	/// Returns the KeyValuePair at the index specified.
	/// </summary>
	/// <param name="index">The index of the KeyValuePair desired</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when the index specified does not refer to a KeyValuePair in this object
	/// </exception>
	public KeyValuePair<TKey, TValue> GetItem(int index)
	{
		if (index < 0 || index >= _keyedCollection.Count)
		{
			throw new ArgumentException($"The index was outside the bounds of the dictionary: {index}");
		}
		return _keyedCollection[index];
	}

	/// <summary>
	/// Sets the value at the index specified.
	/// </summary>
	/// <param name="index">The index of the value desired</param>
	/// <param name="value">The value to set</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when the index specified does not refer to a KeyValuePair in this object
	/// </exception>
	public void SetItem(int index, TValue value)
	{
		if (index < 0 || index >= _keyedCollection.Count)
		{
			throw new ArgumentException($"The index is outside the bounds of the dictionary: {index}");
		}
		var kvp = new KeyValuePair<TKey, TValue>(_keyedCollection[index].Key, value);
		_keyedCollection[index] = kvp;
	}

	/// <summary>
	/// Returns an enumerator that iterates through all the KeyValuePairs in this object.
	/// </summary>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _keyedCollection.GetEnumerator();
	}

	/// <summary>
	/// Removes the key-value pair for the specified key.
	/// </summary>
	/// <param name="key">The key to remove from the dictionary.</param>
	/// <returns>True if the item specified existed and the removal was successful.  False otherwise.</returns>
	public bool Remove(TKey key)
	{
		return _keyedCollection.Remove(key);
	}

	/// <summary>
	/// Removes the key-value pair at the specified index.
	/// </summary>
	/// <param name="index">The index of the key-value pair to remove from the dictionary.</param>
	public void RemoveAt(int index)
	{
		if (index < 0 || index >= _keyedCollection.Count)
		{
			throw new ArgumentException($"The index was outside the bounds of the dictionary: {index}");
		}
		_keyedCollection.RemoveAt(index);
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key associated with the value to get.</param>
	public TValue GetValue(TKey key)
	{
		if (_keyedCollection.Contains(key) == false)
		{
			throw new ArgumentException($"The given key is not present in the dictionary: {key}");
		}
		var kvp = _keyedCollection[key];
		return kvp.Value;
	}

	/// <summary>
	/// Sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key associated with the value to set.</param>
	/// <param name="value">The the value to set.</param>
	public void SetValue(TKey key, TValue value)
	{
		var kvp = new KeyValuePair<TKey, TValue>(key, value);
		var idx = IndexOf(key);
		if (idx > -1)
		{
			_keyedCollection[idx] = kvp;
		}
		else
		{
			_keyedCollection.Add(kvp);
		}
	}

	/// <summary>
	/// Tries to get the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key of the desired element.</param>
	/// <param name="value">
	/// When this method returns, contains the value associated with the specified key if
	/// that key was found.  Otherwise it will contain the default value for parameter's type.
	/// This parameter should be provided uninitialized.
	/// </param>
	/// <returns>True if the value was found.  False otherwise.</returns>
	/// <remarks></remarks>
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (_keyedCollection.Contains(key))
		{
			value = _keyedCollection[key].Value;
			return true;
		}
		else
		{
			value = default(TValue);
			return false;
		}
	}

	#endregion

	#region Sorting
	public void SortKeys()
	{
		_keyedCollection.SortByKeys();
	}

	public void SortKeys(IComparer<TKey> comparer)
	{
		_keyedCollection.SortByKeys(comparer);
	}

	public void SortKeys(Comparison<TKey> comparison)
	{
		_keyedCollection.SortByKeys(comparison);
	}

	public void SortValues()
	{
		var comparer = Comparer<TValue>.Default;
		SortValues(comparer);
	}

	public void SortValues(IComparer<TValue> comparer)
	{
		_keyedCollection.Sort((x, y) => comparer.Compare(x.Value, y.Value));
	}

	public void SortValues(Comparison<TValue> comparison)
	{
		_keyedCollection.Sort((x, y) => comparison(x.Value, y.Value));
	}
	#endregion

	#region IDictionary<TKey, TValue>

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		Add(key, value);
	}

	bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
	{
		return ContainsKey(key);
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys
	{
		get { return Keys; }
	}

	bool IDictionary<TKey, TValue>.Remove(TKey key)
	{
		return Remove(key);
	}

	bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
	{
		return TryGetValue(key, out value);
	}

	ICollection<TValue> IDictionary<TKey, TValue>.Values
	{
		get { return Values; }
	}

	TValue IDictionary<TKey, TValue>.this[TKey key]
	{
		get
		{
			return this[key];
		}
		set
		{
			this[key] = value;
		}
	}

	#endregion

	#region ICollection<KeyValuePair<TKey, TValue>>

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		_keyedCollection.Add(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Clear()
	{
		_keyedCollection.Clear();
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		return _keyedCollection.Contains(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		_keyedCollection.CopyTo(array, arrayIndex);
	}

	int ICollection<KeyValuePair<TKey, TValue>>.Count
	{
		get { return _keyedCollection.Count; }
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
	{
		get { return false; }
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		return _keyedCollection.Remove(item);
	}

	#endregion

	#region IEnumerable<KeyValuePair<TKey, TValue>>

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion

	#region IEnumerable

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion

	#region IOrderedDictionary

	IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
	{
		return new DictionaryEnumerator<TKey, TValue>(this);
	}

	void IOrderedDictionary.Insert(int index, object key, object value)
	{
		Insert(index, (TKey)key, (TValue)value);
	}

	void IOrderedDictionary.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	object IOrderedDictionary.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (TValue)value;
		}
	}

	#endregion

	#region IDictionary

	void IDictionary.Add(object key, object value)
	{
		Add((TKey)key, (TValue)value);
	}

	void IDictionary.Clear()
	{
		Clear();
	}

	bool IDictionary.Contains(object key)
	{
		return _keyedCollection.Contains((TKey)key);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new DictionaryEnumerator<TKey, TValue>(this);
	}

	bool IDictionary.IsFixedSize
	{
		get { return false; }
	}

	bool IDictionary.IsReadOnly
	{
		get { return false; }
	}

	ICollection IDictionary.Keys
	{
		get { return (ICollection)this.Keys; }
	}

	void IDictionary.Remove(object key)
	{
		Remove((TKey)key);
	}

	ICollection IDictionary.Values
	{
		get { return (ICollection)this.Values; }
	}

	object IDictionary.this[object key]
	{
		get
		{
			return this[(TKey)key];
		}
		set
		{
			this[(TKey)key] = (TValue)value;
		}
	}

	#endregion

	#region ICollection

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_keyedCollection).CopyTo(array, index);
	}

	int ICollection.Count
	{
		get { return ((ICollection)_keyedCollection).Count; }
	}

	bool ICollection.IsSynchronized
	{
		get { return ((ICollection)_keyedCollection).IsSynchronized; }
	}

	object ICollection.SyncRoot
	{
		get { return ((ICollection)_keyedCollection).SyncRoot; }
	}

	#endregion
}

/// <summary>
/// A concrete implementation of the abstract KeyedCollection class using lambdas for the
/// implementation.
/// </summary>
public class KeyedCollection2<TKey, TItem> : KeyedCollection<TKey, TItem>
{
	private const string DelegateNullExceptionMessage = "Delegate passed cannot be null";
	private Func<TItem, TKey> _getKeyForItemFunction;

	public KeyedCollection2(Func<TItem, TKey> getKeyForItemFunction) : base()
	{
		if (getKeyForItemFunction == null) throw new ArgumentNullException(DelegateNullExceptionMessage);
		_getKeyForItemFunction = getKeyForItemFunction;
	}

	public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate, IEqualityComparer<TKey> comparer) : base(comparer)
	{
		if (getKeyForItemDelegate == null) throw new ArgumentNullException(DelegateNullExceptionMessage);
		_getKeyForItemFunction = getKeyForItemDelegate;
	}

	protected override TKey GetKeyForItem(TItem item)
	{
		return _getKeyForItemFunction(item);
	}

	public void SortByKeys()
	{
		var comparer = Comparer<TKey>.Default;
		SortByKeys(comparer);
	}

	public void SortByKeys(IComparer<TKey> keyComparer)
	{
		var comparer = new Comparer2<TItem>((x, y) => keyComparer.Compare(GetKeyForItem(x), GetKeyForItem(y)));
		Sort(comparer);
	}

	public void SortByKeys(Comparison<TKey> keyComparison)
	{
		var comparer = new Comparer2<TItem>((x, y) => keyComparison(GetKeyForItem(x), GetKeyForItem(y)));
		Sort(comparer);
	}

	public void Sort()
	{
		var comparer = Comparer<TItem>.Default;
		Sort(comparer);
	}

	public void Sort(Comparison<TItem> comparison)
	{
		var newComparer = new Comparer2<TItem>((x, y) => comparison(x, y));
		Sort(newComparer);
	}

	public void Sort(IComparer<TItem> comparer)
	{
		List<TItem> list = base.Items as List<TItem>;
		if (list != null)
		{
			list.Sort(comparer);
		}
	}
}

public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IOrderedDictionary
{
	new TValue this[int index] { get; set; }
	new TValue this[TKey key] { get; set; }
	new int Count { get; }
	new ICollection<TKey> Keys { get; }
	new ICollection<TValue> Values { get; }
	new void Add(TKey key, TValue value);
	new void Clear();
	void Insert(int index, TKey key, TValue value);
	int IndexOf(TKey key);
	bool ContainsValue(TValue value);
	bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer);
	new bool ContainsKey(TKey key);
	new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();
	new bool Remove(TKey key);
	new void RemoveAt(int index);
	new bool TryGetValue(TKey key, out TValue value);
	TValue GetValue(TKey key);
	void SetValue(TKey key, TValue value);
	KeyValuePair<TKey, TValue> GetItem(int index);
	void SetItem(int index, TValue value);
}

[DebuggerDisplay("{Value}", Name = "[{Index}]: {Key}")]
internal class IndexedKeyValuePairs
{
	public IDictionary Dictionary { get; private set; }
	public int Index { get; private set; }
	public object Key { get; private set; }
	public object Value { get; private set; }

	public IndexedKeyValuePairs(IDictionary dictionary, int index, object key, object value)
	{
		Index = index;
		Value = value;
		Key = key;
		Dictionary = dictionary;
	}
}

internal class OrderedDictionaryDebugView
{

	private IOrderedDictionary _dict;
	public OrderedDictionaryDebugView(IOrderedDictionary dict)
	{
		_dict = dict;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public IndexedKeyValuePairs[] IndexedKeyValuePairs
	{
		get
		{
			IndexedKeyValuePairs[] nkeys = new IndexedKeyValuePairs[_dict.Count];

			int i = 0;
			foreach (object key in _dict.Keys)
			{
				nkeys[i] = new IndexedKeyValuePairs(_dict, i, key, _dict[key]);
				i += 1;
			}
			return nkeys;
		}
	}
}

public class Comparer2<T> : Comparer<T>
{
	private readonly Comparison<T> _compareFunction;

	public Comparer2(Comparison<T> comparison)
	{
		if (comparison == null) throw new ArgumentNullException("comparison");
		_compareFunction = comparison;
	}

	public override int Compare(T arg1, T arg2)
	{
		return _compareFunction(arg1, arg2);
	}
}

/// <summary>
/// Provides functionality for creation of an EqualityComparer with lambda
/// Equals and GetHashCode functions.
/// </summary>
public class EqualityComparer2<T> : EqualityComparer<T>
{
	private readonly Func<T, T, bool> _equalsFunction;
	private readonly Func<T, int> _hashFunction;

	#region Constructors

	public EqualityComparer2(Func<T, T, bool> equalsFunction)
		: this(equalsFunction, o => 0)
	{
	}

	public EqualityComparer2(Func<T, T, bool> equalsFunction, Func<T, int> hashFunction)
	{
		if (equalsFunction == null) throw new ArgumentNullException("equalsFunction");
		if (hashFunction == null) throw new ArgumentNullException("hashFunction");
		_equalsFunction = equalsFunction;
		_hashFunction = hashFunction;
	}

	#endregion

	public override bool Equals(T x, T y)
	{
		return _equalsFunction(x, y);
	}

	public override int GetHashCode(T obj)
	{
		return _hashFunction(obj);
	}

}

public class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IDisposable
{
	readonly IEnumerator<KeyValuePair<TKey, TValue>> _impl;
	public void Dispose() { _impl.Dispose(); }
	public DictionaryEnumerator(IDictionary<TKey, TValue> value)
	{
		this._impl = value.GetEnumerator();
	}
	public void Reset() { _impl.Reset(); }
	public bool MoveNext() { return _impl.MoveNext(); }
	public DictionaryEntry Entry
	{
		get
		{
			var pair = _impl.Current;
			return new DictionaryEntry(pair.Key, pair.Value);
		}
	}
	public object Key { get { return _impl.Current.Key; } }
	public object Value { get { return _impl.Current.Value; } }
	public object Current { get { return Entry; } }
}

// CREDIT: MIT, .NET Foundation
// MODIFIED: for performance

[Serializable]
[DebuggerDisplay("Count = {Count}")]
public abstract class KeyedCollection<TKey, TItem> : Collection<TItem> where TKey : notnull
{
	private const int DefaultThreshold = 0;

	private readonly IEqualityComparer<TKey> comparer; // Do not rename (binary serialization)
	private Dictionary<TKey, TItem>? dict; // Do not rename (binary serialization)
	private int keyCount; // Do not rename (binary serialization)
	private readonly int threshold; // Do not rename (binary serialization)

	protected KeyedCollection() : this(null, DefaultThreshold)
	{
	}

	protected KeyedCollection(IEqualityComparer<TKey>? comparer) : this(comparer, DefaultThreshold)
	{
	}

	protected KeyedCollection(IEqualityComparer<TKey>? comparer, int dictionaryCreationThreshold)
		: base(new List<TItem>())
	{
		if (dictionaryCreationThreshold < -1)
		{
			throw new ArgumentOutOfRangeException(nameof(dictionaryCreationThreshold));
		}

		this.comparer = comparer ?? EqualityComparer<TKey>.Default;
		threshold = dictionaryCreationThreshold == -1 ? int.MaxValue : dictionaryCreationThreshold;
	}

	public new List<TItem> Items
	{
		get
		{
			Debug.Assert(base.Items is List<TItem>);
			return (List<TItem>)base.Items;
		}
	}

	public IEqualityComparer<TKey> Comparer => comparer;

	public TItem this[TKey key]
	{
		get
		{
			TItem item;
			if (TryGetValue(key, out item!))
			{
				return item;
			}

			throw new KeyNotFoundException(key.ToString());
		}
	}

	public bool Contains(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (dict != null)
		{
			return dict.ContainsKey(key);
		}

		foreach (TItem item in Items)
		{
			if (comparer.Equals(GetKeyForItem(item), key))
			{
				return true;
			}
		}

		return false;
	}

	public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TItem item)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (dict != null)
		{
			return dict.TryGetValue(key, out item!);
		}

		foreach (TItem itemInItems in Items)
		{
			TKey keyInItems = GetKeyForItem(itemInItems);
			if (keyInItems != null && comparer.Equals(key, keyInItems))
			{
				item = itemInItems;
				return true;
			}
		}

		item = default;
		return false;
	}

	private bool ContainsItem(TItem item)
	{
		TKey key;
		if ((dict == null) || ((key = GetKeyForItem(item)) == null))
		{
			return Items.Contains(item);
		}

		TItem itemInDict;
		if (dict.TryGetValue(key, out itemInDict!))
		{
			return EqualityComparer<TItem>.Default.Equals(itemInDict, item);
		}

		return false;
	}

	public bool Remove(TKey key)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (dict != null)
		{
			TItem item;
			return dict.TryGetValue(key, out item!) && Remove(item);
		}

		for (int i = 0; i < Items.Count; i++)
		{
			if (comparer.Equals(GetKeyForItem(Items[i]), key))
			{
				RemoveItem(i);
				return true;
			}
		}

		return false;
	}

	protected IDictionary<TKey, TItem>? Dictionary => dict;

	protected void ChangeItemKey(TItem item, TKey newKey)
	{
		if (!ContainsItem(item))
		{
			throw new ArgumentException(nameof(item));
		}

		TKey oldKey = GetKeyForItem(item);
		if (!comparer.Equals(oldKey, newKey))
		{
			if (newKey != null)
			{
				AddKey(newKey, item);
			}
			if (oldKey != null)
			{
				RemoveKey(oldKey);
			}
		}
	}

	protected override void ClearItems()
	{
		base.ClearItems();
		dict?.Clear();
		keyCount = 0;
	}

	protected abstract TKey GetKeyForItem(TItem item);

	protected override void InsertItem(int index, TItem item)
	{
		TKey key = GetKeyForItem(item);
		if (key != null)
		{
			AddKey(key, item);
		}

		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		TKey key = GetKeyForItem(Items[index]);
		if (key != null)
		{
			RemoveKey(key);
		}

		base.RemoveItem(index);
	}

	protected override void SetItem(int index, TItem item)
	{
		TKey newKey = GetKeyForItem(item);
		TKey oldKey = GetKeyForItem(Items[index]);

		if (comparer.Equals(oldKey, newKey))
		{
			if (newKey != null && dict != null)
			{
				dict[newKey] = item;
			}
		}
		else
		{
			if (newKey != null)
			{
				AddKey(newKey, item);
			}

			if (oldKey != null)
			{
				RemoveKey(oldKey);
			}
		}

		base.SetItem(index, item);
	}

	private void AddKey(TKey key, TItem item)
	{
		if (dict != null)
		{
			dict.Add(key, item);
		}
		else if (keyCount == threshold)
		{
			CreateDictionary();
			dict!.Add(key, item);
		}
		else
		{
			if (Contains(key))
			{
				throw new ArgumentException(nameof(key));
			}

			keyCount++;
		}
	}

	private void CreateDictionary()
	{
		dict = new Dictionary<TKey, TItem>(comparer);
		foreach (TItem item in Items)
		{
			TKey key = GetKeyForItem(item);
			if (key != null)
			{
				dict.Add(key, item);
			}
		}
	}

	private void RemoveKey(TKey key)
	{
		Debug.Assert(key != null, "key shouldn't be null!");
		if (dict != null)
		{
			dict.Remove(key);
		}
		else
		{
			keyCount--;
		}
	}
}
