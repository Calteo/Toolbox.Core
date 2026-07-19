using System.ComponentModel;
using System.Diagnostics;
using Toolbox.ComponentModel;

namespace Toolbox.Core.Test.App
{
	internal partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.DataSource = Datas;

			Datas.ListChanged += Datas_ListChanged;
		}

		private void Datas_ListChanged(object? sender, ListChangedEventArgs e)
		{ 
			labelCount.Text = $"Count  = {Datas.Count}";
		}

		private BindableList<Data> Datas { get; } = [];

		private void Form1_Load(object sender, EventArgs e)
		{
		}

		private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			Trace.WriteLine($"{e.Row?.Cells[0].Value}", "Deleting");
		}

		private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
		{
			Trace.WriteLine($"{e.Row?.Cells[0].Value}", "Deleted");
		}
	}
}
