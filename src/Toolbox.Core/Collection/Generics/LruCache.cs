using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Toolbox.Collection.Generics
{
	/// <summary>
	/// Represents the method that handles the event raised when an item is evicted from a collection.
	/// </summary>
	/// <param name="key">The key of the item that was evicted.</param>
	/// <param name="value">The value of the item that was evicted.</param>
	public delegate void ItemEvictedEventHandler<TKey, TValue>(TKey key, TValue value);

	/// <summary>
	/// Represents a thread-safe, fixed-capacity cache that stores key-value pairs and evicts the least recently used items
	/// when the capacity is exceeded.
	/// </summary>
	/// <remarks>This cache is designed for concurrent access and automatically removes the least recently used
	/// entry when adding a new item causes the cache to exceed its specified capacity. Accessing or updating an item marks
	/// it as most recently used. All public members are thread-safe.</remarks>
	/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
	/// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
	public class LruCache<TKey, TValue>(int capacity) where TKey : notnull where TValue : notnull
	{
		private readonly int _capacity = capacity;
		private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache = [];

		private readonly LinkedList<CacheItem> _usageList = [];
		private readonly object _lock = new();
		
		/// <summary>
		/// Occurs when an item is evicted from the cache.
		/// </summary>
		/// <remarks>Subscribe to this event to be notified when an item is removed from the cache.</remarks>
		public event ItemEvictedEventHandler<TKey, TValue> ItemEvicted;

		private class CacheItem
		{
			public TKey Key { get; }
			public TValue Value { get; set; }

			public CacheItem(TKey key, TValue value)
			{
				Key = key;
				Value = value;
			}
		}

		/// <summary>
		/// Attempts to retrieve the value associated with the specified key from the cache.
		/// </summary>
		/// <remarks>If the key is found, the associated entry is marked as most recently used. This method is
		/// thread-safe.</remarks>
		/// <param name="key">The key whose associated value is to be retrieved.</param>
		/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise,
		/// the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the cache contains an element with the specified key; otherwise, false.</returns>
		public bool TryGet(TKey key, [NotNullWhen(true), MaybeNullWhen(false)] out TValue? value)
		{
			lock (_lock)
			{
				if (!_cache.TryGetValue(key, out var node))
				{
					value = default;
					return false;
				}

				// Move to front (most recently used)
				_usageList.Remove(node);
				_usageList.AddFirst(node);

				value = node.Value.Value;
				return true;
			}
		}

		/// <summary>
		/// Adds a key and value to the cache, or updates the value if the key already exists. Moves the key to the most
		/// recently used position.
		/// </summary>
		/// <remarks>If the cache exceeds its capacity, the least recently used item is removed. This method is
		/// thread-safe.</remarks>
		/// <param name="key">The key to add or update in the cache.</param>
		/// <param name="value">The value to associate with the specified key.</param>
		public void Put(TKey key, TValue value)
		{
			LinkedListNode<CacheItem>? evicted = null;

			lock (_lock)
			{
				if (_cache.TryGetValue(key, out var existingNode))
				{
					// Update value and move to front
					existingNode.Value.Value = value;
					_usageList.Remove(existingNode);
					_usageList.AddFirst(existingNode);
					return;
				}

				var newItem = new CacheItem(key, value);
				var newNode = new LinkedListNode<CacheItem>(newItem);

				_usageList.AddFirst(newNode);
				_cache[key] = newNode;

				// Evict least recently used
				if (_cache.Count > _capacity)
				{
					evicted = _usageList.Last;
					if (evicted != null)
					{
						_usageList.RemoveLast();
						_cache.Remove(evicted.Value.Key);
					}
				}
			}
			if (evicted != null)
			{
				ItemEvicted?.Invoke(evicted.Value.Key, evicted.Value.Value);
			}	
		}

		/// <summary>
		/// Removes the value with the specified key from the cache.
		/// </summary>
		/// <remarks>This method is thread-safe. If the specified key does not exist in the cache, the method returns
		/// false and no action is taken.</remarks>
		/// <param name="key">The key of the element to remove from the cache.</param>
		/// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
		public bool Remove(TKey key)
		{
			LinkedListNode<CacheItem>? evicted;

			lock (_lock)
			{
				if (!_cache.TryGetValue(key, out evicted))
					return false;

				_usageList.Remove(evicted);
				_cache.Remove(key);
			}
			
			ItemEvicted?.Invoke(evicted.Value.Key, evicted.Value.Value);

			return true;
		}

		/// <summary>
		/// Gets the number of items currently stored in the cache.
		/// </summary>
		public int Count
		{
			get
			{
				lock (_lock)
				{
					return _cache.Count;
				}
			}
		}
	}
}
