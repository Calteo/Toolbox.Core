using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Collection.Generics;

namespace Toolbox.ComponentModel
{
	/// <summary>
	/// Command line to execute a command.
	/// </summary>
	public class CmdExcecutable
	{
		/// <summary>
		/// Intitalize a new instance of <see cref="CmdExcecutable"/>
		/// </summary>
		public CmdExcecutable(string excutable)
		{
			if (excutable.IsEmpty())
				throw new ArgumentNullException(nameof(excutable));
			if (!File.Exists(excutable))
				throw new FileNotFoundException("Executable not found", excutable);

			Executable = excutable;
		}

		/// <summary>
		/// Path to the executable 
		/// </summary>
		public string Executable { get; }
		/// <summary>
		/// Working directory to execute the command
		/// </summary>
		public string WorkingDirectory { get; set; } = "";
		/// <summary>
		/// Return code of the process
		/// </summary>
		public int ReturnCode { get; private set; } = -1;

		public event EventHandler<DataReceivedEventArgs>? OutputReceived;
		public event EventHandler<DataReceivedEventArgs>? ErrorReceived;

		public Task RunAsync(params string[] args)
		{
			return RunAsync((IEnumerable<string>)args);
		}

		public Task RunAsync(IEnumerable<string> args)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Executable,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = WorkingDirectory
				},
				EnableRaisingEvents = true
			};

			args.ForEach(process.StartInfo.ArgumentList.Add);

			process.Exited += ProcessExited;
			process.OutputDataReceived += ProcessOutputDataReceived;
			process.ErrorDataReceived += ProcessErrorDataReceived;

			if (!process.Start())
				throw new InvalidOperationException($"Failed to start process {Executable} {args}");

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			return process.WaitForExitAsync();
		}

		private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
		{			
			if (e.Data == null) return;

			if (OutputReceived != null)
				OutputReceived(sender, e);
			else
				_outputs.Add(e.Data);
		}

		private readonly List<string> _outputs = [];
		/// <summary>
		/// Get the list of outputs from the command line.
		/// </summary>
		public IReadOnlyList<string> Outputs => OutputReceived==null ? _outputs : throw new InvalidOperationException("Output captured by event");

		private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null) return;

			if (ErrorReceived != null)
				ErrorReceived(sender, e);
			else
				_errors.Add(e.Data);
		}

		private readonly List<string> _errors = [];
		/// <summary>
		/// Get the list of errors from the command line.
		/// </summary>
		public IReadOnlyList<string> Errors => ErrorReceived==null ? _errors : throw new InvalidOperationException("Erros captured by event");

		private void ProcessExited(object? sender, EventArgs e)
		{
			if (sender is Process process)
			{
				ReturnCode = process.ExitCode;

				process.Dispose();
			}
		}
	}
}

