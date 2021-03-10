using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryPoidType : BaseEntryType
	{
		public string Description { get; protected set; }
		public bool IsUpperCase { get; protected set; }

		public EntryPoidType(WrathEdXML.AssetDefinition.EntryPoidType entry)
			: base(entry)
		{
			Description = entry.Description;
			IsUpperCase = entry.IsUpperCase;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			bool isFound = false;
			if (IsAttribute)
			{
				foreach (XmlAttribute attribute in node.Attributes)
				{
					if (attribute.Name == id)
					{
						isFound = true;
						FileHelper.SetUInt(StringHasher.Hash(((IsUpperCase) ? attribute.Value : attribute.Value.ToLowerInvariant())), position, asset.Content);
						break;
					}
				}
			}
			else
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (childNode.Name == id)
					{
						isFound = true;
						FileHelper.SetUInt(StringHasher.Hash(((IsUpperCase) ? childNode.InnerXml : childNode.InnerXml.ToLowerInvariant())), position, asset.Content);
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
			position += 4;
			ErrorDescription = string.Empty;
			return true;
		}
	}
}
