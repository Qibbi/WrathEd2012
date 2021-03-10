using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WrathEd.WrathEdXML.Project
{
	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:WrathEd:Project")]
	[XmlRoot(Namespace = "uri:thundermods.net:WrathEd:Project", IsNullable = false)]
	public class Project
	{
		[XmlAttribute()]
		public string id { get; set; }

		[XmlAttribute()]
		public string GameDefinition { get; set; }

		public static Project Load(string source)
		{
			try
			{
				XmlReader reader = XmlReader.Create(source);
				XmlSerializer serializer = new XmlSerializer(typeof(Project));
				Project project = serializer.Deserialize(reader) as Project;
				reader.Close();
				return project;
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
			XmlWriter writer = XmlWriter.Create(string.Format(
				"{0}{1}{2}.WEProj",
				path,
				Path.DirectorySeparatorChar,
				id),
				writerSettings);
			XmlSerializer serializer = new XmlSerializer(typeof(Project));
			serializer.Serialize(writer, this);
			writer.Close();
		}
	}
}
