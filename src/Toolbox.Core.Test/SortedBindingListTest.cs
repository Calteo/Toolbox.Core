using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Toolbox.ComponentModel;
using Toolbox.Core.Test.Model;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class SortedBindingListTest
    {
        private Data[] CreateData(int length)
        {
            var datas = new Data[length];
            for (int i = 0; i < length; i++)
            {
                datas[i] = new Data();
            }
            return datas;
        }

        private Data[] AddData(SortedBindingList<Data> list, int length)
        {
            var datas = CreateData(length);
            foreach (var data in datas)
            {
                list.Add(data);
            }
            return datas;
        }

        [TestMethod]
        public void Add()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var data = new Data();

            cut.Add(data);

            Assert.AreEqual(1, cut.Count);
            Assert.AreEqual(data, cut[0]);
            Assert.AreEqual(1, raisedEvents.Count);
            Assert.AreEqual(ListChangedType.ItemAdded, raisedEvents[0].ListChangedType);
            Assert.AreEqual(0, raisedEvents[0].NewIndex);
        }

        [TestMethod]
        public void Insert()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var datas = AddData(cut, 5);
            var data = new Data();

            const int index = 2;

            cut.Insert(index, data);
            var last = raisedEvents.Last();

            Assert.AreEqual(datas.Length+1, cut.Count);

            Assert.AreEqual(data, cut[index]);
            
            Assert.AreEqual(ListChangedType.ItemAdded, last.ListChangedType);
            Assert.AreEqual(index, last.NewIndex);
        }

        [TestMethod]
        public void AddSorted()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            cut.ApplySort(nameof(Data.Name), ListSortDirection.Ascending);

            var datas = AddData(cut, 3);

            var sorted = datas.OrderBy(d => d.Name).ToArray();

            Assert.AreEqual(datas.Length, cut.Count);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(sorted[i], cut[i]);
            }
        }

        [TestMethod]
        public void AddThenSort()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var datas = AddData(cut, 5);

            const string propertyName = nameof(Data.Name);
            const ListSortDirection direction = ListSortDirection.Ascending;

            cut.ApplySort(propertyName, direction);

            var sorted = datas.OrderBy(d => d.Name).ToArray();
            var last = raisedEvents.Last();

            Assert.AreEqual(datas.Length, cut.Count);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(sorted[i], cut[i]);
            }
            Assert.IsTrue(cut.IsSorted);
            Assert.AreEqual(propertyName, cut.SortProperty.Name);
            Assert.AreEqual(direction, cut.SortDirection);
            Assert.AreEqual(ListChangedType.Reset, last.ListChangedType);
        }

        [TestMethod]
        public void AddThenSortDescending()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var datas = AddData(cut, 5);

            const string propertyName = nameof(Data.Name);
            const ListSortDirection direction = ListSortDirection.Descending;

            cut.ApplySort(propertyName, direction);

            var sorted = datas.OrderByDescending(d => d.Name).ToArray();
            var last = raisedEvents.Last();

            Assert.AreEqual(datas.Length, cut.Count);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(sorted[i], cut[i]);
            }
            Assert.IsTrue(cut.IsSorted);
            Assert.AreEqual(propertyName, cut.SortProperty.Name);
            Assert.AreEqual(direction, cut.SortDirection);
            Assert.AreEqual(ListChangedType.Reset, last.ListChangedType);
        }

        /// <inheritdoc cref="ICollection{T}" />
        [TestMethod]
        public void CopyTo()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var datas = AddData(cut, 10);
            var result = new Data[datas.Length];

            cut.CopyTo(result);

            Assert.AreEqual(datas.Length, result.Length);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(datas[i], result[i]);
            }
        }

        [TestMethod]
        public void Clear()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var data = new Data();

            cut.Add(data);
            cut.Clear();

            var last = raisedEvents.Last();

            Assert.AreEqual(0, cut.Count);
            Assert.AreEqual(ListChangedType.Reset, last.ListChangedType);
        }

        [TestMethod]
        public void Contains()
        {
            var cut = new SortedBindingList<Data>();

            var data = new Data();

            cut.Add(data);

            var rc = cut.Contains(data);

            Assert.IsTrue(rc);
        }

        [TestMethod]
        public void IndexOf()
        {
            var cut = new SortedBindingList<Data>();

            var datas = AddData(cut, 3);

            var rc = cut.IndexOf(datas[1]);

            Assert.AreEqual(1, rc);
        }

        [TestMethod]
        public void IndexOfNotFound()
        {
            var cut = new SortedBindingList<Data>();

            AddData(cut, 3);

            var rc = cut.IndexOf(new Data());

            Assert.AreEqual(-1, rc);
        }

        [TestMethod]
        public void Remove()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var data = new Data();

            cut.Add(data);
            cut.Remove(data);

            var last = raisedEvents.Last();

            Assert.AreEqual(0, cut.Count);
            Assert.AreEqual(ListChangedType.ItemDeleted, last.ListChangedType);
            Assert.AreEqual(0, last.NewIndex);
        }

        [TestMethod]
        public void RemoveAt()
        {
            var cut = new SortedBindingList<Data>();
            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            var datas = AddData(cut, 3);

            const int at = 1;

            cut.RemoveAt(at);
            var last = raisedEvents.Last();

            Assert.AreEqual(datas.Length-1, cut.Count);
            for (int i = 0, j=0; i < datas.Length; i++)
            {
                if (i == at) continue;
                Assert.AreEqual(datas[i], cut[j]);
                j++;
            }
            Assert.AreEqual(ListChangedType.ItemDeleted, last.ListChangedType);
            Assert.AreEqual(at, last.NewIndex);
        }

        [TestMethod]
        public void SetAt()
        {
            var cut = new SortedBindingList<Data>();

            var datas = CreateData(2);

            cut.Add(datas[0]);

            cut[0] = datas[1];

            Assert.AreEqual(1, cut.Count);
            Assert.AreEqual(datas[1], cut[0]);
        }

        [TestMethod]
        public void EnumerateUnsorted()
        {
            var cut = new SortedBindingList<Data>();

            var datas = AddData(cut, 6);

            Assert.AreEqual(datas.Length, cut.Count);

            var i = 0;
            foreach (var item in cut)
            {
                Assert.AreEqual(datas[i++], item);
            }
        }

        [TestMethod]
        public void EnumerateSorted()
        {
            var cut = new SortedBindingList<Data>();

            var datas = AddData(cut, 6);

            cut.ApplySort(nameof(Data.Name), ListSortDirection.Ascending);
            var sorted = datas.OrderBy(d => d.Name).ToArray();

            Assert.AreEqual(sorted.Length, cut.Count);

            var i = 0;
            foreach (var item in cut)
            {
                Assert.AreEqual(sorted[i++], item);
            }
        }

        [TestMethod]
        public void Find()
        {
            var cut = new SortedBindingList<Data>();

            var datas = AddData(cut, 10);

            const int findIndex = 6;

            var index = cut.Find(nameof(Data.Name), datas[findIndex].Name);

            Assert.AreEqual(findIndex, index);           
        }
    }
}