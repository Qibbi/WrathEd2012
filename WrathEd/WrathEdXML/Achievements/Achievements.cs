using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WrathEd.WrathEdXML.Achievements
{
	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:WrathEd:Achievements")]
	public class Achievement
	{
		[XmlAttribute()]
		public uint ID { get; set; }

		[XmlAttribute()]
		public bool IsAchieved { get; set; }

		[XmlAttribute()]
		public uint Progress { get; set; }
	}

	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:WrathEd:Achievements")]
	[XmlRoot(Namespace = "uri:thundermods.net:WrathEd:Achievements", IsNullable = false)]
	public class Achievements
	{
		[XmlElement()]
		public List<Achievement> Achievement { get; set; }

		private const string AchievementsXml = "Achievements.xml";

		public Achievements()
		{
			Achievement = new List<Achievement>();
		}

		public static Achievements Load()
		{
			try
			{
				XmlReader reader = XmlReader.Create(string.Format(
					"{0}{1}{2}",
					Globals.DocPathWrathEd,
					Path.DirectorySeparatorChar,
					AchievementsXml));
				XmlSerializer serializer = new XmlSerializer(typeof(Achievements));
				Achievements achievements = serializer.Deserialize(reader) as Achievements;
				reader.Close();
				return achievements;
			}
			catch
			{
				return null;
			}
		}

		public void Save()
		{
			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Encoding = Encoding.UTF8;
			writerSettings.Indent = true;
			writerSettings.IndentChars = "\t";
			writerSettings.NewLineOnAttributes = true;
			XmlWriter writer = XmlWriter.Create(string.Format(
				"{0}{1}{2}",
				Globals.DocPathWrathEd,
				Path.DirectorySeparatorChar,
				AchievementsXml),
				writerSettings);
			XmlSerializer serializer = new XmlSerializer(typeof(Achievements));
			serializer.Serialize(writer, this);
			writer.Close();
		}
	}
}
