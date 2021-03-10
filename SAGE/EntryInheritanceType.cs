using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryInheritanceType : BaseEntryType
	{
		public string AssetType { get; protected set; }

		public EntryInheritanceType(WrathEdXML.AssetDefinition.EntryInheritanceType entry)
			: base(entry)
		{
			AssetType = entry.AssetType;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			foreach (GameAssetType gameAssetType in game.Assets.GameAssetTypes)
			{
				if (gameAssetType.id == AssetType)
				{
					if (gameAssetType.Entries != null)
					{
						foreach (BaseEntryType baseEntryType in gameAssetType.Entries)
						{
							if (!baseEntryType.Compile(baseUri, asset, node, game, trace, ref position, out ErrorDescription))
							{
								return false;
							}
						}
					}
					ErrorDescription = string.Empty;
					return true;
				}
			}
			foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
			{
				if (baseAssetType.id == AssetType)
				{
					AssetType baseAsset = baseAssetType as AssetType;
					if (baseAsset.Entries != null)
					{
						foreach (BaseEntryType baseEntryType in baseAsset.Entries)
						{
							if (!baseEntryType.Compile(baseUri, asset, node, game, trace, ref position, out ErrorDescription))
							{
								return false;
							}
						}
					}
					ErrorDescription = string.Empty;
					return true;
				}
			}
			ErrorDescription = string.Empty;
			return true;
		}
	}
}
