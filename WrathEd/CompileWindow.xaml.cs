using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SAGE;
using SAGE.Compiler;
using SAGE.WrathEdXML.GameDefinition;

namespace WrathEd
{
	using Defines = SAGE.Compiler.Defines;
	using XmlDefines = SAGE.Xml.Defines;

	public partial class CompileWindow : Window
	{
		public CompileWindow()
		{
			uint achievementID = (uint)AchievementType.GETTING_STARTED;
			if (!Globals.Achievements[achievementID].IsAchieved)
			{
				Achievement.Unlock(Globals.Achievements[achievementID]);
			}
			InitializeComponent();
			ThreadPool.QueueUserWorkItem(Startup);
		}

		public void Startup(object empty)
		{
			uint achievementID = (uint)AchievementType.BUILDING_TIME;
			if (!Globals.Achievements[achievementID].IsAchieved)
			{
				UnlockAchievement(achievementID);
			}
			SetErrorDesc(string.Empty);
			SetCompileTitle("Loading Definitions");
			SetAssetName(string.Empty);
			Globals.Game = new GameDefinition(Game.Load(
				Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
				+ Path.DirectorySeparatorChar
				+ Globals.Settings.GameDefinitionPath),
				(string value) =>
				{
					SetFileName(value);
					return true;
				});
			SetGameName(Globals.Game.id);
			SetFileName(string.Empty);
			Uri compileUri = new Uri(Globals.Compile, UriKind.RelativeOrAbsolute);
			if (!compileUri.IsAbsoluteUri)
			{
				compileUri = new Uri(new Uri(Environment.CurrentDirectory + Path.DirectorySeparatorChar), compileUri);
				Globals.Compile = compileUri.LocalPath;
			}
			Uri outputUri = new Uri((Globals.Output == null) ? "" : Globals.Output, UriKind.RelativeOrAbsolute);
			if (!outputUri.IsAbsoluteUri)
			{
				outputUri = new Uri(new Uri(Environment.CurrentDirectory + Path.DirectorySeparatorChar), outputUri);
				Globals.Output = outputUri.LocalPath;
			}
			if (Macro.Root == null)
			{
				Macro.Root = Globals.Compile.Substring(0, Globals.Compile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
			}
			else
			{
				Uri rootUri = new Uri(Macro.Root, UriKind.RelativeOrAbsolute);
				if (!rootUri.IsAbsoluteUri)
				{
					rootUri = new Uri(new Uri(Environment.CurrentDirectory + Path.DirectorySeparatorChar), rootUri);
					Macro.Root = rootUri.LocalPath;
				}
			}
			Uri baseUri = new Uri(Macro.Root);
			Defines.Define.Clear();
			if (StreamCompiler.CompileStream(Globals.Game, compileUri, baseUri, outputUri, Globals.Version, Globals.BasePatchStream, Globals.IsMapCompile, Globals.LLod, Globals.MLod, SetCompileTitle, SetFileName, SetAssetName, SetBar, SetErrorDesc))
			{
				DispatchClose();
			}
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
		{
			DragMove();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs args)
		{
			Close();
		}

		public void DispatchClose()
		{
			Dispatcher.Invoke((Action)(() =>
			{
				Close();
			}));
		}

		public bool SetCompileTitle(string value)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				CompileTitle.Text = value;
			}));
			return true;
		}

		public bool SetCompileTitle(string value, int percentage)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				CompileTitle.Text = string.Format("{0}... {1:d02}%", value, percentage);
			}));
			return true;
		}

		public bool SetCompileTitle(string value, int percentage, bool isValue)
		{
			if (isValue)
			{
				SetCompileTitle(value, percentage);
			}
			else
			{
				SetCompileTitle(value);
			}
			return true;
		}

		public bool SetGameName(string value)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				GameName.Text = value;
			}));
			return true;
		}

		public bool SetFileName(string value)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				FileName.Text = value;
			}));
			return true;
		}

		public bool SetAssetName(string value)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				AssetName.Text = value;
			}));
			return true;
		}

		public bool SetBarToNoValue()
		{
			Dispatcher.Invoke((Action)(() =>
			{
				Progress.IsProgress = false;
			}));
			return true;
		}

		public bool SetBarToValue(double value)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				Progress.IsProgress = true;
				Progress.Value = value;
			}));
			return true;
		}

		public bool SetBar(double value, bool isValue)
		{
			if (isValue)
			{
				SetBarToValue(value);
			}
			else
			{
				SetBarToNoValue();
			}
			return true;
		}

		public bool SetErrorDesc(string value)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				ErrorDescription.Text = value;
			}));
			return true;
		}

		public bool UnlockAchievement(uint achievementHash)
		{
			Dispatcher.Invoke((Action)(() =>
			{
				Achievement.Unlock(Globals.Achievements[achievementHash]);
			}));
			return true;
		}
	}
}
