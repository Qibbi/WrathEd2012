using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryReferenceType : BaseEntryType
	{
		public string AssetType { get; protected set; }
		public string Default { get; protected set; }
		public string Description { get; protected set; }

		public EntryReferenceType(WrathEdXML.AssetDefinition.EntryReferenceType entry)
			: base(entry)
		{
			AssetType = entry.AssetType;
			Default = entry.Default;
			Description = entry.Description;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			bool isFound = false;
			string value = string.Empty;
			if (IsAttribute)
			{
				if (value == string.Empty && Default != null)
				{
					value = Default;
				}
				foreach (XmlAttribute attribute in node.Attributes)
				{
					if (attribute.Name == id)
					{
						isFound = true;
						value = attribute.Value;
						break;
					}
				}
			}
			else
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (childNode.Name == "#text")
					{
						isFound = true;
						value = childNode.InnerText;
						break;
					}
					else if (childNode.Name == id)
					{
						isFound = true;
						value = childNode.InnerXml;
						break;
					}
				}
			}
			if (!isFound && IsRequired)
			{
				if (IsAttribute)
				{
					ErrorDescription = string.Format(RequiredNotFoundAttribute, id, trace);
				}
				else
				{
					ErrorDescription = string.Format(RequiredNotFoundElement, id, trace);
				}
				return false;
			}
			if (value != string.Empty)
			{
				string[] name = value.Split(':');
				if (name.Length == 1)
				{
					asset.AssetImports.Add(position, new uint[] { StringHasher.Hash(AssetType), StringHasher.Hash(name[0].ToLowerInvariant()) });
				}
				else
				{
					asset.AssetImports.Add(position, new uint[] { StringHasher.Hash(name[0]), StringHasher.Hash(name[1].ToLowerInvariant()) });
				}
			}
			position += 4;
			ErrorDescription = string.Empty;
			return true;
		}
	}
}