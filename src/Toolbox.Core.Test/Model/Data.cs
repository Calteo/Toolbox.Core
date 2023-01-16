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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string properyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
        #endregion

    }
}
