using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryChoiceType : BaseEntryType
	{
		private const string LessThenMinElements = "Critical Error:\n{0} has less then {1} {2} elements.";
		private const string MoreThenMaxElements = "Critical Error:\n{0} has more then {1} {2} elements.";

		public EntryType[] Entries { get; protected set; }
		public int MinLength { get; protected set; }
		public int MaxLength { get; protected set; }

		public EntryChoiceType(WrathEdXML.AssetDefinition.EntryChoiceType entry)
			: base(entry)
		{
			Entries = new EntryType[entry.Entries.Length];
			for (int idx = 0; idx < Entries.Length; ++idx)
			{
				Entries[idx] = new EntryType(entry.Entries[idx] as WrathEdXML.AssetDefinition.EntryType);
			}
			MinLength = entry.MinLength;
			MaxLength = entry.MaxLength;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			List<XmlNode> nodes = new List<XmlNode>();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				foreach (EntryType entry in Entries)
				{
					if (childNode.Name == entry.id)
					{
						nodes.Add(childNode);
						break;
					}
				}
			}
			if (nodes.Count < MinLength)
			{
				ErrorDescription = string.Format(LessThenMinElements, trace, MinLength, id);
				return false;
			}
			if (MaxLength != 0 && nodes.Count > MaxLength)
			{
				ErrorDescription = string.Format(MoreThenMaxElements, trace, MaxLength, id);
				return false;
			}
			if (nodes.Count == 0)
			{
				if (MaxLength == 1)
				{
					position += 4;
				}
				else
				{
					position += 8;
				}
			}
			else
			{
				if (MaxLength == 1)
				{
					foreach (EntryType entry in Entries)
					{
						if (nodes[0].Name == entry.id)
						{
							foreach (BaseAssetType choiceBaseAsset in game.Assets.AssetTypes)
							{
								if (choiceBaseAsset.id == entry.AssetType)
								{
									int length = choiceBaseAsset.GetLength(game);
									BinaryAsset assetChoice = new BinaryAsset(length);
									FileHelper.SetUInt(StringHasher.Hash(entry.AssetType), position, asset.Content);
									asset.SubAssets.Add(-1, assetChoice);
									trace += '.' + id;
									AssetType choiceAsset = choiceBaseAsset as AssetType;
									int subPosition = 0;
									foreach (BaseEntryType relocationBaseEntry in choiceAsset.Entries)
									{
										if (!relocationBaseEntry.Compile(baseUri, assetChoice, nodes[0], game, trace, ref subPosition, out ErrorDescription))
										{
											return false;
										}
									}
									break;
								}
							}
							break;
						}
					}
					position += 4;
				}
				else
				{
					FileHelper.SetInt(nodes.Count, position, asset.Content);
					position += 4;
					if (nodes.Count != 0)
					{
						BinaryAsset choiceList = new BinaryAsset(nodes.Count * 4);
						asset.SubAssets.Add(position, choiceList);
						int listPosition = 0;
						foreach (XmlNode childNode in nodes)
						{
							foreach (EntryType entry in Entries)
							{
								if (childNode.Name == entry.id)
								{
									foreach (BaseAssetType choiceBaseAsset in game.Assets.AssetTypes)
									{
										if (choiceBaseAsset.id == entry.AssetType)
										{
											int length = choiceBaseAsset.GetLength(game);
											BinaryAsset assetChoice = new BinaryAsset(length + 4);
											FileHelper.SetUInt(StringHasher.Hash(entry.AssetType), 0, assetChoice.Content);
											choiceList.SubAssets.Add(listPosition, assetChoice);
											listPosition += 4;
											string choiceTrace = trace + '.' + id;
											AssetType choiceAsset = choiceBaseAsset as AssetType;
											int subPosition = 4;
											foreach (BaseEntryType relocationBaseEntry in choiceAsset.Entries)
											{
												if (!relocationBaseEntry.Compile(baseUri, assetChoice, childNode, game, choiceTrace, ref subPosition, out ErrorDescription))
												{
													return false;
												}
											}
											break;
										}
									}
									break;
								}
							}
						}
					}
					position += 4;
				}
			}
			ErrorDescription = string.Empty;
			return true;
		}
	}
}