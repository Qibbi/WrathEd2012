using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using SAGE;
using SAGE.CSF;
using WrathEd.WrathEdXML.Settings;

namespace WrathEd
{
	public enum AchievementType
	{
		GETTING_STARTED,
		BUILDING_TIME,
		CODE_SPY,
		LITTLE_BIGS,
		SHOW_EVERYTHING,
		MANY_THINGS
	}

	public static class Globals
	{
		public const string ExplorerExe = "explorer.exe";
		public const string CSFPath = "Localization\\WrathEd.csf";

		public const int MajorVersion = 1;
		public const int MinorVersion = 7;
		public const int DevVersion = 1;

		public static string[] DevVersions;

		public static Settings Settings { get; set; }

		public static string DocPathWrathEd { get; set; }

		public static string Compile { get; set; }
		public static string BasePatchStream { get; set; }
		public static bool IsMapCompile { get; set; }
		public static string Output { get; set; }
		public static string Version { get; set; }
		public static string LLod { get; set; }
		public static string MLod { get; set; }

		public static GameDefinition Game { get; set; }

		public static bool IsVisibleAchievements = false;
		public static Dictionary<uint, Achievement> Achievements { get; set; }

		public static CSF CSF;

		static Globals()
		{
			CSF = new CSF(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
				+ Path.DirectorySeparatorChar
				+ CSFPath);
			DevVersions = new string[] { "UI:VersionAlpha", "UI:VersionBeta", "UI:VersionGold" };
		}

		public static void CreateAndLoadAchievements()
		{
			Achievements = new Dictionary<uint, Achievement>();

			Achievement achievementGS = new Achievement(
				AchievementType.GETTING_STARTED,
				CSF.GetLocalizedString("ACHIEVEMENT:GettingStartedName"),
				CSF.GetLocalizedString("ACHIEVEMENT:GettingStartedDesc"),
				"gettingstarted");
			Achievements.Add(achievementGS.ID, achievementGS);

			Achievement achievementCompile10 = new Achievement(
				AchievementType.BUILDING_TIME,
				CSF.GetLocalizedString("ACHIEVEMENT:BuildingTimeName"),
				CSF.GetLocalizedString("ACHIEVEMENT:BuildingTimeDesc"),
				"buildintime");
			achievementCompile10.AddProgress(
				() =>
				{
					if (++achievementCompile10.Progress >= 10)
					{
						return true;
					}
					return false;
				},
				() =>
				{
					return Math.Min(achievementCompile10.Progress / 10.0d * 100, 100);
				},
				() =>
				{
					return achievementCompile10.Progress + " / 10";
				});
			Achievements.Add(achievementCompile10.ID, achievementCompile10);

			Achievement achievementBigView50 = new Achievement(
				AchievementType.CODE_SPY,
				CSF.GetLocalizedString("ACHIEVEMENT:CodeSpyName"),
				CSF.GetLocalizedString("ACHIEVEMENT:CodeSpyDesc"),
				"gettingstarted");
			achievementBigView50.AddProgress(
				() =>
				{
					if (++achievementBigView50.Progress >= 50)
					{
						return true;
					}
					return false;
				},
				() =>
				{
					return Math.Min(achievementBigView50.Progress / 50.0d * 100, 100);
				},
				() =>
				{
					return achievementBigView50.Progress + " / 50";
				});
			Achievements.Add(achievementBigView50.ID, achievementBigView50);

			Achievement achievementBigs10 = new Achievement(
				AchievementType.LITTLE_BIGS,
				CSF.GetLocalizedString("ACHIEVEMENT:LittleBigsName"),
				CSF.GetLocalizedString("ACHIEVEMENT:LittleBigsDesc"),
				"gettingstarted");
			achievementBigs10.AddProgress(
				() =>
				{
					if (++achievementBigs10.Progress >= 10)
					{
						return true;
					}
					return false;
				},
				() =>
				{
					return Math.Min(achievementBigs10.Progress / 10.0d * 100, 100);
				},
				() =>
				{
					return achievementBigs10.Progress + " / 10";
				});
			Achievements.Add(achievementBigs10.ID, achievementBigs10);

			Achievement achievementSkuDefs10 = new Achievement(
				AchievementType.SHOW_EVERYTHING,
				CSF.GetLocalizedString("ACHIEVEMENT:ShowEverythingName"),
				CSF.GetLocalizedString("ACHIEVEMENT:ShowEverythingDesc"),
				"gettingstarted");
			achievementSkuDefs10.AddProgress(
				() =>
				{
					if (++achievementSkuDefs10.Progress >= 10)
					{
						return true;
					}
					return false;
				},
				() =>
				{
					return Math.Min(achievementSkuDefs10.Progress / 10.0d * 100, 100);
				},
				() =>
				{
					return achievementSkuDefs10.Progress + " / 10";
				});
			Achievements.Add(achievementSkuDefs10.ID, achievementSkuDefs10);

			Achievement achievementExtract10 = new Achievement(
				AchievementType.MANY_THINGS,
				CSF.GetLocalizedString("ACHIEVEMENT:ManyThingsName"),
				CSF.GetLocalizedString("ACHIEVEMENT:ManyThingsDesc"),
				"gettingstarted");
			achievementExtract10.AddProgress(
				() =>
				{
					if (++achievementExtract10.Progress >= 10)
					{
						return true;
					}
					return false;
				},
				() =>
				{
					return Math.Min(achievementExtract10.Progress / 10.0d * 100, 100);
				},
				() =>
				{
					return achievementExtract10.Progress + " / 10";
				});
			Achievements.Add(achievementExtract10.ID, achievementExtract10);

			WrathEdXML.Achievements.Achievements achievemnts = WrathEdXML.Achievements.Achievements.Load();
			if (achievemnts != null)
			{
				for (int idx = 0; idx < achievemnts.Achievement.Count; ++idx)
				{
					Achievements[achievemnts.Achievement[idx].ID].Progress = achievemnts.Achievement[idx].Progress;
					Achievements[achievemnts.Achievement[idx].ID].IsAchieved = achievemnts.Achievement[idx].IsAchieved;
				}
			}
		}
	}
}