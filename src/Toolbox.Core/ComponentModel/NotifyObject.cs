using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Toolbox.ComponentModel
{
	public class NotifyObject : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging
		public event PropertyChangingEventHandler PropertyChanging;
		protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
			=> PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
		#endregion
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		#endregion
		#region SetField
		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
				=> SetField(ref field, value, out var _, propertyName);

		protected bool SetField<T>(ref T field, T value, out T oldValue, [CallerMemberName] string propertyName = "")
		{
			oldValue = field;
			if (Equals(field, value)) return true;

			OnPropertyChanging(propertyName);
			field = value;
			OnPropertyChanged(propertyName);

			return false;
		}
		#endregion
	}
}
