using System;
using System.ComponentModel;
using System.Diagnostics;
using Toolbox.ComponentModel;

namespace Toolbox.Core.Test.Model
{
	[DebuggerDisplay("{Name} - {Timestamp}")]
    class Data : NotifyObject
    {
        public Data()
        {
            Name = $"Test-{GetHashCode()}";
            Timestamp = DateTime.Now;            
        }

        #region Name
        private string _name;
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }
        #endregion

        #region Timestamp
        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetField(ref _timestamp, value);            
        }
        #endregion
        #region Child
        private Data _child;

        public Data Child
		{
			get => _child;
			set
			{
                if (SetField(ref _child, value, out var old)) return;

				if (old != null) old.PropertyChanged -= ChildPropertyChanged;
                if (value != null) value.PropertyChanged += ChildPropertyChanged;
			}
		}

        public int ChildPropertyChangedCount {  get; set; }
		private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ChildPropertyChangedCount++;
		}
		#endregion

	}
}
