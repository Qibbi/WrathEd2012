using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WrathEd.WrathEdXML.Settings
{
	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:WrathEd:BinaryAssetBuilder")]
	public class LastProjectType
	{
		[XmlAttribute()]
		public string ID { get; set; }

		[XmlAttribute()]
		public string Path { get; set; }
	}

	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:WrathEd:BinaryAssetBuilder")]
	[XmlRoot(Namespace = "uri:thundermods.net:WrathEd:BinaryAssetBuilder", IsNullable = false)]
	public class Settings
	{
		[XmlElement()]
		public string GameDefinition { get; set; }

		[XmlElement()]
		public List<LastProjectType> LastProject { get; set; }

		public const string SettingsXml = "Settings.xml";

		public string GameDefinitionPath
		{
			get
			{
				return "Games" + Path.DirectorySeparatorChar + GameDefinition + ".xml";
			}
		}

		public static Settings CreateSettings(string defaultGame)
		{
			Settings settings = new Settings();
			settings.GameDefinition = defaultGame;
			settings.LastProject = new List<LastProjectType>();
			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Encoding = Encoding.UTF8;
			writerSettings.Indent = true;
			writerSettings.IndentChars = "\t";
			writerSettings.NewLineOnAttributes = true;
			XmlWriter writer = XmlWriter.Create(string.Format(
				"{0}{1}{2}",
				Globals.DocPathWrathEd,
				Path.DirectorySeparatorChar,
				SettingsXml),
				writerSettings);
			XmlSerializer serializer = new XmlSerializer(typeof(Settings));
			serializer.Serialize(writer, settings);
			writer.Close();
			return settings;
		}

		public static Settings LoadSettings()
		{
			XmlReader reader = XmlReader.Create(string.Format(
				"{0}{1}{2}",
				Globals.DocPathWrathEd,
				Path.DirectorySeparatorChar,
				SettingsXml));
			XmlSerializer serializer = new XmlSerializer(typeof(Settings));
			Settings settings = serializer.Deserialize(reader) as Settings;
			reader.Close();
			return settings;
		}

		public void SaveSettings()
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
				SettingsXml),
				writerSettings);
			XmlSerializer serializer = new XmlSerializer(typeof(Settings));
			serializer.Serialize(writer, this);
			writer.Close();
		}
	}
}
