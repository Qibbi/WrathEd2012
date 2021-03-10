using System;
using System.Xml;
using System.Xml.Serialization;

namespace WrathEd.WrathEdXML.Mod
{
	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:WrathEd:Mod")]
	[XmlRoot(Namespace = "uri:thundermods.net:WrathEd:Mod", IsNullable = false)]
	public class ModDescription
	{
		[XmlAttribute()]
		public string id { get; set; }

		[XmlAttribute()]
		public string Name { get; set; }

		[XmlAttribute()]
		public string Version { get; set; }

		[XmlAttribute()]
		public string LogoImage { get; set; }

		[XmlAttribute()]
		public string NeededLanguage { get; set; }
	}
}