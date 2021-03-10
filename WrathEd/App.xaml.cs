using System;
using System.IO;
using System.Windows;
using SAGE.Compiler;

namespace WrathEd
{
	public partial class App : Application
	{
		private static Log _log;
		private static Uri _logPath;
		private const string errorMessage = "UI:ErrorMessage";
		private const string errorTitle = "UI:ErrorTitle";

		private const string GameDefinition = "-gameDefinition";
		private const string BigView = "-bigView";
		private const string Compile = "-compile";
		private const string Root = "-root";
		private const string Art = "-art";
		private const string Audio = "-audio";
		private const string Data = "-data";
		private const string BasePatchStream = "-basePatchStream";
		private const string BPS = "-bps";
		private const string Map = "-map";
		private const string Terrain = "-terrain";
		private const string Output = "-out";
		private const string Version = "-version";
		private const string LLod = "-lowLOD";
		private const string MLod = "-mediumLOD";

		private string gameDefinition = "Kane's Wrath";

		public App()
			: base()
		{
			AppDomain.CurrentDomain.UnhandledException += UnhandledException;
			string[] args = Environment.GetCommandLineArgs();
			//Uri currentDirectory = new Uri(args[0], UriKind.RelativeOrAbsolute);
			//Environment.CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			Globals.DocPathWrathEd = string.Format(
				"{0}{1}WrathEd",
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				Path.DirectorySeparatorChar);
			if (!Directory.Exists(Globals.DocPathWrathEd))
			{
				Directory.CreateDirectory(Globals.DocPathWrathEd);
			}
			StartupUri = new Uri("pack://application:,,,/BigView.xaml");
			bool isGameDefinitionCall = false;
			/* WrathEd.exe [-gameDefinition:string] [-bigView|[-compile:string [-map] [-terrain:string] [-out:string] [-version:string] [-lowLOD:string] [-mediumLOD:string]] [-root:string] [-art:string] [-audio:string] [-data:string] [-basePatchStream|bps:string,string] */
			int argIndex = 1;
			while (args.Length > argIndex)
			{
				if (args[argIndex].StartsWith(GameDefinition, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > GameDefinition.Length)
					{
						gameDefinition = args[argIndex].Substring(GameDefinition.Length + 1);
					}
					else
					{
						gameDefinition = args[++argIndex];
					}
					isGameDefinitionCall = true;
				}
				else if (args[argIndex].StartsWith(BigView, StringComparison.InvariantCultureIgnoreCase))
				{
					StartupUri = new Uri("pack://application:,,,/BigView.xaml");
				}
				else if (args[argIndex].StartsWith(Compile, StringComparison.InvariantCultureIgnoreCase))
				{
					StartupUri = new Uri("pack://application:,,,/CompileWindow.xaml");
					if (args[argIndex].Length > Compile.Length)
					{
						Globals.Compile = args[argIndex].Substring(Compile.Length + 1);
					}
					else
					{
						Globals.Compile = args[++argIndex];
					}
				}
				else if (args[argIndex].StartsWith(Map, StringComparison.InvariantCultureIgnoreCase))
				{
					Globals.IsMapCompile = true;
				}
				else if (args[argIndex].StartsWith(Terrain, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > Terrain.Length)
					{
						Macro.Terrain = args[argIndex].Substring(Terrain.Length + 1) + Path.DirectorySeparatorChar;
					}
					else
					{
						Macro.Terrain = args[++argIndex] + Path.DirectorySeparatorChar;
					}
				}
				else if (args[argIndex].StartsWith(Output, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > Output.Length)
					{
						Globals.Output = args[argIndex].Substring(Output.Length + 1) + Path.DirectorySeparatorChar;
					}
					else
					{
						Globals.Output = args[++argIndex] + Path.DirectorySeparatorChar;
					}
				}
				else if (args[argIndex].StartsWith(Version, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > Version.Length)
					{
						Globals.Version = args[argIndex].Substring(Version.Length + 1);
					}
					else
					{
						Globals.Version = args[++argIndex];
					}
				}
				else if (args[argIndex].StartsWith(LLod, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > LLod.Length)
					{
						Globals.LLod = args[argIndex].Substring(LLod.Length + 1);
					}
					else
					{
						Globals.LLod = args[++argIndex];
					}
				}
				else if (args[argIndex].StartsWith(MLod, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > MLod.Length)
					{
						Globals.MLod = args[argIndex].Substring(MLod.Length + 1);
					}
					else
					{
						Globals.MLod = args[++argIndex];
					}
				}
				else if (args[argIndex].StartsWith(Root, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > Root.Length)
					{
						Macro.Root = args[argIndex].Substring(Root.Length + 1) + Path.DirectorySeparatorChar;
					}
					else
					{
						Macro.Root = args[++argIndex] + Path.DirectorySeparatorChar;
					}
				}
				else if (args[argIndex].StartsWith(Art, StringComparison.InvariantCultureIgnoreCase))
				{
					Macro.Art.Clear();
					string[] source = null;
					if (args[argIndex].Length > Art.Length)
					{
						source = args[argIndex].Substring(Art.Length + 1).Split(';');
					}
					else
					{
						source = args[++argIndex].Split(';');
					}
					foreach (string macro in source)
					{
						Macro.Art.Add(macro);
					}
				}
				else if (args[argIndex].StartsWith(Audio, StringComparison.InvariantCultureIgnoreCase))
				{
					Macro.Audio.Clear();
					string[] source = null;
					if (args[argIndex].Length > Audio.Length)
					{
						source = args[argIndex].Substring(Audio.Length + 1).Split(';');
					}
					else
					{
						source = args[++argIndex].Split(';');
					}
					foreach (string macro in source)
					{
						Macro.Audio.Add(macro);
					}
				}
				else if (args[argIndex].StartsWith(Data, StringComparison.InvariantCultureIgnoreCase))
				{
					Macro.Data.Clear();
					string[] source = null;
					if (args[argIndex].Length > Data.Length)
					{
						source = args[argIndex].Substring(Data.Length + 1).Split(';');
					}
					else
					{
						source = args[++argIndex].Split(';');
					}
					foreach (string macro in source)
					{
						Macro.Data.Add(macro);
					}
				}
				else if (args[argIndex].StartsWith(BasePatchStream, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > BasePatchStream.Length)
					{
						Globals.BasePatchStream = args[argIndex].Substring(BasePatchStream.Length + 1);
					}
					else
					{
						Globals.BasePatchStream = args[++argIndex];
					}
				}
				else if (args[argIndex].StartsWith(BPS, StringComparison.InvariantCultureIgnoreCase))
				{
					if (args[argIndex].Length > BPS.Length)
					{
						Globals.BasePatchStream = args[argIndex].Substring(BPS.Length + 1);
					}
					else
					{
						Globals.BasePatchStream = args[++argIndex];
					}
				}
				++argIndex;
			}
			if (System.IO.File.Exists(string.Format(
				"{0}{1}{2}",
				Globals.DocPathWrathEd,
				Path.DirectorySeparatorChar,
				WrathEdXML.Settings.Settings.SettingsXml)))
			{
				Globals.Settings = WrathEdXML.Settings.Settings.LoadSettings();
				if (isGameDefinitionCall)
				{
					Globals.Settings.GameDefinition = gameDefinition;
				}
			}
			else
			{
				Globals.Settings = WrathEdXML.Settings.Settings.CreateSettings(gameDefinition);
			}
			Globals.CreateAndLoadAchievements();

			Uri UserDirectory = new Uri(Globals.DocPathWrathEd + Path.DirectorySeparatorChar, UriKind.RelativeOrAbsolute);
			string dateTimeNow = string.Format(
				"{0:D4}{1:D2}{2:D2}_{3:D2}-{4:D2}-{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
			_logPath = new Uri(UserDirectory, string.Format(".\\Logs\\WrathEd_{0}.txt", dateTimeNow));
			string logDirectory = Path.GetDirectoryName(_logPath.AbsolutePath).Replace("%20", " ");
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}
			string[] oldLogs = Directory.GetFiles(logDirectory);
			if (oldLogs.Length > 9)
			{
				File.Delete(oldLogs[0]);
			}
			FileStream logStream = null;
			try
			{
				logStream = File.Create(_logPath.AbsolutePath.Replace("%20", " "));
			}
			catch { }
			if (logStream == null)
			{
				_log = new Log();
			}
			else
			{
				_log = new Log(logStream);
			}
			App.Current.Exit += new ExitEventHandler(AppExit);
		}

		private void UnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			Exception exception = (args.ExceptionObject as Exception);
			while (exception.InnerException != null)
			{
				exception = exception.InnerException;
			}
			_log.Write(LogType.ERROR, exception.Message + "\r\n" + exception.StackTrace);
			if (MessageBox.Show(string.Format(Globals.CSF.GetLocalizedString(LocalizedStrings.ErrorMessage), exception.Message), Globals.CSF.GetLocalizedString(LocalizedStrings.ErrorTitle),
				MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
			{
#if !DEBUG
				Environment.Exit(-1);
#endif
			}
		}

		/// <summary>
		/// Fired when the application exits.
		/// Disposes the log.
		/// </summary>
		/// <param name="sender">Unused.</param>
		/// <param name="args">Unused.</param>
		private void AppExit(object sender, ExitEventArgs args)
		{
			_log.Write(LogType.DEFAULT, Globals.CSF.GetLocalizedString(LocalizedStrings.LogExit));
			_log.Dispose();
		}
	}
}
