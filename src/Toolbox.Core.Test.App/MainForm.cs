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

			Datas.ListChanged += DatasListChanged;
		}

		private void DatasListChanged(object? sender, ListChangedEventArgs e)
		{
			textBoxTrace.Text += $"ListChanged: {e.ListChangedType} {e.OldIndex}->{e.NewIndex}" + Environment.NewLine;
			labelCount.Text = $"Count  = {Datas.Count}";
		}

		private BindableList<Data> Datas { get; } = [];
		// private BindingList<Data> Datas { get; } = [];

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
