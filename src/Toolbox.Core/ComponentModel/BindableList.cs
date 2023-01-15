using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class BindableList<T> : Collection<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
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
        }

        private Dictionary<string, PropertyDescriptor> DataProperties { get; }

        /// <summary>
        /// Create a new instance of <see cref="BindableList{T}"/>.
        /// </summary>
        public BindableList(IEnumerable<T> other) : this()
        {
            other.ForEach(Add);
        }

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

        #region Resetted
        private event EventHandler<ListEventArgs> ResettedHandler;
        /// <summary>
        /// Raised after the collection is reset.
        /// </summary>
        public event EventHandler<ListEventArgs> Resetted
        {
            add => AddHandler(ref ResettedHandler, value);
            remove => RemoveHandler(ref ResettedHandler, value);
        }
        private void OnResetted()
        {
            ResettedHandler?.Invoke(this, new ListEventArgs());
        }
        #endregion
        #region Resetting
        private event EventHandler<ListEventArgs> ResettingHandler;
        /// <summary>
        /// Raised before the collection is resetting.
        /// </summary>
        public event EventHandler<ListEventArgs> Resetting
        {
            add => AddHandler(ref ResettingHandler, value);
            remove => RemoveHandler(ref ResettingHandler, value);
        }

        private void OnResetting()
        {
            ResettingHandler?.Invoke(this, new ListEventArgs());
        }
        #endregion

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
        /// <summary>
        /// Is using <see cref="AddNew"/> allowed.
        /// </summary>
        public bool AllowNew { get; set; } = true;
        /// <summary>
        /// Can items be removed.
        /// </summary>
        public bool AllowRemove { get; set; } = true;

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

        #region IBindingList 
        /// <summary>
        /// Is the collection sorted.
        /// </summary>
        bool IBindingList.IsSorted => false;

        /// <summary>
        /// Direction of sort
        /// </summary>
        ListSortDirection IBindingList.SortDirection => throw new NotImplementedException();

        /// <summary>
        /// Property used for sorting
        /// </summary>
        PropertyDescriptor IBindingList.SortProperty => throw new NotImplementedException();

        /// <summary>
        /// Supports the <see cref="INotifyPropertyChanged"/> events.
        /// </summary>
        bool IBindingList.SupportsChangeNotification => true;

        /// <summary>
        /// Support searching of an item.
        /// </summary>
        bool IBindingList.SupportsSearching => false;

        /// <summary>
        /// Support sorting of collection.
        /// </summary>
        bool IBindingList.SupportsSorting => false;

        private readonly bool _raisesItemChangedEvents;
        /// <summary>
        /// Collection will raise item change events.
        /// </summary>
        bool IRaiseItemChangedEvents.RaisesItemChangedEvents => _raisesItemChangedEvents;

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
        #endregion

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

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotImplementedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotImplementedException();
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
        #region ICancelAddNew
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
        public string PropertyName { get;  }
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
    public class ListEventArgs : EventArgs
    {
    }
    #endregion
}
