using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Toolbox.Core.Test.Model
{
    [DebuggerDisplay("{Name} - {Timestamp}")]
    class Data : INotifyPropertyChanged
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
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region Timestamp
        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (value == _timestamp) return;
                _timestamp = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
