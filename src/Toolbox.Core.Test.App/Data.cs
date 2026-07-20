using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Toolbox.Core.Test.App
{
	internal class Data : INotifyPropertyChanged
	{
		private string name = "";

		public event PropertyChangedEventHandler? PropertyChanged;

		BindingList

		public string Name
		{
			get => name;
			set
			{
				if (name == value) return;
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}
	}
}
