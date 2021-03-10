using System;
using System.Windows;
using System.Windows.Input;

namespace WrathEd
{
	public partial class AboutBox : Window
	{
		private const string close = "UI:Close";
		private const string version = "UI:Version";
		private const string aboutDescription = "UI:AboutDescription";
		private const string aboutEA = "UI:AboutEA";

		public AboutBox()
		{
			InitializeComponent();
			CloseButton.Content = Globals.CSF.GetLocalizedString(close);
			Description.Text = Globals.CSF.GetLocalizedString(aboutDescription);
			Version.Text = string.Format(
				Globals.CSF.GetLocalizedString(version),
				Globals.CSF.GetLocalizedString(Globals.DevVersions[Globals.DevVersion]),
				Globals.MajorVersion, Globals.MinorVersion);
			EA.Text = Globals.CSF.GetLocalizedString(aboutEA);
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
		{
			DragMove();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs args)
		{
			Close();
		}
	}
}
