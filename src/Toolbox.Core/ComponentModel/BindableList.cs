using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Toolbox.Collection.Generics;

namespace Toolbox.ComponentModel
{
    /// <summary>
    /// A replacement for <see cref="BindingList{T}"/> with better events.
    /// </summary>
    /// <remarks>
    /// The events autoamtically use the <see cref="ISynchronizeInvoke.Invoke(Delegate, object[])"/>
    /// if the target of the event handler supports the interfae.
    /// </remarks>
    public class BindableList<T> : IList<T>, IReadOnlyList<T>, IList,
        IBindingList, IRaiseItemChangedEvents, ICancelAddNew
    {
        /// <summary>
        /// Creates a new instance of <see cref="SortedBindingList{T}"/>.
        /// </summary>
        public BindableList()
        {
            RaisesItemChangedEvents = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
            DataProperties = TypeDescriptor.GetProperties(typeof(T))
                                .Cast<PropertyDescriptor>()
                                .ToDictionary(p => p.Name);

            Comparer = new ItemComparer<T>(Items);
        }

        /// <summary>
        /// Create a new instance of <see cref="BindableList{T}"/>.
        /// </summary>
        public BindableList(IEnumerable<T> other) : this()
        {
            other.ForEach(Add);
        }

        #region Internal Properties
        private Dictionary<string, PropertyDescriptor> DataProperties { get; }
        private List<T> Items { get; } = new List<T>();
        private List<int> Indices { get; } = new List<int>();
        private ItemComparer<T> Comparer { get; }

        private const bool IsReadOnly = false;
        #endregion

        #region T.INotifyPropertyChanged
        private void Attach(T item)
        {
            if (!RaisesItemChangedEvents || item == null) return;

            var changingItem = (INotifyPropertyChanged)item;
            changingItem.PropertyChanged += ItemPropertyChanged;
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;
            var index = IndexOf(item);

            OnItemChanged(index, item, e.PropertyName);
            if (DataProperties.TryGetValue(e.PropertyName, out var descriptor))
            {
                OnListChanged(ListChangedType.ItemChanged, index, 0, descriptor);
            }
            else
            {
                OnListChanged(ListChangedType.ItemChanged, index);
            }
        }

        private void Detach(T item)
        {
            if (!RaisesItemChangedEvents || item == null) return;

            var changingItem = (INotifyPropertyChanged)item;
            changingItem.PropertyChanged -= ItemPropertyChanged;
        }
        #endregion


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

        bool IList.IsReadOnly => IsReadOnly;

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
            OnResetting();

            Items.ForEach(i => Detach(i));
            Items.Clear();
            Indices.Clear();

            OnResetted();
            OnListChanged(ListChangedType.Reset);
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
        public bool RaisesItemChangedEvents { get; }
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

        bool ICollection<T>.IsReadOnly => IsReadOnly;

        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        public int Count => Items.Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => Items;
        #endregion
        #region IBindingList
        public bool AllowEdit { get; set; } = true;

        public bool AllowNew { get; set; } = true;

        public bool AllowRemove { get; set; } = true;

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

        private int PendingAdd { get; set; } = -1;
        private void CommitPendingItem()
        {
            if (PendingAdd >= 0)
            {
                OnItemAdded(PendingAdd, GetItem(PendingAdd));
                OnListChanged(ListChangedType.ItemAdded, PendingAdd);
                PendingAdd = -1;
            }
        }

        /// <summary>
        /// Creates a new item, which can me commited (<see cref=""/>
        /// </summary>
        /// <returns></returns>
        public T AddNew()
        {
            var args = new AddingNewEventArgs();
            OnAddingNew(args);

            if (!(args.NewObject is T item))
            {
                item = Activator.CreateInstance<T>();
            }
            
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
            OnResetting();
            Comparer.SortDirection = direction;
            Comparer.IsSorted = true;
            Comparer.SortProperty = property;

            CreateIndices();

            OnResetted();
            OnListChanged(ListChangedType.Reset);
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
            var property = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false);
            if (property == null)
                throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

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
        #endregion

        #region Events

        #region ItemAdded

        private event EventHandler<ItemEventArgs<T>> ItemAddedHandler;
        /// <summary>
        /// Raised after an item was added.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemAdded
        {
            add => AddHandler(ref ItemAddedHandler, value);
            remove => RemoveHandler(ref ItemAddedHandler, value);
        }

        private void OnItemAdded(int index, T item)
        {
            ItemAddedHandler?.Invoke(this, new ItemEventArgs<T>(index, item));
        }

        #endregion
        #region AddingItem

        private event EventHandler<ItemEventArgs<T>> AddingItemHandler;
        /// <summary>
        /// Raised before an item is added.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> AddingItem
        {
            add => AddHandler(ref AddingItemHandler, value);
            remove => RemoveHandler(ref AddingItemHandler, value);
        }

        private void OnAddingItem(int index, T item)
        {
            AddingItemHandler?.Invoke(this, new ItemEventArgs<T>(index, item));
        }

        #endregion
        #region ItemRemoved
        private event EventHandler<ItemEventArgs<T>> ItemRemovedHandler;
        /// <summary>
        /// Raised after an item is removed.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemRemoved
        {
            add => AddHandler(ref ItemRemovedHandler, value);
            remove => RemoveHandler(ref ItemRemovedHandler, value);
        }

        private void OnItemRemoved(int index, T item)
        {
            ItemRemovedHandler?.Invoke(this, new ItemEventArgs<T>(index, item));
        }
        #endregion
        #region RemovingItem
        private event EventHandler<ItemEventArgs<T>> RemovingItemHandler;
        /// <summary>
        /// Raised before an item is removed.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> RemovingItem
        {
            add => AddHandler(ref RemovingItemHandler, value);
            remove => RemoveHandler(ref RemovingItemHandler, value);
        }

        private void OnRemovingItem(int index, T item)
        {
            RemovingItemHandler?.Invoke(this, new ItemEventArgs<T>(index, item));
        }
        #endregion
        #region ItemChanged
        private event EventHandler<ItemChangedEventArgs<T>> ItemChangedHandler;
        /// <summary>
        /// Raised after an item was changed.
        /// </summary>
        /// <see cref="INotifyPropertyChanged"/>
        public event EventHandler<ItemChangedEventArgs<T>> ItemChanged
        {
            add => AddHandler(ref ItemChangedHandler, value);
            remove => RemoveHandler(ref ItemChangedHandler, value);
        }

        private void OnItemChanged(int index, T item, string propertyName)
        {
            ItemChangedHandler?.Invoke(this, new ItemChangedEventArgs<T>(index, item, propertyName));
        }
        #endregion
        #region Resetted
        private event EventHandler<ListResetEventArgs> ResettedHandler;
        /// <summary>
        /// Raised after the collection is reset.
        /// </summary>
        public event EventHandler<ListResetEventArgs> Resetted
        {
            add => AddHandler(ref ResettedHandler, value);
            remove => RemoveHandler(ref ResettedHandler, value);
        }
        private void OnResetted()
        {
            ResettedHandler?.Invoke(this, new ListResetEventArgs());
        }
        #endregion
        #region Resetting
        private event EventHandler<ListResetEventArgs> ResettingHandler;
        /// <summary>
        /// Raised before the collection is resetting.
        /// </summary>
        public event EventHandler<ListResetEventArgs> Resetting
        {
            add => AddHandler(ref ResettingHandler, value);
            remove => RemoveHandler(ref ResettingHandler, value);
        }

        private void OnResetting()
        {
            ResettingHandler?.Invoke(this, new ListResetEventArgs());
        }
        #endregion
        #region Setting
        private event EventHandler<ItemSetEventArgs<T>> SettingItemHandler;
        /// <summary>
        /// Raised before an item is set.
        /// </summary>
        public event EventHandler<ItemSetEventArgs<T>> SettingItem
        {
            add => AddHandler(ref SettingItemHandler, value);
            remove => RemoveHandler(ref SettingItemHandler, value);
        }

        private void OnSetting(ItemSetEventArgs<T> args)
        {
            SettingItemHandler?.Invoke(this, args);
        }
        #endregion

        #region Set
        private event EventHandler<ItemSetEventArgs<T>> ItemSetHandler;
        /// <summary>
        /// Raised after an item is set.
        /// </summary>
        public event EventHandler<ItemSetEventArgs<T>> ItemSet
        {
            add => AddHandler(ref ItemSetHandler, value);
            remove => RemoveHandler(ref ItemSetHandler, value);
        }

        private void OnSet(ItemSetEventArgs<T> args)
        {
            ItemSetHandler?.Invoke(this, args);
        }
        #endregion
        #region ListChanged
        private event ListChangedEventHandler ListChangedHandler;
        /// <summary>
        /// Raised if the collection changes
        /// </summary>
        public event ListChangedEventHandler ListChanged
        {
            add => AddHandler(ref ListChangedHandler, value);
            remove => RemoveHandler(ref ListChangedHandler, value);
        }

        private void OnListChanged(ListChangedType type, int newIndex = 0, int oldIndex = 0, PropertyDescriptor descriptor = null)
        {
            var args = descriptor == null
                            ? new ListChangedEventArgs(type, newIndex, oldIndex)
                            : new ListChangedEventArgs(type, newIndex, descriptor);

            ListChangedHandler?.Invoke(this, args);
        }
        #endregion
        #region AddingNew
        private event AddingNewEventHandler AddingNewHandler;
        /// <summary>
        /// Raised when an new item needs to be added.
        /// </summary>
        /// <see cref="AddNew"/>
        public event AddingNewEventHandler AddingNew
        {
            add => AddHandler(ref AddingNewHandler, value);
            remove => RemoveHandler(ref AddingNewHandler, value);
        }

        private void OnAddingNew(AddingNewEventArgs args)
        {
            AddingNewHandler?.Invoke(this, args);
        }
        #endregion

        #endregion
        #region EventManagement
        private Dictionary<string, Dictionary<Delegate, object>> InvokeHandler { get; } = new Dictionary<string, Dictionary<Delegate, object>>
    {
        { nameof(AddingItem), new Dictionary<Delegate, object>() },
        { nameof(ItemAdded), new Dictionary<Delegate, object>() },
        { nameof(RemovingItem), new Dictionary<Delegate, object>() },
        { nameof(ItemRemoved), new Dictionary<Delegate, object>() },
        { nameof(SettingItem), new Dictionary<Delegate, object>() },
        { nameof(ItemSet), new Dictionary<Delegate, object>() },
        { nameof(Resetting), new Dictionary<Delegate, object>() },
        { nameof(Resetted), new Dictionary<Delegate, object>() },
        { nameof(ListChanged), new Dictionary<Delegate, object>() }
    };

        private void AddHandler<TE>(ref EventHandler<TE> handler, EventHandler<TE> value, [CallerMemberName] string methodName = null) where TE : EventArgs
        {
            if (value.Target is ISynchronizeInvoke synchronize)
            {
                var invoker = new EventInvoker<TE>(value);
                InvokeHandler[methodName].Add(value, invoker);
                handler += invoker.RaiseEvent;
            }
            else
            {
                handler += value;
            }
        }

        private void RemoveHandler<TE>(ref EventHandler<TE> handler, EventHandler<TE> value, [CallerMemberName] string methodName = null) where TE : EventArgs
        {
            if (value.Target is ISynchronizeInvoke synchronize)
            {
                if (InvokeHandler[methodName].TryGetValue(value, out var obj))
                {
                    var invoker = (EventInvoker<TE>)obj;
                    InvokeHandler[methodName].Remove(value);
                    handler -= invoker.RaiseEvent;
                }
            }
            else
            {
                handler -= value;
            }
        }

        private void AddHandler(ref ListChangedEventHandler handler, ListChangedEventHandler value, [CallerMemberName] string methodName = null)
        {
            if (value.Target is ISynchronizeInvoke synchronize)
            {
                var invoker = new EventInvokerBase<ListChangedEventHandler, ListChangedEventArgs>(value);
                InvokeHandler[methodName].Add(value, invoker);
                handler += invoker.RaiseEvent;
            }
            else
            {
                handler += value;
            }
        }
        private void RemoveHandler(ref ListChangedEventHandler handler, ListChangedEventHandler value, [CallerMemberName] string methodName = null)
        {
            if (value.Target is ISynchronizeInvoke synchronize)
            {
                if (InvokeHandler[methodName].TryGetValue(value, out var obj))
                {
                    var invoker = (EventInvokerBase<ListChangedEventHandler, ListChangedEventArgs>)obj;
                    InvokeHandler[methodName].Remove(value);
                    handler -= invoker.RaiseEvent;
                }
            }
            else
            {
                handler -= value;
            }
        }
        private void AddHandler(ref AddingNewEventHandler handler, AddingNewEventHandler value, [CallerMemberName] string methodName = null)
        {
            if (value.Target is ISynchronizeInvoke synchronize)
            {
                var invoker = new EventInvokerBase<AddingNewEventHandler, AddingNewEventArgs>(value);
                InvokeHandler[methodName].Add(value, invoker);
                handler += invoker.RaiseEvent;
            }
            else
            {
                handler += value;
            }
        }
        private void RemoveHandler(ref AddingNewEventHandler handler, AddingNewEventHandler value, [CallerMemberName] string methodName = null)
        {
            if (value.Target is ISynchronizeInvoke synchronize)
            {
                if (InvokeHandler[methodName].TryGetValue(value, out var obj))
                {
                    var invoker = (EventInvokerBase<AddingNewEventHandler, AddingNewEventArgs>)obj;
                    InvokeHandler[methodName].Remove(value);
                    handler -= invoker.RaiseEvent;
                }
            }
            else
            {
                handler -= value;
            }
        }
    
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
            var property = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false);
            if (property == null)
                throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

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

            CommitPendingItem();

            var oldItem = Items[index];
            var args = new ItemSetEventArgs<T>(index, oldItem, value);

            OnSetting(args);

            Detach(oldItem);
            Items[index] = value;
            Attach(value);

            OnSet(args);
            OnListChanged(ListChangedType.ItemChanged, index);
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
        }

        private void AddCore(T item)
        {
            if (!AllowEdit) throw new NotSupportedException();

            CommitPendingItem();
            if (IsSorted)
            {
                var dataIndex = Items.Count;

                Items.Add(item);
                var index = Indices.BinarySearch(dataIndex, Comparer);
                if (index < 0) index = ~index;

                OnAddingItem(index, item);

                Attach(item);
                Indices.Insert(index, dataIndex);

                OnItemAdded(index, item);
                OnListChanged(ListChangedType.ItemAdded, index);
            }
            else
            {
                var index = Items.Count;

                OnAddingItem(index, item);
                Indices.Add(Items.Count);
                Items.Add(item);
                Attach(item);
                OnItemAdded(index, item);
                OnListChanged(ListChangedType.ItemAdded, index);
            }
            
        }

        private void RemoveAtCore(int index)
        {
            if (!AllowRemove) throw new NotSupportedException();

            var dataIndex = Indices[index];
            var item = Items[dataIndex];

            OnRemovingItem(index, item);

            Detach(item);
            Items.RemoveAt(dataIndex);
            Indices.RemoveAt(index);
            for (var i = 0; i < Indices.Count; i++)
            {
                if (Indices[i] > dataIndex) Indices[i]--;
            }

            OnItemRemoved(index, item);
            OnListChanged(ListChangedType.ItemDeleted, 0, index);
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

            CommitPendingItem();

            OnAddingItem(index, item);

            Items.Insert(index, item);
            Indices.Add(Indices.Count);
            Attach(item);

            OnItemAdded(index, item);
            OnListChanged(ListChangedType.ItemAdded, index);
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

                int rc = 0;
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

internal class EventInvokerBase<TH, TE> where TH : Delegate where TE : EventArgs
{
    public EventInvokerBase(TH handler)
    {
        Handler = handler;
        Synchronized = (ISynchronizeInvoke)handler.Target;
    }

    public ISynchronizeInvoke Synchronized { get; }
    public TH Handler { get; }

    internal void RaiseEvent(object sender, TE eventArgs)
    {
        if (Synchronized.InvokeRequired)
        {
            Synchronized.Invoke(Handler, new object[] { sender, eventArgs });
        }
        else
        {
            Handler.DynamicInvoke(sender, eventArgs);
        }
    }
}

internal class EventInvoker<T> : EventInvokerBase<EventHandler<T>, T> where T : EventArgs
{
    public EventInvoker(EventHandler<T> handler) : base(handler)
    {
    }
}

#region EventArgs
/// <summary>
/// Event arguments for signaling changes for an item.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ItemEventArgs<T> : EventArgs
{
    /// <summary>
    /// Create a new instance of <see cref="ItemEventArgs{T}"/>.
    /// </summary>
    public ItemEventArgs(int index, T item)
    {
        Index = index;
        Item = item;
    }

    /// <summary>
    /// The item affected.
    /// </summary>
    public T Item { get; }

    /// <summary>
    /// Index of the item.
    /// </summary>
    public int Index { get; }
}

/// <summary>
/// Event arguments for signaling changes on an item.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ItemChangedEventArgs<T> : ItemEventArgs<T>
{
    /// <summary>
    /// Create a new instance of <see cref="ItemChangedEventArgs{T}"/>.
    /// </summary>
    public ItemChangedEventArgs(int index, T item, string propertyName) : base(index, item)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Property that raised the change.
    /// </summary>
    public string PropertyName { get; }
}

/// <summary>
/// Event arguments for signaling changes on an item replacement.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ItemSetEventArgs<T> : EventArgs
{
    /// <summary>
    /// Create a new instance of <see cref="ItemSetEventArgs{T}"/>.
    /// </summary>
    public ItemSetEventArgs(int index, T oldItem, T newItem)
    {
        Index = index;
        OldItem = oldItem;
        NewItem = newItem;
    }
    /// <summary>
    /// Index of the items.
    /// </summary>
    public int Index { get; }
    /// <summary>
    /// The item getting replaced
    /// </summary>
    public T OldItem { get; }
    /// <summary>
    /// The new item
    /// </summary>
    public T NewItem { get; }

}

/// <summary>
/// Event arguments for signaling changes on the collection.
/// </summary>
public class ListResetEventArgs : EventArgs
{
}
#endregion


    /*
    : IList<T>, IReadOnlyList<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
{
    /// <summary>
    /// Create a new instance of <see cref="BindableList{T}"/>.
    /// </summary>
    public BindableList()
    {
        _raisesItemChangedEvents = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
        DataProperties = TypeDescriptor.GetProperties(typeof(T))
                            .Cast<PropertyDescriptor>()
                            .ToDictionary(p => p.Name);

        Comparer = new ItemComparer<T>(Items);
    }

    /// <summary>
    /// Create a new instance of <see cref="BindableList{T}"/>.
    /// </summary>
    public BindableList(IEnumerable<T> other) : this()
    {
        other.ForEach(Add);
    }

    #region Internal Properties
    private Dictionary<string, PropertyDescriptor> DataProperties { get; }
    private List<T> Items { get; } = new List<T>();
    private List<int> Indices { get; } = new List<int>();
    private ItemComparer<T> Comparer { get; }
    #endregion

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

    bool IList.IsReadOnly => IsReadOnly;

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

    bool ICollection<T>.IsReadOnly => IsReadOnly;

    /// <summary>
    /// Returns the number of elements in the list.
    /// </summary>
    public int Count => Items.Count;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => Items;
    #endregion

    #region T.INotifyPropertyChanged
    private void Attach(T item)
    {
        if (!_raisesItemChangedEvents || item==null) return;

        var changingItem = (INotifyPropertyChanged)item;
        changingItem.PropertyChanged += ItemPropertyChanged;
    }

    private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var item = (T)sender;
        var index = IndexOf(item);

        OnItemChanged(index, item, e.PropertyName);
        if (DataProperties.TryGetValue(e.PropertyName, out var descriptor))
        {
            OnListChanged(ListChangedType.ItemChanged, index, 0, descriptor);
        }
        else
        {
            OnListChanged(ListChangedType.ItemChanged, index);
        }
    }

    private void Detach(T item)
    {
        if (!_raisesItemChangedEvents || item==null) return;

        var changingItem = (INotifyPropertyChanged)item;
        changingItem.PropertyChanged -= ItemPropertyChanged;
    }
    #endregion
    protected override void InsertItem(int index, T item)
    {
        if (!AllowEdit) throw new NotSupportedException();

        CommitItem();

        OnAddingItem(index, item);
        base.InsertItem(index, item);
        Attach(item);
        OnItemAdded(index, item);
        OnListChanged(ListChangedType.ItemAdded, index);
    }

    protected override void RemoveItem(int index)
    {
        if (!AllowRemove) throw new NotSupportedException();

        CommitItem();

        var item = this[index];
        OnRemovingItem(index, item);
        Detach(item);
        base.RemoveItem(index);            
        OnItemRemoved(index, item);
        OnListChanged(ListChangedType.ItemDeleted, -1, index);
    }

    protected override void ClearItems()
    {
        if (!AllowRemove) throw new NotSupportedException();

        CommitItem();

        OnResetting();

        Items.ForEach(i => Detach(i));

        base.ClearItems();
        OnResetted();
        OnListChanged(ListChangedType.Reset, 0, 0);
    }


    protected override void SetItem(int index, T item)
    {
        CommitItem();
        var oldItem = this[index];

        var args = new ItemSetEventArgs<T>(index, oldItem, item);
        OnSetting(args);
        Detach(oldItem);
        base.SetItem(index, item);
        Attach(item);
        OnSet(args);
        OnListChanged(ListChangedType.ItemChanged, index);
    }

    #region AllowEdit
    private bool allowEdit = true;
    /// <summary>
    /// Is adding and setting allowed.
    /// </summary>
    public bool AllowEdit
    {
        get => allowEdit;
        set
        {
            if (allowEdit == value) return;
            allowEdit = value;
            OnListChanged(ListChangedType.Reset);
        }
    }
    #endregion
    #region AllowNew
    /// <summary>
    /// Is using <see cref="AddNew"/> allowed.
    /// </summary>
    public bool AllowNew { get; set; } = true;
    #endregion
    #region AllowRemove
    /// <summary>
    /// Can items be removed.
    /// </summary>
    public bool AllowRemove { get; set; } = true;
    #endregion

    #region IBindingList 
    /// <summary>
    /// Is the collection sorted.
    /// </summary>
    public bool IsSorted => false;

    /// <summary>
    /// Direction of sort
    /// </summary>
    public ListSortDirection SortDirection => throw new NotImplementedException();

    /// <summary>
    /// Property used for sorting
    /// </summary>
    public PropertyDescriptor SortProperty => throw new NotImplementedException();

    /// <summary>
    /// Supports the <see cref="INotifyPropertyChanged"/> events.
    /// </summary>
    bool IBindingList.SupportsChangeNotification => true;

    /// <summary>
    /// Support searching of an item.
    /// </summary>
    bool IBindingList.SupportsSearching => true;

    /// <summary>
    /// Support sorting of collection.
    /// </summary>
    bool IBindingList.SupportsSorting => true;

    /// <summary>
    /// Is edition allowed.
    /// </summary>
    /// <see cref="AllowEdit"/>
    bool IBindingList.AllowEdit => AllowEdit;

    /// <summary>
    /// Is add a new item allowed.
    /// </summary>
    /// <see cref="AllowNew"/>
    bool IBindingList.AllowNew => AllowNew;

    /// <summary>
    /// Is removing allowed.
    /// </summary>
    /// <see cref="AllowRemove"/>
    bool IBindingList.AllowRemove => AllowRemove;
    #endregion

    #region IRaiseItemChangedEvents
    private readonly bool _raisesItemChangedEvents;
    /// <summary>
    /// Collection will raise item change events.
    /// </summary>
    bool IRaiseItemChangedEvents.RaisesItemChangedEvents => _raisesItemChangedEvents;
    #endregion
    #region ICancelAddNew
    #region AddNew
    private T PendingItem { get; set; }

    private void CommitItem()
    {
        if (PendingItem == null) return;

        var index = IndexOf(PendingItem);

        OnItemAdded(index, PendingItem);
        OnListChanged(ListChangedType.ItemAdded, index);
        PendingItem = default;
    }

    void IBindingList.AddIndex(PropertyDescriptor property)
    {
        throw new NotImplementedException();
    }

    object IBindingList.AddNew()
    {
        CommitItem();

        var args = new AddingNewEventArgs();

        OnAddingNew(args);

        var pendingItem = (T)(args.NewObject ?? Activator.CreateInstance<T>());

        Add(pendingItem);

        return PendingItem = pendingItem;
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
    /// <param name="propertyName">The property used for sorting.</param>
    /// <param name="direction">The direction of the sort</param>
    /// <see cref="IsSorted"/>
    /// <see cref="SortDirection"/>
    /// <exception cref="ArgumentException">When the type of elements does not have a property named <paramref name="propertyName"/></exception>
    public void ApplySort(string propertyName, ListSortDirection direction)
    {
        if (!DataProperties.TryGetValue(propertyName, out var property))
            throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

        ApplySort(property, direction);
    }

    private void CreateIndices()
    {
        OnResetting();

        Indices.Clear();
        for (var i = 0; i < Items.Count; i++)
        {
            Indices.Add(i);
        }

        if (IsSorted)
        {
            Indices.Sort(Comparer);
        }

        OnResetted();
        OnListChanged(ListChangedType.Reset);
    }


    /// <summary>
    /// Find an element having a certain property value.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="key"></param>
    /// <returns>Index of the element or <c>-1</c> if no element was found.</returns>
    public int Find(PropertyDescriptor property, object key)
    {
        return -1;
        // return Indices.Find(i => property.GetValue(Items[Indices[i]]).Equals(key));
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
        var property = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false);
        if (property == null)
            throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

        return Find(property, key);
    }

    void IBindingList.RemoveIndex(PropertyDescriptor property)
    {
        throw new NotImplementedException();
    }

    void IBindingList.RemoveSort()
    {
        throw new NotImplementedException();
    }
    #endregion

    /// <summary>
    /// Cancels an item created with <see cref="AddNew"/>.
    /// </summary>
    /// <param name="itemIndex"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CancelNew(int itemIndex)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Commits an itme created with <see cref="AddNew"/>.
    /// </summary>
    /// <param name="itemIndex"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <remarks>
    /// Changes to the collection will automatically commit a pending new item.
    /// </remarks>
    public void EndNew(int itemIndex)
    {
        if (IndexOf(PendingItem) != itemIndex)
            throw new ArgumentException("Index does not match pending item.", nameof(itemIndex));

        CommitItem();
    }
    #endregion

    #region EventManagement


    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public bool IsFixedSize => throw new NotImplementedException();

    public bool IsSynchronized => throw new NotImplementedException();

    public object SyncRoot => throw new NotImplementedException();

    object IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int IndexOf(T item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int Add(object value)
    {
        throw new NotImplementedException();
    }

    public bool Contains(object value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(object value)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, object value)
    {
        throw new NotImplementedException();
    }

    public void Remove(object value)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region IComparer<T>
    private class ItemComparer<TI> : IComparer<int>
    {
        public ItemComparer(IList<TI> items)
        {
            Items = items;
        }

        public IList<TI> Items { get; }

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

            int rc = 0;
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

internal class EventInvokerBase<TH, TE> where TH : Delegate where TE : EventArgs
{
    public EventInvokerBase(TH handler)
    {
        Handler = handler;
        Synchronized = (ISynchronizeInvoke)handler.Target;
    }

    public ISynchronizeInvoke Synchronized { get; }
    public TH Handler { get; }

    internal void RaiseEvent(object sender, TE eventArgs)
    {
        if (Synchronized.InvokeRequired)
        {
            Synchronized.Invoke(Handler, new object[] { sender, eventArgs });
        }
        else
        {
            Handler.DynamicInvoke(sender, eventArgs);
        }
    }
}

internal class EventInvoker<T> : EventInvokerBase<EventHandler<T>,T> where T : EventArgs
{
    public EventInvoker(EventHandler<T> handler) : base(handler)
    {
    }
}

    */

