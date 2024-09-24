using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Toolbox.ComponentModel
{
    /// <summary>
    /// A <see cref="BindingList{T}"/> that support sorting and filtering
    /// </summary>
    public class SortedBindingList<T> : IList<T>, IReadOnlyList<T>, IList,
        IBindingList, IRaiseItemChangedEvents, ICancelAddNew where T: new()
    {
        /// <summary>
        /// Creates a new instance of <see cref="SortedBindingList{T}"/>.
        /// </summary>
        public SortedBindingList()
        {
            Comparer = new ItemComparer<T>(Items);
        }

        private List<T> Items { get; } = new List<T>();
        private List<int> Indices { get; } = new List<int>();
        private ItemComparer<T> Comparer { get; }
        
        private const bool _isReadOnly = false;

        #region IList
        object IList.this[int index]
        {
            get => GetItem(index);
            set
            {
                if (value is T tValue)
                {
                    SetItem(index, tValue);
                    return;
                }

                throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
            }
        }
        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => _isReadOnly;

        int IList.Add(object value)
        {
            if (value is T tValue)
            {
                Add(tValue);
                return IndexOf(tValue);
            }

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        public void Clear()
        {
            foreach (var item in Items)
            {
                if (item is INotifyPropertyChanged notifyItem)
                    notifyItem.PropertyChanged -= ItemPropertyChanged;
            }
            Items.Clear();
            Indices.Clear();
            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, 0));
        }
        #endregion
        #region IList<T>
        /// <summary>
        /// Gets or sets an element at an index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The element at the index.</returns>
        /// <exception cref="InvalidOperationException">When the list is sorted (<see cref="IsSorted"/>) setting an element is not possible, since
        /// it would break the ordering.</exception>
        public T this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }
        #endregion
        #region IReadOnlyList<T>
        T IReadOnlyList<T>.this[int index] => Items[Indices[index]];
        #endregion
        #region IRaiseItemChangedEvents
        bool IRaiseItemChangedEvents.RaisesItemChangedEvents => true;
        #endregion
        #region ICollection<T>
        /// <summary>
        /// Adds an element to the list.
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>If the list is sorted (<see cref="IsSorted"/>) then it is inserted at the correct position.</remarks>
        public void Add(T item)
        {
            AddCore(item);
        }

        bool ICollection<T>.IsReadOnly => _isReadOnly;

        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        public int Count => Items.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => Items;
        #endregion
        #region IBindingList
        bool IBindingList.AllowEdit => true;

        bool IBindingList.AllowNew => true;

        bool IBindingList.AllowRemove => true;

        /// <summary>
        /// Indicates if the list is sorted.
        /// </summary>
        /// <see cref="SortDirection"/>
        /// <see cref="SortProperty"/>
        public bool IsSorted => Comparer.IsSorted;

        /// <summary>
        /// Direction of sorting.
        /// </summary>
        /// <remarks>Only applies it <see cref="IsSorted"/> is <c>true</c>.</remarks>
        /// <see cref="SortProperty"/>
        public ListSortDirection SortDirection => Comparer.SortDirection;

        /// <summary>
        /// Property by with the elements are sorted.
        /// </summary>
        /// <remarks>If this property is <c>null</c> then the <see cref="ToString()"/> method is used for sorting.</remarks>
        /// <see cref="IsSorted"/>
        /// <see cref="SortDirection"/>
        public PropertyDescriptor SortProperty => Comparer.SortProperty;

        bool IBindingList.SupportsChangeNotification => true;

        bool IBindingList.SupportsSearching => true;

        bool IBindingList.SupportsSorting => true;

        private int PendingAdd { get; set; }
        private void CommitPendingItem()
        {
            if (PendingAdd >= 0)
                PendingAdd = -1;
        }

        /// <summary>
        /// Creates a new item, which can me commited (<see cref=""/>
        /// </summary>
        /// <returns></returns>
        public T AddNew()
        {
            var item = new T();
            AddCore(item);
            PendingAdd = IndexOf(item);
            return item;
        }

        object IBindingList.AddNew()
        {
            return AddNew();
        }

        /// <summary>
        /// Apply sorting to the list.
        /// </summary>
        /// <param name="property">The property used for sorting. If this property is <c>null</c> then the <see cref="ToString()"/> method is used for sorting.</param>
        /// <param name="direction">The direction of the sort</param>
        /// <see cref="IsSorted"/>
        /// <see cref="SortDirection"/>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            Comparer.SortDirection = direction;
            Comparer.IsSorted = true;
            Comparer.SortProperty = property;

            CreateIndices();
        }

        /// <summary>
        /// Apply sorting to the list.
        /// </summary>
        /// <param name="propertyName">The property used for sorting. If this property is <c>null</c> then the <see cref="ToString()"/> method is used for sorting.</param>
        /// <param name="direction">The direction of the sort</param>
        /// <see cref="IsSorted"/>
        /// <see cref="SortDirection"/>
        /// <exception cref="ArgumentException">When the type of elements does not have a property named <paramref name="propertyName"/></exception>
        public void ApplySort(string propertyName, ListSortDirection direction)
        {
            var property = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false) 
                ?? throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

			ApplySort(property, direction);
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {            
        }

        /// <summary>
        /// Removes the sorting
        /// </summary>
        public void RemoveSort()
        {
            Comparer.IsSorted = false;
            Comparer.SortDirection = ListSortDirection.Ascending;
            Comparer.SortProperty = null;

            CreateIndices();
        }

        /// <summary>
        /// Gets fired, when the list changes.
        /// </summary>
        /// <see cref="IBindingList.ListChanged"/>
        public event ListChangedEventHandler ListChanged;
        #endregion

        /// <summary>
        /// Cancel an item that was created wird <see cref="AddNew"/>.
        /// </summary>
        /// <param name="itemIndex"></param>
        public void CancelNew(int itemIndex)
        {
            if (itemIndex != PendingAdd) return;
            RemoveAt(itemIndex);
            PendingAdd = -1;
        }

        bool IList.Contains(object value)
        {
            if (value is T tValue)
                return Contains(tValue);

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        /// <summary>
        /// Checks it an element is part of the list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns><c>true</c> if it is, else <c>false</c></returns>
        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copy the elements into an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex">Starting index in <paramref name="array"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="arrayIndex"/> is less than <c>0</c></exception>
        /// <exception cref="ArgumentException">if <paramref name="array"/> is to small to take all elements starting at <paramref name="arrayIndex"/></exception>
        public void CopyTo(T[] array, int arrayIndex = 0)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex > Count) throw new ArgumentException("Target array to small.");

            for (var i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = Items[Indices[i]];
            }
        }

        /// <summary>
        /// Commits an item that was created wird <see cref="AddNew"/>.
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <remarks>Items are automatically commited, when needed for other operations.</remarks>
        public void EndNew(int itemIndex)
        {
            CommitPendingItem();
        }

        /// <summary>
        /// Find an element having a certain property value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        /// <returns>Index of the element or <c>-1</c> if no element was found.</returns>
        public int Find(PropertyDescriptor property, object key)
        {
            return Indices.Find(i => property.GetValue(Items[Indices[i]]).Equals(key));
        }

        /// <summary>
        /// Find an element having a certain property value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        /// <returns>Index of the element or <c>-1</c> if no element was found.</returns>
        /// <exception cref="ArgumentException">if <paramref name="propertyName"/> is not a property of the element type.</exception>
        public int Find(string propertyName, object key)
        {
            var property = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false) 
                ?? throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");
			return Find(property, key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator for the list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This uses sorting if it is enabled.</remarks>
        /// <see cref="IsSorted"/>
        public IEnumerator<T> GetEnumerator()
        {
            return new ItemEnumerator<T>(Indices, Items);
        }

        int IList.IndexOf(object value)
        {
            if (value is T tValue)
                return IndexOf(tValue);

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        /// <summary>
        /// Returns the index of an element
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Index of the elment or <c>-1</c> it is not in the list.</returns>
        public int IndexOf(T item)
        {
            var dataIndex = Items.IndexOf(item);
            return dataIndex >= 0 ? Indices.IndexOf(dataIndex) : -1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is T tValue)
            {
                InsertCore(index, tValue);
                return;
            }

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        /// <summary>
        /// Inserts an element into the list
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <exception cref="InvalidOperationException">When the list is sorted (<see cref="IsSorted"/>) inserting an element is not possible, since
        /// it would break the ordering.</exception>
        public void Insert(int index, T item)
        {
            InsertCore(index, item);
        }

        void IList.Remove(object value)
        {
            if (value is T tValue)
            {
                RemoveCore(tValue);
                return;
            }

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        /// <summary>
        /// Remove an element from the list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            return RemoveCore(item);
        }

        /// <summary>
        /// Remove an element at a postion.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            RemoveAtCore(index);
        }

        #region Core

        private T GetItem(int index)
        {
            return Items[Indices[index]];
        }

        private void SetItem(int index, T value)
        {
            if (Comparer.IsSorted)
                throw new InvalidOperationException("No set operation when list is sorted.");

            if (Items[Indices[index]] is INotifyPropertyChanged notifyItem)
                notifyItem.PropertyChanged -= ItemPropertyChanged;

            Items[Indices[index]] = value;
            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index));

            if (Items[Indices[index]] is INotifyPropertyChanged newNotifyItem)
                newNotifyItem.PropertyChanged += ItemPropertyChanged;
        }

        private void CreateIndices()
        {
            Indices.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                Indices.Add(i);
            }

            if (IsSorted)
            {
                Indices.Sort(Comparer);
            }
            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, 0));
        }

        private void AddCore(T item)
        {
            CommitPendingItem();
            if (IsSorted)
            {
                var dataIndex = Items.Count;
                Items.Add(item);

                var index = Indices.BinarySearch(dataIndex, Comparer);
                if (index >= 0)
                {
                    Indices.Insert(index, dataIndex);
                }
                else
                {
                    index = ~index;
                    Indices.Insert(index, dataIndex);
                }
                ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemAdded, index));

            }
            else
            {
                Indices.Add(Items.Count);
                Items.Add(item);
                ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemAdded, Items.Count - 1));
            }
            if (item is INotifyPropertyChanged notifyItem)
                notifyItem.PropertyChanged += ItemPropertyChanged;                             
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;
            var index = IndexOf(item);
            var property = TypeDescriptor.GetProperties(item).Find(e.PropertyName, false);

            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index, property));
        }

        private void RemoveAtCore(int index)
        {
            var dataIndex = Indices[index];
            var item = Items[dataIndex];
            if (item is INotifyPropertyChanged notifyItem)
                notifyItem.PropertyChanged -= ItemPropertyChanged;

            Items.RemoveAt(dataIndex);
            Indices.RemoveAt(index);
            for (var i = 0; i < Indices.Count; i++)
            {
                if (Indices[i] > dataIndex) Indices[i]--;
            }
            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        private bool RemoveCore(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
                RemoveAt(index);

            return index >= 0;
        }

        public void InsertCore(int index, T item)
        {
            if (Comparer.IsSorted)
                throw new InvalidOperationException("No insert operation when list is sorted.");

            Items.Insert(index, item);
            Indices.Add(Indices.Count);

            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemAdded, index));

            if (item is INotifyPropertyChanged notifyItem)
                notifyItem.PropertyChanged -= ItemPropertyChanged;
        }

        #endregion

        #region IComparer<T>
        private class ItemComparer<TI> : IComparer<int>
        {
            public ItemComparer(List<TI> items)
            {
                Items = items;
            }

            public List<TI> Items { get; }

            public bool IsSorted { get; set; }

            public ListSortDirection SortDirection { get; set; }

            public PropertyDescriptor SortProperty { get; set; }

            public int Compare(int x, int y)
            {
                var xItem = (object)Items[x];
                var yItem = (object)Items[y];

                if (SortProperty != null)
                {
                    xItem = SortProperty.GetValue(xItem);
                    yItem = SortProperty.GetValue(yItem);
                }

				int rc;
                if (xItem is IComparable xCompare)
                {
                    rc = xCompare.CompareTo(yItem);
                }
                else
                {
                    rc = xItem.ToString().CompareTo(yItem.ToString());
                }
                return SortDirection == ListSortDirection.Ascending ? rc : -rc;

            }
        }
        #endregion

        #region Enumerator<T>
        private class ItemEnumerator<TI> : IEnumerator<TI>
        {
            public ItemEnumerator(List<int> indices, List<TI> items)
            {
                IndicesEnumerator = indices.GetEnumerator();
                Items = items;
            }

            private IEnumerator<int> IndicesEnumerator { get; }
            private List<TI> Items { get; }

            public TI Current => Items[IndicesEnumerator.Current];

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                return IndicesEnumerator.MoveNext();
            }

            public void Reset()
            {
                IndicesEnumerator.Reset();
            }

            private bool _disposed;

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        IndicesEnumerator.Dispose();
                    }

                    _disposed = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
