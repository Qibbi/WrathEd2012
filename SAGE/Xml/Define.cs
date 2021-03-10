using System;
using System.Xml;
using System.Xml.Serialization;

namespace SAGE.Xml
{
	[Serializable()]
	[XmlType(Namespace = "uri:ea.com:eala:asset")]
	public class Define
	{
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "value")]
		public string Value { get; set; }

		[XmlAttribute(AttributeName = "override")]
		public bool Override { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:ea.com:eala:asset")]
	public class Defines
	{
		[XmlElement()]
		public Define[] Define { get; set; }
	}
}
