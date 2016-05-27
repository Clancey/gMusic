using System;
using Java.Lang;


namespace FFMpeg
{
	public class CommandResult
	{
		public string Output {get;private set;}
		public bool Success {get;private set;}

		public CommandResult(bool success, string output) {
			this.Success = success;
			this.Output = output;
		}

		public static CommandResult getDummyFailureResponse() {
			return new CommandResult(false, "");
		}

		public static CommandResult GetOutputFromProcess(Process process) {
			string output;
			if (IsSuccess(process.ExitValue())) {
				output = Util.ConvertInputStreamToString(process.InputStream);
			} else {
				output = Util.ConvertInputStreamToString(process.ErrorStream);
			}
			return new CommandResult(IsSuccess(process.ExitValue()), output);
		}

		public static bool IsSuccess(int exitValue) {
			return exitValue != -1 && exitValue == 0;
		}

	}
	class Util
	{
		public static string ConvertInputStreamToString(System.IO.Stream stream)
		{
			using(var reader = new System.IO.StreamReader(stream)){
				string text = reader.ReadToEnd ();
				return text;
			}
		}
		public static void destroyProcess(Process process) {
			if (process != null)
				process.Destroy();
		}
	}
	public class ShellCommand
	{

		public Process Run (string[] commandStrings)
		{
			Process process = null;
			try {
				process = Runtime.GetRuntime ().Exec (commandStrings);
			} catch (System.Exception e) {
				System.Console.WriteLine ("Exception while trying to run: " + string.Join(" " , commandStrings), e);
			}
			return process;
		}
		public CommandResult RunWaitFor (string s)
		{
			return RunWaitFor (new []{ s });
		}
		public CommandResult RunWaitFor (string[] s)
		{
			Process process = Run (s);

			int exitValue = -1;
			string output = null;
			try {
				if (process != null) {
					exitValue = process.WaitFor ();

					if (CommandResult.IsSuccess (exitValue)) {
						output = Util.ConvertInputStreamToString (process.InputStream);
					} else {
						output = Util.ConvertInputStreamToString (process.ErrorStream);
					}
				}
			} catch (InterruptedException e) {
				System.Console.WriteLine ("Interrupt exception", e);
			} finally {
				Util.destroyProcess (process);
			}

			return new CommandResult (CommandResult.IsSuccess (exitValue), output);
		}

	}
}