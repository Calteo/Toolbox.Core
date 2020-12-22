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
        IBindingList, IRaiseItemChangedEvents, ICancelAddNew
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
        
        private const bool IsReadOnly = false;

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

        public void Clear()
        {
            Items.Clear();
            Indices.Clear();
            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset,0));
        }
        #endregion

        #region IList<T>
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
        public bool RaisesItemChangedEvents => true;
        #endregion

        #region ICollection<T>
        public void Add(T item)
        {
            AddCore(item);
        }

        bool ICollection<T>.IsReadOnly => IsReadOnly;

        public int Count => Items.Count;          

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => Items;
        #endregion

        #region IBindingList
        bool IBindingList.AllowEdit => true;

        bool IBindingList.AllowNew => true;

        bool IBindingList.AllowRemove => true;

        public bool IsSorted => Comparer.IsSorted;

        public ListSortDirection SortDirection => Comparer.SortDirection;

        public PropertyDescriptor SortProperty => Comparer.SortProperty;

        bool IBindingList.SupportsChangeNotification => true;

        bool IBindingList.SupportsSearching => true;

        bool IBindingList.SupportsSorting => true;

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        object IBindingList.AddNew()
        {
            throw new NotImplementedException();
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            Comparer.SortDirection = direction;
            Comparer.IsSorted = true;
            Comparer.SortProperty = property;

            CreateIndices();
        }

        public void ApplySort(string propertyName, ListSortDirection direction)
        {
            var property = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, false);
            if (property == null)
                throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

            ApplySort(property, direction);
        }


        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        public void RemoveSort()
        {
            Comparer.IsSorted = false;
            Comparer.SortDirection = ListSortDirection.Ascending;
            Comparer.SortProperty = null;

            CreateIndices();
        }

        public event ListChangedEventHandler ListChanged;
        #endregion

        void ICancelAddNew.CancelNew(int itemIndex)
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            if (value is T tValue)
                return Contains(tValue);

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex = 0)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex > Count) throw new ArgumentException("Target arry to small.");

            for (var i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = Items[Indices[i]];
            }
        }

        void ICancelAddNew.EndNew(int itemIndex)
        {
            throw new NotImplementedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            if (value is T tValue)
                return IndexOf(tValue);

            throw new ArgumentException($"Value is not of type {typeof(T).FullName}.", nameof(value));
        }

        public int IndexOf(T item)
        {
            var dataIndex = Items.IndexOf(item);
            return dataIndex >= 0 ? Indices.IndexOf(dataIndex) : -1;
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
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

        public bool Remove(T item)
        {
            return RemoveCore(item);
        }

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

            Items[Indices[index]] = value;
            ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index));
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
        }

        private void RemoveAtCore(int index)
        {
            var dataIndex = Indices[index];
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
    }
}
