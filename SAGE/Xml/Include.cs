using System;
using System.Xml;
using System.Xml.Serialization;

namespace SAGE.Xml
{
	public enum IncludeType
	{
		REFERENCE = 0x00,
		reference = 0x00,
		INSTANCE = 0x01,
		instance = 0x01,
		ALL = 0x02,
		all = 0x02
	}

	[Serializable()]
	[XmlType(Namespace = "uri:ea.com:eala:asset")]
	public class Include
	{
		[XmlAttribute(AttributeName = "type")]
		public IncludeType Type { get; set; }

		[XmlAttribute(AttributeName = "source")]
		public string Source { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:ea.com:eala:asset")]
	public class Includes
	{
		[XmlElement()]
		public Include[] Include { get; set; }
	}
}
