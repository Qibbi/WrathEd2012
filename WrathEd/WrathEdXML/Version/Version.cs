using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WrathEd.WrathEdXML.Version
{
	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:WrathEd:BinaryAssetBuilder:Version")]
	public class Version
	{
		[XmlAttribute()]
		public int Major { get; set; }

		[XmlAttribute()]
		public int Minor { get; set; }
	}

	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:WrathEd:BinaryAssetBuilder:Version")]
	[XmlRoot(Namespace = "uri:thundermods.net:WrathEd:BinaryAssetBuilder:Version", IsNullable = false)]
	public class BinaryAssetBuilder
	{
		[XmlElement()]
		public Version Version { get; set; }

		public static BinaryAssetBuilder Load(string source)
		{
			XmlReader reader = XmlReader.Create(source);
			XmlSerializer serializer = new XmlSerializer(typeof(BinaryAssetBuilder));
			BinaryAssetBuilder bab = serializer.Deserialize(reader) as BinaryAssetBuilder;
			reader.Close();
			return bab;
		}
	}
}
