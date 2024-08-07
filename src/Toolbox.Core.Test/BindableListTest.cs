using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.ComponentModel;
using Toolbox.Core.Test.Model;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class BindableListTest
    {
        [TestMethod, TestCategory("create")]
        public void CreateWithAdd()
        {
            const string someName = "SomeName";

            var data = new Data { Name = someName };

            var cut = new BindableList<Data> { data };

            Assert.AreEqual(1, cut.Count);
            Assert.AreEqual(someName, cut[0].Name);
        }

        [TestMethod, TestCategory("create")]
        public void CreateWithEnumerable()
        {
            var datas = new[]
            {
                new Data(),
                new Data(),
                new Data()
            };

            var cut = new BindableList<Data>( datas );

            Assert.AreEqual(datas.Length, cut.Count);
            for (int i = 0; i < datas.Length; i++)
            {
                Assert.AreSame(datas[i], cut[i]);
            }
        }

		[TestMethod, TestCategory("add")]
        public void AddRange()
        {
            var range = new[]
            {
				new Data() { Name = "Data1" },
				new Data() { Name = "Data2" },
				new Data() { Name = "Data3" }
			};
			var cut = new BindableList<Data>();

            cut.AddRange(range);

            Assert.AreEqual(range.Length, cut.Count);
            for(int i = 0;i < range.Length; i++)
                Assert.AreSame(range[i], cut[i]);
		}

		[TestMethod, TestCategory("add")]
        public void AddWithEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data> { new() };

            var adding = new Handler<ItemEventArgs<Data>>(cut);
            var added = new Handler<ItemEventArgs<Data>>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;

            cut.Add(data);

            Assert.AreEqual(2, cut.Count);

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(1, changed.Calls[0].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("add")]
        public void AddWithSynchronizedEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data> { new() };

            var adding = new SynchronizedHandler<ItemEventArgs<Data>>(cut);
            var added = new SynchronizedHandler<ItemEventArgs<Data>>(cut);
            var changed = new SynchronizedHandler<ListChangedEventArgs>(cut);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;

            cut.Add(data);

            Assert.AreEqual(2, cut.Count);

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(1, changed.Calls[0].EventArgs.NewIndex);
        }


        [TestMethod, TestCategory("add")]
        public void AddNullWithEvents()
        {
            var cut = new BindableList<Data> { new() };

            var adding = new Handler<ItemEventArgs<Data>>(cut);
            var added = new Handler<ItemEventArgs<Data>>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;

            cut.Add(null);

            Assert.AreEqual(2, cut.Count);

            adding.AssertCalls();
            Assert.IsNull(adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.IsNull(added.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(1, changed.Calls[0].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("add")]
        public void AddNewWithEvents()
        {
            var cut = new BindableList<Data> { new() };

            var newHandler = new Handler<AddingNewEventArgs>(cut);
            var adding = new Handler<ItemEventArgs<Data>>(cut);
            var added = new Handler<ItemEventArgs<Data>>(cut, 2);
            var changed = new Handler<ListChangedEventArgs>(cut, 2);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;
            cut.AddingNew += newHandler.Raised;          

            var bindingList = cut as IBindingList;
            var cancelAddNew = cut as ICancelAddNew;

            var data = bindingList.AddNew();
            cancelAddNew.EndNew(bindingList.IndexOf(data));

            Assert.AreEqual(2, cut.Count);
            Assert.IsInstanceOfType(data, typeof(Data));

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[0].EventArgs.Index);
            Assert.AreSame(data, added.Calls[1].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[1].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[1].EventArgs.ListChangedType);
            Assert.AreEqual(1, changed.Calls[0].EventArgs.NewIndex);
            Assert.AreEqual(1, changed.Calls[1].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("add")]
        public void AddNewCustomObjectWithEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data> { new() };

            var newHandler = new Handler<AddingNewEventArgs>(cut);
            var adding = new Handler<ItemEventArgs<Data>>(cut);
            var added = new Handler<ItemEventArgs<Data>>(cut, 2);
            var changed = new Handler<ListChangedEventArgs>(cut, 2);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;
            cut.AddingNew += newHandler.Raised;
            cut.AddingNew += (s, e) => e.NewObject = data;

            var bindingList = cut as IBindingList;
            var cancelAddNew = cut as ICancelAddNew;

            var rc = bindingList.AddNew();
            cancelAddNew.EndNew(bindingList.IndexOf(rc));

            Assert.AreEqual(2, cut.Count);
            Assert.IsInstanceOfType(rc, typeof(Data));
            Assert.AreSame(data, rc);

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[0].EventArgs.Index);
            Assert.AreSame(data, added.Calls[1].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[1].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[1].EventArgs.ListChangedType);
            Assert.AreEqual(1, changed.Calls[0].EventArgs.NewIndex);
            Assert.AreEqual(1, changed.Calls[1].EventArgs.NewIndex);
        }



        [TestMethod, TestCategory("add")]
        public void AddNewImplicitCommtWithEvents()
        {
            var cut = new BindableList<Data> { new() };

            var adding = new Handler<ItemEventArgs<Data>>(cut);
            var added = new Handler<ItemEventArgs<Data>>(cut, 2);
            var changed = new Handler<ListChangedEventArgs>(cut, 3);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;

            var bindingList = cut as IBindingList;
            
            var data = bindingList.AddNew();
            cut[0] = new Data();
            
            Assert.AreEqual(2, cut.Count);
            Assert.IsInstanceOfType(data, typeof(Data));

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[0].EventArgs.Index);
            Assert.AreSame(data, added.Calls[1].EventArgs.Item);
            Assert.AreEqual(1, added.Calls[1].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[1].EventArgs.ListChangedType);
            Assert.AreEqual(ListChangedType.ItemChanged, changed.Calls[2].EventArgs.ListChangedType);
            Assert.AreEqual(1, changed.Calls[0].EventArgs.NewIndex);
            Assert.AreEqual(1, changed.Calls[1].EventArgs.NewIndex);
            Assert.AreEqual(0, changed.Calls[2].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("set")]
        public void SetWithEvents()
        {
            var oldData = new Data();
            var newData = new Data();

            const int index = 1;

            var cut = new BindableList<Data> { new(), oldData };

            var setting = new Handler<ItemSetEventArgs<Data>>(cut);
            var set = new Handler<ItemSetEventArgs<Data>>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.SettingItem += setting.Raised;
            cut.ItemSet += set.Raised;
            cut.ListChanged += changed.Raised;

            cut[index] = newData;

            Assert.AreEqual(2, cut.Count);

            setting.AssertCalls();
            Assert.AreSame(oldData, setting.Calls[0].EventArgs.OldItem);
            Assert.AreSame(newData, setting.Calls[0].EventArgs.NewItem);
            Assert.AreEqual(index, setting.Calls[0].EventArgs.Index);

            set.AssertCalls();
            Assert.AreSame(oldData, set.Calls[0].EventArgs.OldItem);
            Assert.AreSame(newData, set.Calls[0].EventArgs.NewItem);
            Assert.AreEqual(index, set.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemChanged, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(index, changed.Calls[0].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("add")]
        [ExpectedException(typeof(NotSupportedException))]
        public void AddOnProtectedList()
        {
            var data = new Data();

            var cut = new BindableList<Data> { AllowEdit = false };

            cut.Add(data);
        }

        [TestMethod, TestCategory("event")]
        public void AddRemoveEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data>();

            var adding = new Handler<ItemEventArgs<Data>>(cut, 0);
            var added = new Handler<ItemEventArgs<Data>>(cut, 0);
            var listChanged = new Handler<ListChangedEventArgs>(cut, 0);
            var itemChanged = new Handler<ItemChangedEventArgs<Data>>(cut, 0);
            var resetting = new Handler<ListResetEventArgs>(cut,0);
            var resetted = new Handler<ListResetEventArgs>(cut, 0);
            var setting = new Handler<ItemSetEventArgs<Data>>(cut, 0);
            var set = new Handler<ItemSetEventArgs<Data>>(cut, 0);

            cut.ItemAdded += added.Raised;
            cut.ItemAdded -= added.Raised;
            cut.AddingItem += adding.Raised;
            cut.AddingItem -= adding.Raised;
            cut.ListChanged += listChanged.Raised;
            cut.ListChanged -= listChanged.Raised;
            cut.ItemChanged += itemChanged.Raised;
            cut.ItemChanged -= itemChanged.Raised;
            cut.Resetting += resetting.Raised;
            cut.Resetting -= resetting.Raised;
            cut.Resetted += resetted.Raised;
            cut.Resetted -= resetted.Raised;
            cut.SettingItem += setting.Raised;
            cut.SettingItem -= setting.Raised;
            cut.ItemSet += set.Raised;
            cut.ItemSet -= set.Raised;

            cut.Add(data);
            cut[0] = new Data
            {
                Name = "NewName"
            };
            cut.RemoveAt(0);
            cut.Clear();

            adding.AssertCalls();
            added.AssertCalls();
            listChanged.AssertCalls();
            itemChanged.AssertCalls();
            resetting.AssertCalls();
            resetted.AssertCalls();
            setting.AssertCalls();
            set.AssertCalls();
        }

        [TestMethod, TestCategory("event")]
        public void AddRemoveSynchronizedEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data>();

            var adding = new SynchronizedHandler<ItemEventArgs<Data>>(cut, 0);
            var added = new SynchronizedHandler<ItemEventArgs<Data>>(cut, 0);
            var changed = new SynchronizedHandler<ListChangedEventArgs>(cut, 0);
            var resetting = new SynchronizedHandler<ListResetEventArgs>(cut, 0);
            var resetted = new SynchronizedHandler<ListResetEventArgs>(cut, 0);
            var setting = new SynchronizedHandler<ItemSetEventArgs<Data>>(cut, 0);
            var set = new SynchronizedHandler<ItemSetEventArgs<Data>>(cut, 0);

            cut.ItemAdded += added.Raised;
            cut.ItemAdded -= added.Raised;
            cut.AddingItem += adding.Raised;
            cut.AddingItem -= adding.Raised;
            cut.ListChanged += changed.Raised;
            cut.ListChanged -= changed.Raised;
            cut.Resetting += resetting.Raised;
            cut.Resetting -= resetting.Raised;
            cut.Resetted += resetted.Raised;
            cut.Resetted -= resetted.Raised;
            cut.SettingItem += setting.Raised;
            cut.SettingItem -= setting.Raised;
            cut.ItemSet += set.Raised;
            cut.ItemSet -= set.Raised;

            cut.Add(data);
            cut[0] = new Data();
            cut.RemoveAt(0);
            cut.Clear();

            adding.AssertCalls();
            added.AssertCalls();
            changed.AssertCalls();
            resetting.AssertCalls();
            resetted.AssertCalls();
            setting.AssertCalls();
            set.AssertCalls();
        }

        [TestMethod, TestCategory("clear")]
        public void ClearItemsWithEvents()
        {
            var cut = new BindableList<Data>
            {
                new(), new(), new(), new()
            };

            var resetting = new Handler<ListResetEventArgs>(cut);
            var resetted = new Handler<ListResetEventArgs>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.Resetting += resetting.Raised;
            cut.Resetted += resetted.Raised;
            cut.ListChanged += changed.Raised;

            cut.Clear();

            Assert.AreEqual(0, cut.Count);

            resetting.AssertCalls();
                        
            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.Reset, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(0, changed.Calls[0].EventArgs.NewIndex);
            Assert.AreEqual(0, changed.Calls[0].EventArgs.OldIndex);
        }

        [TestMethod]
        public void CheckSupportsChangeNotification()
        {
            var cut = new BindableList<Data>();

            var bindingList = cut as IBindingList;

            Assert.IsNotNull(bindingList);
            Assert.IsTrue(bindingList.SupportsChangeNotification);            
        }

        [TestMethod, TestCategory("remove")]
        public void RemoveItemWithEvents()
        {
            var data1 = new Data();
            var data2 = new Data();

            var cut = new BindableList<Data> {  data1, data2 };

            var removing = new Handler<ItemEventArgs<Data>>(cut);
            var removed = new Handler<ItemEventArgs<Data>>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.RemovingItem += removing.Raised;
            cut.ItemRemoved += removed.Raised;
            cut.ListChanged += changed.Raised;

            var rc = cut.Remove(data1);

            Assert.IsTrue(rc);

            Assert.AreEqual(1, cut.Count);
            Assert.AreSame(data2, cut[0]);

            removing.AssertCalls();
            Assert.AreSame(data1, removing.Calls[0].EventArgs.Item);
            Assert.AreEqual(0, removing.Calls[0].EventArgs.Index);

            removed.AssertCalls();
            Assert.AreSame(data1, removed.Calls[0].EventArgs.Item);
            Assert.AreEqual(0, removed.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemDeleted, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(0, changed.Calls[0].EventArgs.OldIndex);
        }

        [TestMethod, TestCategory("remove")]
        public void RemoveAtWithEvents()
        {
            var data1 = new Data();
            var data2 = new Data();

            var cut = new BindableList<Data> { data1, data2 };

            var removing = new Handler<ItemEventArgs<Data>>(cut);
            var removed = new Handler<ItemEventArgs<Data>>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.RemovingItem += removing.Raised;
            cut.ItemRemoved += removed.Raised;
            cut.ListChanged += changed.Raised;

            cut.RemoveAt(0);

            Assert.AreEqual(1, cut.Count);
            Assert.AreSame(data2, cut[0]);

            removing.AssertCalls();
            Assert.AreSame(data1, removing.Calls[0].EventArgs.Item);
            Assert.AreEqual(0, removing.Calls[0].EventArgs.Index);

            removed.AssertCalls();
            Assert.AreSame(data1, removed.Calls[0].EventArgs.Item);
            Assert.AreEqual(0, removed.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemDeleted, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(0, changed.Calls[0].EventArgs.OldIndex);
        }

        [TestMethod, TestCategory("remove")]
        [ExpectedException(typeof(NotSupportedException))]
        public void RemoveOnProtectedList()
        {
            var data1 = new Data();
            var data2 = new Data();

            var cut = new BindableList<Data> { data1, data2 };
            cut.AllowRemove = false;

            cut.Remove(data1);
        }

        [TestMethod, TestCategory("remove")]
        [ExpectedException(typeof(NotSupportedException))]
        public void RemoveAtProtectedList()
        {
            var data1 = new Data();
            var data2 = new Data();

            var cut = new BindableList<Data> { data1, data2 };
            cut.AllowRemove = false;

            cut.RemoveAt(0);
        }

        [TestMethod, TestCategory("Insert")]
        public void InsertWithEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data>
            {
                new(), new(), new(), new()
            };

            var adding = new Handler<ItemEventArgs<Data>>(cut);
            var added = new Handler<ItemEventArgs<Data>>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;

            cut.Insert(3, data);

            Assert.AreEqual(5, cut.Count);

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(3, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(3, added.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(3, changed.Calls[0].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("insert")]
        public void InsertWithSynchronizedEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data>
            {
                new(), new(), new(), new()
            };

            var adding = new SynchronizedHandler<ItemEventArgs<Data>>(cut);
            var added = new SynchronizedHandler<ItemEventArgs<Data>>(cut);
            var changed = new SynchronizedHandler<ListChangedEventArgs>(cut);

            cut.AddingItem += adding.Raised;
            cut.ItemAdded += added.Raised;
            cut.ListChanged += changed.Raised;

            cut.Insert(3, data);

            Assert.AreEqual(5, cut.Count);

            adding.AssertCalls();
            Assert.AreSame(data, adding.Calls[0].EventArgs.Item);
            Assert.AreEqual(3, adding.Calls[0].EventArgs.Index);

            added.AssertCalls();
            Assert.AreSame(data, added.Calls[0].EventArgs.Item);
            Assert.AreEqual(3, added.Calls[0].EventArgs.Index);

            changed.AssertCalls();
            Assert.AreEqual(ListChangedType.ItemAdded, changed.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(3, changed.Calls[0].EventArgs.NewIndex);
        }

        [TestMethod, TestCategory("event")]
        public void ItemChangeWithEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data>
            {
                data
            };

            var itemChanged = new Handler<ItemChangedEventArgs<Data>>(cut);
            var listChanged = new Handler<ListChangedEventArgs>(cut);

            cut.ItemChanged += itemChanged.Raised;
            cut.ListChanged += listChanged.Raised;

            data.Name = "new Name";

            itemChanged.AssertCalls();
            listChanged.AssertCalls();

            Assert.AreEqual(0, itemChanged.Calls[0].EventArgs.Index);
            Assert.AreSame(data, itemChanged.Calls[0].EventArgs.Item);
            Assert.AreEqual(nameof(Data.Name), itemChanged.Calls[0].EventArgs.PropertyName);

            Assert.AreEqual(ListChangedType.ItemChanged, listChanged.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(0, listChanged.Calls[0].EventArgs.NewIndex);           
            Assert.AreEqual(nameof(Data.Name), listChanged.Calls[0].EventArgs.PropertyDescriptor.Name);
        }

        [TestMethod, TestCategory("event")]
        public void ItemChangeAfterSetWithEvents()
        {
            var data = new Data();

            var cut = new BindableList<Data>() { new() };

            var itemChanged = new Handler<ItemChangedEventArgs<Data>>(cut);
            var listChanged = new Handler<ListChangedEventArgs>(cut);

            cut[0] = data;

            cut.ItemChanged += itemChanged.Raised;
            cut.ListChanged += listChanged.Raised;

            data.Name = "new Name";

            itemChanged.AssertCalls();
            listChanged.AssertCalls();

            Assert.AreEqual(0, itemChanged.Calls[0].EventArgs.Index);
            Assert.AreSame(data, itemChanged.Calls[0].EventArgs.Item);
            Assert.AreEqual(nameof(Data.Name), itemChanged.Calls[0].EventArgs.PropertyName);

            Assert.AreEqual(ListChangedType.ItemChanged, listChanged.Calls[0].EventArgs.ListChangedType);
            Assert.AreEqual(0, listChanged.Calls[0].EventArgs.NewIndex);
            Assert.AreEqual(nameof(Data.Name), listChanged.Calls[0].EventArgs.PropertyDescriptor.Name);
        }

        [TestMethod, TestCategory("event")]
        public void NoItemChangeAfterRemove()
        {
            var data = new Data();

            var cut = new BindableList<Data>
            {
                data
            };

            var itemChanged = new Handler<ItemChangedEventArgs<Data>>(cut, 0);
            var listChanged = new Handler<ListChangedEventArgs>(cut, 0);

            cut.Remove(data);

            cut.ItemChanged += itemChanged.Raised;
            cut.ListChanged += listChanged.Raised;

            data.Name = "new Name";

            itemChanged.AssertCalls();
            listChanged.AssertCalls();
        }

        [TestMethod, TestCategory("event")]
        public void NoItemChangeAfterSetReplacement()
        {
            var data = new Data();

            var cut = new BindableList<Data>
            {
                data
            };

            var itemChanged = new Handler<ItemChangedEventArgs<Data>>(cut, 0);
            var listChanged = new Handler<ListChangedEventArgs>(cut, 0);

            cut[0] = new Data();

            cut.ItemChanged += itemChanged.Raised;
            cut.ListChanged += listChanged.Raised;

            data.Name = "new Name";

            itemChanged.AssertCalls();
            listChanged.AssertCalls();
        }

        [TestMethod, TestCategory("event")]
        public void NoItemChangeAfterClear()
        {
            var data = new Data();

            var cut = new BindableList<Data>
            {
                data
            };

            var itemChanged = new Handler<ItemChangedEventArgs<Data>>(cut, 0);
            var listChanged = new Handler<ListChangedEventArgs>(cut, 0);

            cut.Clear();

            cut.ItemChanged += itemChanged.Raised;
            cut.ListChanged += listChanged.Raised;

            data.Name = "new Name";

            itemChanged.AssertCalls();
            listChanged.AssertCalls();
        }

        private static Data[] CreateData(int length)
        {
            var datas = new Data[length];
            for (int i = 0; i < length; i++)
            {
                datas[i] = new Data();
            }
            return datas;
        }

        private static Data[] AddData(BindableList<Data> list, int length)
        {
            var datas = CreateData(length);
            foreach (var data in datas)
            {
                list.Add(data);
            }
            return datas;
        }

        [TestMethod]
        public void AddSorted()
        {
            var cut = new BindableList<Data>();

            var raisedEvents = new List<ListChangedEventArgs>();
            cut.ListChanged += (s, e) => raisedEvents.Add(e);

            cut.ApplySort(nameof(Data.Name), ListSortDirection.Ascending);

            var datas = AddData(cut, 3);

            var sorted = datas.OrderBy(d => d.Name).ToArray();

            Assert.AreEqual(datas.Length, cut.Count);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(sorted[i].Name, cut[i].Name, $"item[{i}]");
            }
        }

        [TestMethod]
        public void AddThenSort()
        {
            var cut = new BindableList<Data>();

            var datas = AddData(cut, 5);

            var resetting = new Handler<ListResetEventArgs>(cut);
            var resetted = new Handler<ListResetEventArgs>(cut);
            var changed = new Handler<ListChangedEventArgs>(cut);

            cut.Resetting += resetting.Raised;
            cut.Resetted += resetted.Raised;
            cut.ListChanged += changed.Raised;


            const string propertyName = nameof(Data.Name);
            const ListSortDirection direction = ListSortDirection.Ascending;

            cut.ApplySort(propertyName, direction);

            var sorted = datas.OrderBy(d => d.Name).ToArray();

            resetting.AssertCalls();
            resetted.AssertCalls();
            changed.AssertCalls();

            Assert.AreEqual(datas.Length, cut.Count);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(sorted[i], cut[i]);
            }
            Assert.IsTrue(cut.IsSorted);
            Assert.AreEqual(propertyName, cut.SortProperty.Name);
            Assert.AreEqual(direction, cut.SortDirection);
            Assert.AreEqual(ListChangedType.Reset, changed.Calls[0].EventArgs.ListChangedType);
        }

        [TestMethod]
        public void AddThenSortDescending()
        {
            var cut = new BindableList<Data>();

            var datas = AddData(cut, 5);

            var changed = new Handler<ListChangedEventArgs>(cut);
            cut.ListChanged += changed.Raised;

            const string propertyName = nameof(Data.Name);
            const ListSortDirection direction = ListSortDirection.Descending;

            cut.ApplySort(propertyName, direction);

            var sorted = datas.OrderByDescending(d => d.Name).ToArray();
            
            Assert.AreEqual(datas.Length, cut.Count);
            for (var i = 0; i < datas.Length; i++)
            {
                Assert.AreEqual(sorted[i], cut[i]);
            }
            Assert.IsTrue(cut.IsSorted);
            Assert.AreEqual(propertyName, cut.SortProperty.Name);
            Assert.AreEqual(direction, cut.SortDirection);
            Assert.AreEqual(ListChangedType.Reset, changed.Calls[0].EventArgs.ListChangedType);
        }

        [TestMethod]
        public void Contains()
        {
            var cut = new BindableList<Data>();

            var data = new Data();

            cut.Add(data);

            var rc = cut.Contains(data);

            Assert.IsTrue(rc);
        }

        [TestMethod]
        public void IndexOf()
        {
            var cut = new BindableList<Data>();

            var datas = AddData(cut, 3);

            var rc = cut.IndexOf(datas[1]);

            Assert.AreEqual(1, rc);
        }

        [TestMethod]
        public void IndexOfNotFound()
        {
            var cut = new BindableList<Data>();

            AddData(cut, 3);

            var rc = cut.IndexOf(new Data());

            Assert.AreEqual(-1, rc);
        }

        [TestMethod]
        public void EnumerateUnsorted()
        {
            var cut = new BindableList<Data>();

            var datas = AddData(cut, 6);

            Assert.AreEqual(datas.Length, cut.Count);

            var i = 0;
            foreach (var item in cut)
            {
                Assert.AreEqual(datas[i++], item, $"item[{i}]");
            }
        }

        [TestMethod]
        public void EnumerateSorted()
        {
            var cut = new BindableList<Data>();

            var datas = AddData(cut, 6);

            cut.ApplySort(nameof(Data.Name), ListSortDirection.Ascending);
            var sorted = datas.OrderBy(d => d.Name).ToArray();

            Assert.AreEqual(sorted.Length, cut.Count);

            var i = 0;
            foreach (var item in cut)
            {
                Assert.AreEqual(sorted[i++], item, $"item[{i}]");
            }
        }

        [TestMethod]
        public void Find()
        {
            var cut = new BindableList<Data>();

            var datas = AddData(cut, 10);

            const int findIndex = 6;

            var index = cut.Find(nameof(Data.Name), datas[findIndex].Name);

            Assert.AreEqual(findIndex, index);
        }

        #region Handler
        class Handler<T> where T : EventArgs
        {
            public Handler(object sender, int excepted = 1)
            {
                Sender = sender;
                Expected = excepted;
            }

            public void AssertCalls()
            {
                Assert.AreEqual(Expected, Calls.Count);
                for (var i = 0; i < Calls.Count; i++)
                {
                    Assert.AreSame(Sender, Calls[i].Sender, $"sender on call[{i}]");
                }
            }

            public object Sender { get; }
            public int Expected { get; private set; }
            public List<Call<T>> Calls { get; } = new List<Call<T>>();
            
            public virtual void Raised(object sender, T args)
            {
                Calls.Add(new Call<T>(sender, args));
            }
        }

        class Call<T>
        {
            public Call(object sender, T eventArgs)
            {
                Sender = sender;
                EventArgs = eventArgs;
            }

            public object Sender { get; }
            public T EventArgs { get; }
        }
        #endregion
        #region SynchronizedHandler
        class SynchronizedHandler<T> : Handler<T>, ISynchronizeInvoke where T : EventArgs
        {
            static SynchronizedHandler()
            {
                DataSlotInvoked = Thread.AllocateDataSlot();
            }

            private static LocalDataStoreSlot DataSlotInvoked { get; }

            public SynchronizedHandler(object sender, int excepted = 1, bool invokeRequired = true) : base(sender, excepted)
            {
                InvokeRequired = invokeRequired;
            }

            public override void Raised(object sender, T args)
            {
                var invoked = Thread.GetData(DataSlotInvoked) ?? false;

                Assert.IsTrue(!InvokeRequired || (bool)invoked, $"Handler called wihtout Invoke()");
                    
                base.Raised(sender, args);
            }

            public bool InvokeRequired { get; }

            public IAsyncResult BeginInvoke(Delegate method, object[] args)
            {
                throw new NotImplementedException();
            }

            public object EndInvoke(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public object Invoke(Delegate method, object[] args)
            {
                Thread.SetData(DataSlotInvoked, true);
                method.DynamicInvoke(args);
                Thread.SetData(DataSlotInvoked, false);
                return null;
            }
        }
        #endregion
    }
}