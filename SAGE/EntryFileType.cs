using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryFileType : BaseEntryType
	{
		public string AssetType { get; protected set; }
		public string Description { get; protected set; }

		public EntryFileType(WrathEdXML.AssetDefinition.EntryFileType entry)
			: base(entry)
		{
			AssetType = entry.AssetType;
			Description = entry.Description;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			ErrorDescription = string.Format("Critical Error: {0} not implemented!", typeof(EntryFileType).Name);
			return false;
		}
	}
}
