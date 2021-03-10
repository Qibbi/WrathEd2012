using System;
using System.Timers;
using System.Windows;

namespace WrathEd
{
	public partial class AchievementBox : Window
	{
		Timer Timer;
		Achievement Achievement { get; set; }

		public AchievementBox(Achievement achievement)
		{
			Achievement = achievement;
			InitializeComponent();
			AchievementTitle.Text = Globals.CSF.GetLocalizedString("ACHIEVEMENT:Unlocked");
			AchievementImage.Source = achievement.Image;
			AchievementName.Text = achievement.Name;
			AchievementDesc.Text = achievement.Description;
			Timer = new Timer();
			Timer.Elapsed += CloseWindow;
			Timer.Interval = 10000;
			Timer.Enabled = true;
		}

		private void CloseWindow(object source, EventArgs args)
		{
			Timer.Enabled = false;
			Timer.Close();
			this.Dispatcher.Invoke((Action)(() =>
			{
				Close();
			}));
		}

		private void Window_MouseUp(object sender, RoutedEventArgs args)
		{
			Timer.Enabled = false;
			Timer.Close();
			Close();
		}
	}
}
