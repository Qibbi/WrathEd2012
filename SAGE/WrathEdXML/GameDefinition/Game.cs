using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SAGE.WrathEdXML.GameDefinition
{
	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:GameDefinition")]
	public enum ColorType
	{
		RED,
		GREEN,
		BLUE
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:GameDefinition")]
	public class RegistryEntryType
	{
		[XmlAttribute()]
		public string Key { get; set; }

		[XmlAttribute()]
		public string Value { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:GameDefinition")]
	public class StreamType
	{
		[XmlAttribute()]
		public string Name { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }

		[XmlAttribute()]
		public bool IsNameRequired { get; set; }

		public StreamType()
		{
			IsNameRequired = false;
		}
	}

	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:SAGE:WrathEdXML:GameDefinition")]
	[XmlRoot(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:GameDefinition", IsNullable = false)]
	public class Game
	{
		[XmlAttribute()]
		public string id { get; set; }

		[XmlAttribute()]
		public short ManifestVersion { get; set; }

		[XmlAttribute()]
		public uint AllTypesHash { get; set; }

		[XmlAttribute()]
		public string WorldBuilderVersion { get; set; }

		[XmlAttribute()]
		public ColorType ThemeColor { get; set; }

		[XmlElement()]
		public RegistryEntryType RegistryEntry { get; set; }

		[XmlElement()]
		public List<StreamType> Stream { get; set; }

		public static Game Load(string source)
		{
			XmlReader reader = XmlReader.Create(source);
			XmlSerializer serializer = new XmlSerializer(typeof(Game));
			Game game = serializer.Deserialize(reader) as Game;
			reader.Close();
			return game;
		}
	}
}
