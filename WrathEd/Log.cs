using System;
using System.IO;
using System.Text;

namespace WrathEd
{
	/// <summary>
	/// Describes the log output type.
	/// </summary>
	public enum LogType
	{
		DEFAULT,
		CALL,
		WARNING,
		ERROR
	}

	/// <summary>
	/// Handles logging for WrathEd.
	/// </summary>
	public class Log : IDisposable
	{
		private StreamWriter _logWriter;
		private bool _isDisabled;


		/// <summary>
		/// Creates a disabled logger.
		/// </summary>
		public Log()
		{
			_isDisabled = true;
		}

		/// <summary>
		/// Creates a logger that until it disposes can write to the given stream.
		/// </summary>
		/// <param name="stream">The stream to log to.</param>
		public Log(FileStream stream)
		{
			_isDisabled = false;
			_logWriter = new StreamWriter(stream);
			_logWriter.AutoFlush = true;
			Write(LogType.CALL, Environment.CommandLine, false);
			WriteNewLine();
			WriteLineSeparator();
		}

		/// <summary>
		/// Writes a new line to the log.
		/// </summary>
		public void WriteNewLine()
		{
			if (_isDisabled)
			{
				return;
			}
			_logWriter.WriteLine();
		}

		/// <summary>
		/// Writes a separator line to the log.
		/// </summary>
		public void WriteLineSeparator()
		{
			if (_isDisabled)
			{
				return;
			}
			_logWriter.WriteLine("--------------------------------------------------------------------------------------------------------------------------------");
		}

		/// <summary>
		/// Writes messages to the log.
		/// </summary>
		/// <param name="type">The message type creating different output styles.</param>
		/// <param name="message">The message to write.</param>
		/// <param name="timestamp">Indicates if the line should contain a timestamp.</param>
		public void Write(LogType type, string message, bool timestamp = true)
		{
			if (_isDisabled)
			{
				return;
			}
			if (type == LogType.WARNING || type == LogType.ERROR)
			{
				_logWriter.WriteLine();
			}
			switch (type)
			{
				case LogType.DEFAULT:
					if (timestamp)
					{
						_logWriter.WriteLine(string.Format("[{0}] {1}", DateTime.Now.TimeOfDay, message));
					}
					else
					{
						_logWriter.WriteLine(message);
					}
					break;
				case LogType.CALL:
					if (timestamp)
					{
						_logWriter.WriteLine(string.Format("[{0}] {1}: {2}", DateTime.Now.TimeOfDay, Globals.CSF.GetLocalizedString(LocalizedStrings.LogCall), message));
					}
					else
					{
						_logWriter.WriteLine(string.Format("{0}: {1}", Globals.CSF.GetLocalizedString(LocalizedStrings.LogCall), message));
					}
					break;
				case LogType.WARNING:
					_logWriter.WriteLine(string.Format("[{0}] {1}: {2}", DateTime.Now.TimeOfDay, Globals.CSF.GetLocalizedString(LocalizedStrings.LogWarning), message));
					break;
				case LogType.ERROR:
					_logWriter.WriteLine(string.Format("[{0}] {1}: {2}", DateTime.Now.TimeOfDay, Globals.CSF.GetLocalizedString(LocalizedStrings.LogError), message));
					break;
			}
			if (type == LogType.WARNING || type == LogType.ERROR)
			{
				_logWriter.WriteLine();
			}
		}

		/// <summary>
		/// Closes the underlying writer.
		/// </summary>
		public void Dispose()
		{
			if (_isDisabled)
			{
				return;
			}
			_isDisabled = true;
			_logWriter.Close();
		}
	}
}
