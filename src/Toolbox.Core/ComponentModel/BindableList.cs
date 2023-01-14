using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Toolbox.Collection.Generics;

namespace Toolbox.ComponentModel
{
    /// <summary>
    /// A replacement for <see cref="BindingList{T}"/> with better events.
    /// </summary>
    public class BindableList<T> : Collection<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
    {
        /// <summary>
        /// Create a new instance of <see cref="BindableList{T}"/>.
        /// </summary>
        public BindableList()
        {
        }

        /// <summary>
        /// Create a new instance of <see cref="BindableList{T}"/>.
        /// </summary>
        public BindableList(IEnumerable<T> other)
        {
            other.ForEach(Add);
        }

        protected override void InsertItem(int index, T item)
        {
            if (!AllowEdit) throw new NotSupportedException();

            CommitItem();

            OnAddingItem(index, item);
            base.InsertItem(index, item);
            OnItemAdded(index, item);
            OnListChanged(ListChangedType.ItemAdded, index);
        }

        #region ItemAdded
        private event EventHandler<ItemEventArgs<T>> ItemAddedHandler;
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
            base.RemoveItem(index);
            OnItemRemoved(index, item);
            OnListChanged(ListChangedType.ItemDeleted, -1, index);
        }

        #region ItemRemoved
        private event EventHandler<ItemEventArgs<T>> ItemRemovedHandler;
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

        protected override void ClearItems()
        {
            if (!AllowRemove) throw new NotSupportedException();

            CommitItem();

            OnResetting();
            base.ClearItems();
            OnResetted();
            OnListChanged(ListChangedType.Reset, 0, 0);
        }

        #region Resetted
        private event EventHandler<ListEventArgs> ResettedHandler;
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

            var args = new ItemSetEventArgs<T>(index, this[index], item);
            OnSetting(args);
            base.SetItem(index, item);
            OnSet(args);
            OnListChanged(ListChangedType.ItemChanged, index);
        }

        #region Setting
        private event EventHandler<ItemSetEventArgs<T>> SettingItemHandler;
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
        public bool AllowNew { get; set; } = true;
        public bool AllowRemove { get; set; } = true;

        #region AddingNew
        private event AddingNewEventHandler AddingNewHandler;
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
        bool IBindingList.IsSorted => false;

        ListSortDirection IBindingList.SortDirection => throw new NotImplementedException();

        PropertyDescriptor IBindingList.SortProperty => throw new NotImplementedException();

        bool IBindingList.SupportsChangeNotification => true;

        bool IBindingList.SupportsSearching => false;

        bool IBindingList.SupportsSorting => false;

        bool IRaiseItemChangedEvents.RaisesItemChangedEvents => true;

        bool IBindingList.AllowEdit => AllowEdit;

        bool IBindingList.AllowNew => AllowNew;

        bool IBindingList.AllowRemove => AllowRemove;

        #region ListChanged
        private event ListChangedEventHandler ListChangedHandler;
        public event ListChangedEventHandler ListChanged
        {
            add => AddHandler(ref ListChangedHandler, value);
            remove => RemoveHandler(ref ListChangedHandler, value);
        }

        event ListChangedEventHandler IBindingList.ListChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        private void OnListChanged(ListChangedType type, int newIndex = 0, int oldIndex = 0)
        {
            var args = new ListChangedEventArgs(type, newIndex, oldIndex);
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
        public void CancelNew(int itemIndex)
        {
            throw new NotImplementedException();
        }
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
    public class ItemEventArgs<T> : EventArgs
    {
        public ItemEventArgs(int index, T item)
        {            
            Index = index;
            Item = item;
        }

        public T Item { get; }

        public int Index { get; }
    }

    public class ItemSetEventArgs<T> : EventArgs
    {
        public ItemSetEventArgs(int index, T oldItem, T newItem)
        {
            Index = index;
            OldItem = oldItem;
            NewItem = newItem;
        }
        public int Index { get; }
        public T OldItem { get; private set; }
        public T NewItem { get; }
        
    }

    public class ListEventArgs : EventArgs
    {
    }
    #endregion
}
