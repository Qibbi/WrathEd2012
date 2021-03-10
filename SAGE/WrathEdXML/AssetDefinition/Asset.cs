using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SAGE.WrathEdXML.AssetDefinition
{
	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public abstract class BaseAssetType
	{
		[XmlAttribute()]
		public string id { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EnumAssetType : BaseAssetType
	{
		[XmlElement()]
		public string[] Entry { get; set; }

		[XmlAttribute()]
		public bool IsUsingAll { get; set; }

		[XmlAttribute()]
		public bool IsStartingNegative { get; set; }

		public EnumAssetType()
		{
			IsUsingAll = false;
			IsStartingNegative = false;
		}
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class FlagsAssetType : BaseAssetType
	{
		[XmlAttribute()]
		public string BaseEnum { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public abstract class BaseEntryType
	{
		[XmlAttribute()]
		public string id { get; set; }

		[XmlAttribute()]
		public bool IsAttribute { get; set; }

		[XmlAttribute()]
		public bool IsRequired { get; set; }

		[XmlAttribute()]
		public bool IsVoid { get; set; }

		public BaseEntryType()
		{
			IsAttribute = false;
			IsRequired = false;
			IsVoid = false;
		}
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryPoidType : BaseEntryType
	{
		[XmlAttribute()]
		public string Description { get; set; }

		[XmlAttribute()]
		public bool IsUpperCase { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryFileType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryReferenceType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }

		[XmlAttribute()]
		public string Default { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryWeakReferenceType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }

		[XmlAttribute()]
		public string Default { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }

		[XmlAttribute()]
		public string Default { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryRelocationType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryListType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }

		[XmlAttribute()]
		public string Description { get; set; }

		[XmlAttribute()]
		public int MinLength { get; set; }

		[XmlAttribute()]
		public int MaxLength { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryChoiceType : BaseEntryType
	{
		[XmlElement(ElementName = "Entry", Type = typeof(EntryType))]
		public EntryType[] Entries { get; set; }

		[XmlAttribute()]
		public int MinLength { get; set; }

		[XmlAttribute()]
		public int MaxLength { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class EntryInheritanceType : BaseEntryType
	{
		[XmlAttribute()]
		public string AssetType { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class AssetType : BaseAssetType
	{
		[XmlElement(ElementName = "EntryPoid", Type = typeof(EntryPoidType))]
		[XmlElement(ElementName = "EntryFile", Type = typeof(EntryFileType))]
		[XmlElement(ElementName = "EntryReference", Type = typeof(EntryReferenceType))]
		[XmlElement(ElementName = "EntryWeakReference", Type = typeof(EntryWeakReferenceType))]
		[XmlElement(ElementName = "Entry", Type = typeof(EntryType))]
		[XmlElement(ElementName = "EntryRelocation", Type = typeof(EntryRelocationType))]
		[XmlElement(ElementName = "EntryList", Type = typeof(EntryListType))]
		[XmlElement(ElementName = "EntryChoice", Type = typeof(EntryChoiceType))]
		[XmlElement(ElementName = "EntryInheritance", Type = typeof(EntryInheritanceType))]
		public BaseEntryType[] Entries { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class RuntimeType
	{
		[XmlElement(ElementName = "EntryPoid", Type = typeof(EntryPoidType))]
		[XmlElement(ElementName = "EntryFile", Type = typeof(EntryFileType))]
		[XmlElement(ElementName = "EntryReference", Type = typeof(EntryReferenceType))]
		[XmlElement(ElementName = "EntryWeakReference", Type = typeof(EntryWeakReferenceType))]
		[XmlElement(ElementName = "Entry", Type = typeof(EntryType))]
		[XmlElement(ElementName = "EntryRelocation", Type = typeof(EntryRelocationType))]
		[XmlElement(ElementName = "EntryList", Type = typeof(EntryListType))]
		[XmlElement(ElementName = "EntryChoice", Type = typeof(EntryChoiceType))]
		[XmlElement(ElementName = "EntryInheritance", Type = typeof(EntryInheritanceType))]
		public BaseEntryType[] Entries { get; set; }
	}

	[Serializable()]
	[XmlType(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	public class GameAssetType : BaseAssetType
	{
		[XmlElement(ElementName = "EntryPoid", Type = typeof(EntryPoidType))]
		[XmlElement(ElementName = "EntryFile", Type = typeof(EntryFileType))]
		[XmlElement(ElementName = "EntryReference", Type = typeof(EntryReferenceType))]
		[XmlElement(ElementName = "EntryWeakReference", Type = typeof(EntryWeakReferenceType))]
		[XmlElement(ElementName = "Entry", Type = typeof(EntryType))]
		[XmlElement(ElementName = "EntryRelocation", Type = typeof(EntryRelocationType))]
		[XmlElement(ElementName = "EntryList", Type = typeof(EntryListType))]
		[XmlElement(ElementName = "EntryChoice", Type = typeof(EntryChoiceType))]
		[XmlElement(ElementName = "EntryInheritance", Type = typeof(EntryInheritanceType))]
		public BaseEntryType[] Entries { get; set; }

		[XmlElement()]
		public RuntimeType Runtime { get; set; }

		[XmlAttribute()]
		public uint TypeHash { get; set; }

		[XmlAttribute()]
		public bool IsTokenized { get; set; }

		[XmlAttribute()]
		public bool HasSpecialCompileHandling { get; set; }
	}

	[Serializable()]
	[XmlType(AnonymousType = false, Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition")]
	[XmlRoot(Namespace = "uri:thundermods.net:SAGE:WrathEdXML:AssetDefinition", IsNullable = false)]
	public class AssetDefinition
	{
		[XmlElement(ElementName = "Asset", Type = typeof(AssetType))]
		[XmlElement(ElementName = "EnumAsset", Type = typeof(EnumAssetType))]
		[XmlElement(ElementName = "FlagsAsset", Type = typeof(FlagsAssetType))]
		[XmlElement(ElementName = "GameAsset", Type = typeof(GameAssetType))]
		public BaseAssetType[] Assets { get; set; }

		public static AssetDefinition Load(string source)
		{
			try
			{
				XmlReader reader = XmlReader.Create(source);
				XmlSerializer serializer = new XmlSerializer(typeof(AssetDefinition));
				AssetDefinition assetDefinition = serializer.Deserialize(reader) as AssetDefinition;
				reader.Close();
				return assetDefinition;
			}
			catch
			{
				return null;
			}
		}
	}
}
