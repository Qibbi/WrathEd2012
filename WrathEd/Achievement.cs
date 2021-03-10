using System;
using System.Windows.Media.Imaging;
using SAGE;
using WrathEd.WrathEdXML.Achievements;

namespace WrathEd
{
	public class Achievement
	{
		public uint ID { get; private set; }

		public string Name { get; private set; }
		public string Description { get; private set; }
		public BitmapImage Image { get; private set; }

		public Func<bool> ProgressCheck;
		public Func<double> ProgressStatus;
		public double ProgressPerc { get { return ProgressStatus(); } set { } }
		public Func<string> ProgressToolTipBuilder;
		public string ProgressToolTip { get { return ProgressToolTipBuilder(); } set { } }
		public uint Progress { get; set; }
		public bool IsNotProgressive { get; set; }
		public bool IsAchieved { get; set; }

		public Achievement(AchievementType id, string name, string description, string imageName)
		{
			ID = (uint)id;
			Name = name;
			Description = description;
			Uri imageUri = new Uri("pack://application:,,,/WrathEd;component/Art/Textures/Achievements/" + imageName + ".png");
			try
			{
				Image = new BitmapImage(imageUri);
			}
			catch
			{
			}
			Progress = 0;
			IsNotProgressive = true;
			ProgressCheck = () =>
				{
					return true;
				};
			ProgressStatus = () =>
				{
					if (IsAchieved)
					{
						return 100.0d;
					}
					return 0.0d;
				};
			IsAchieved = false;
			ProgressToolTipBuilder = () =>
				{
					if (IsAchieved)
					{
						return "Completed";
					}
					return "Not Completed";
				};
		}

		public void AddProgress(Func<bool> progressCheck, Func<double> progressStatus, Func<string> progressToolTipBuilder)
		{
			IsNotProgressive = false;
			ProgressCheck = progressCheck;
			ProgressStatus = progressStatus;
			IsAchieved = false;
			ProgressToolTipBuilder = progressToolTipBuilder;
		}

		public static void Unlock(Achievement achievement)
		{
			if (achievement.IsNotProgressive || achievement.ProgressCheck())
			{
				achievement.IsAchieved = true;
			}
			Achievements achievements = Achievements.Load();
			if (achievements == null)
			{
				achievements = new Achievements();
			}
			WrathEdXML.Achievements.Achievement xmlAchievement;
			bool isNotFound = true;
			for (int idx = 0; idx < achievements.Achievement.Count; ++idx)
			{
				if (achievements.Achievement[idx].ID == achievement.ID)
				{
					isNotFound = false;
					xmlAchievement = achievements.Achievement[idx];
					xmlAchievement.Progress = achievement.Progress;
					xmlAchievement.IsAchieved = achievement.IsAchieved;
					break;
				}
			}
			if (isNotFound)
			{
				xmlAchievement = new WrathEdXML.Achievements.Achievement();
				xmlAchievement.ID = achievement.ID;
				xmlAchievement.Progress = achievement.Progress;
				xmlAchievement.IsAchieved = achievement.IsAchieved;
				achievements.Achievement.Add(xmlAchievement);
			}
			achievements.Save();
			if (achievement.IsAchieved)
			{
				AchievementBox achievementBox = new AchievementBox(achievement);
				achievementBox.Show();
			}
		}
	}
}
