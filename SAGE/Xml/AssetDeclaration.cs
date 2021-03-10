using System;
using System.Xml;
using System.Xml.Serialization;

namespace SAGE.Xml
{
	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:ea.com:eala:asset")]
	[XmlRoot(Namespace = "uri:ea.com:eala:asset", IsNullable = false)]
	public class AssetDeclaration
	{
		[XmlElement()]
		public Includes Includes { get; set; }
		
		[XmlElement()]
		public Defines Defines { get; set; }
		
		[XmlAnyElement()]
		public XmlElement[] Elements { get; set; }

		public string Source { get; set; }

		public static AssetDeclaration Load(string source, string sourceMacro)
		{
			try
			{
				XmlReader reader = XmlReader.Create(source);
				XmlSerializer serializer = new XmlSerializer(typeof(AssetDeclaration));
				AssetDeclaration sageXml = serializer.Deserialize(reader) as AssetDeclaration;
				reader.Close();
				sageXml.Source = sourceMacro;
				return sageXml;
			}
			catch(Exception ex)
			{
				return null;
			}
		}
	}
}
