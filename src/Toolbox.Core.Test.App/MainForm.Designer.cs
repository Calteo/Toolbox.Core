namespace Toolbox.Core.Test.App
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			dataGridView1 = new DataGridView();
			ColumnName = new DataGridViewTextBoxColumn();
			labelCount = new Label();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			SuspendLayout();
			// 
			// dataGridView1
			// 
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { ColumnName });
			dataGridView1.Location = new Point(38, 36);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersWidth = 51;
			dataGridView1.Size = new Size(663, 332);
			dataGridView1.TabIndex = 0;
			dataGridView1.UserDeletedRow += dataGridView1_UserDeletedRow;
			dataGridView1.UserDeletingRow += dataGridView1_UserDeletingRow;
			// 
			// ColumnName
			// 
			ColumnName.DataPropertyName = "Name";
			ColumnName.HeaderText = "Name";
			ColumnName.MinimumWidth = 6;
			ColumnName.Name = "ColumnName";
			ColumnName.Width = 125;
			// 
			// labelCount
			// 
			labelCount.AutoSize = true;
			labelCount.Location = new Point(758, 62);
			labelCount.Name = "labelCount";
			labelCount.Size = new Size(50, 20);
			labelCount.TabIndex = 1;
			labelCount.Text = "label1";
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1124, 538);
			Controls.Add(labelCount);
			Controls.Add(dataGridView1);
			Name = "MainForm";
			Text = "Form1";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DataGridView dataGridView1;
		private DataGridViewTextBoxColumn ColumnName;
		private Label labelCount;
	}
}
