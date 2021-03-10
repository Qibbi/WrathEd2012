using System;
using System.Windows;

namespace WrathEd
{
	public partial class DemoError : Window
	{
		public DemoError()
		{
			InitializeComponent();
			Description.Text = "DEMO VERSION!";
			EX.Text = "You have to run WrathEd with the -compile switch.\nExample: WrathEd.exe -compile:\"MyMapPath/map.xml\"";
		}

		private void CloseButton_Click(object sender, RoutedEventArgs args)
		{
			Close();
		}
	}
}
