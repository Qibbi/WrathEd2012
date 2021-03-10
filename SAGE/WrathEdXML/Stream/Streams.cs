using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WrathEd.WrathEdXML.Stream
{
	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:Streams")]
	public abstract class Include
	{
		[XmlAttribute()]
		public string id { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:Streams")]
	public class Patch : Include
	{
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:Streams")]
	public class Reference : Include
	{
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:Streams")]
	public class Stream
	{
		[XmlAttribute()]
		public string id { get; set; }

		[XmlAttribute()]
		public string Version { get; set; }

		[XmlElement()]
		public Patch PatchedStream { get; set; }

		[XmlElement()]
		public List<Reference> ReferencedStream { get; set; }
	}

	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:SAGE:WrathEdXML:Streams")]
	[XmlRoot(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:Streams", IsNullable = false)]
	public class Streams
	{
		[XmlElement()]
		public List<Stream> Stream { get; set; }

		public static Streams Load(string source)
		{
			try
			{
				XmlReader reader = XmlReader.Create(source);
				XmlSerializer serializer = new XmlSerializer(typeof(Streams));
				Streams streams = serializer.Deserialize(reader) as Streams;
				reader.Close();
				return streams;
			}
			catch
			{
				return null;
			}
		}

		public void Save(string path)
		{
			XmlWriterSettings writerSettings = new XmlWriterSettings();
			writerSettings.Encoding = Encoding.UTF8;
			writerSettings.Indent = true;
			writerSettings.IndentChars = "\t";
			writerSettings.NewLineOnAttributes = true;
			XmlWriter writer = XmlWriter.Create(path, writerSettings);
			XmlSerializer serializer = new XmlSerializer(typeof(Streams));
			serializer.Serialize(writer, this);
			writer.Close();
		}
	}
}
