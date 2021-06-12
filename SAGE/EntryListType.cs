using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryListType : BaseEntryType
	{
		private const string LessThenMinElements = "Critical Error:\n{0} has less then {1} {2} elements.";
		private const string MoreThenMaxElements = "Critical Error:\n{0} has more then {1} {2} elements.";

		public string AssetType { get; protected set; }
		public string Description { get; protected set; }
		public int MinLength { get; protected set; }
		public int MaxLength { get; protected set; }

		public EntryListType(WrathEdXML.AssetDefinition.EntryListType entry)
			: base(entry)
		{
			AssetType = entry.AssetType;
			Description = entry.Description;
			MinLength = entry.MinLength;
			MaxLength = entry.MaxLength;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			if (IsAttribute)
			{
				bool isFound = false;
				string[] values = null;
				foreach (XmlAttribute attribute in node.Attributes)
				{
					if (attribute.Name == id)
					{
						isFound = true;
						values = attribute.Value.Split(' ');
						break;
					}
				}
				if (isFound)
				{
					if (values.Length < MinLength)
					{
						ErrorDescription = string.Format(LessThenMinElements, trace, MinLength, id);
						return false;
					}
					if (MaxLength != 0 && values.Length > MaxLength)
					{
						ErrorDescription = string.Format(MoreThenMaxElements, trace, MaxLength, id);
						return false;
					}
					if (values.Length != 0)
					{
						FileHelper.SetInt(values.Length, position, asset.Content);
						position += 4;
						trace += '.' + id;
						BinaryAsset assetList = null;
						int subPosition = 0;
						switch (AssetType)
						{
							case String:
								assetList = new BinaryAsset(values.Length * 8);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									int stringLength = value.Length;
									FileHelper.SetInt(stringLength, subPosition, assetList.Content);
									subPosition += 4;
									stringLength += 4 - (stringLength & 3);
									BinaryAsset stringAsset = new BinaryAsset(stringLength);
									FileHelper.SetString(value, 0, stringAsset.Content);
									assetList.SubAssets.Add(subPosition, stringAsset);
									subPosition += 4;
								}
								break;
							case Byte:
								int byteCount = 4 - (values.Length & 3);
								if (byteCount == 4)
								{
									assetList = new BinaryAsset(values.Length);
								}
								else
								{
									assetList = new BinaryAsset(values.Length + byteCount);
								}
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									FileHelper.SetByte(byte.Parse(value), subPosition, assetList.Content);
									++subPosition;
								}
								break;
							case Angle:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.Angle angle = new Types.Angle(value);
									FileHelper.SetFloat(angle.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case Percentage:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.Percentage percentage = new Types.Percentage(value);
									FileHelper.SetFloat(percentage.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case SageBool:
								int sageBoolCount = 4 - (values.Length & 3);
								if (sageBoolCount == 4)
								{
									assetList = new BinaryAsset(values.Length);
								}
								else
								{
									assetList = new BinaryAsset(values.Length + sageBoolCount);
								}
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.SageBool sageBool = new Types.SageBool(value);
									FileHelper.SetBool(sageBool.Value, subPosition, assetList.Content);
									++subPosition;
								}
								break;
							case SageInt:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.SageInt sageInt = new Types.SageInt(value);
									FileHelper.SetInt(sageInt.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case SageReal:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.SageReal sageReal = new Types.SageReal(value);
									FileHelper.SetFloat(sageReal.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case SageUnsignedInt:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.SageUnsignedInt sageUnsignedInt = new Types.SageUnsignedInt(value);
									FileHelper.SetUInt(sageUnsignedInt.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case SageUnsignedShort:
								int sageUnsignedShortCount = 2 - (values.Length & 1);
								if (sageUnsignedShortCount == 2)
								{
									assetList = new BinaryAsset(values.Length * 2);
								}
								else
								{
									assetList = new BinaryAsset((values.Length + sageUnsignedShortCount) * 2);
								}
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.SageUnsignedShort sageUnsignedShort = new Types.SageUnsignedShort(value);
									FileHelper.SetUShort(sageUnsignedShort.Value, subPosition, assetList.Content);
									subPosition += 2;
								}
								break;
							case Time:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.Time time = new Types.Time(value);
									FileHelper.SetFloat(time.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case Velocity:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.Velocity velocity = new Types.Velocity(value);
									FileHelper.SetFloat(velocity.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							case DurationUnsignedInt:
								assetList = new BinaryAsset(values.Length * 4);
								asset.SubAssets.Add(position, assetList);
								foreach (string value in values)
								{
									Types.DurationUnsignedInt durationUnsignedInt = new Types.DurationUnsignedInt(value);
									FileHelper.SetUInt(durationUnsignedInt.Value, subPosition, assetList.Content);
									subPosition += 4;
								}
								break;
							default:
								foreach (BaseAssetType listBaseAsset in game.Assets.AssetTypes)
								{
									if (listBaseAsset.id == AssetType)
									{
										AssetType listAsset = listBaseAsset as AssetType;
										assetList = new BinaryAsset(values.Length * listAsset.GetLength(game));
										asset.SubAssets.Add(position, assetList);
										for (int idx = 0; idx < values.Length; ++idx)
										{
											XmlDocument fakeDocument = new XmlDocument();
											XmlNode fakeAttribute = fakeDocument.CreateAttribute(id);
											fakeAttribute.Value = values[idx];
											foreach (BaseEntryType listBaseEntry in listAsset.Entries)
											{
												if (!listBaseEntry.Compile(baseUri, assetList, fakeAttribute, game, trace, ref subPosition, out ErrorDescription))
												{
													return false;
												}
											}
										}
										break;
									}
								}
								break;
						}
						position += 4;
					}
					else
					{
						position += 8;
					}
				}
				else
				{
					if (IsRequired)
					{
						ErrorDescription = string.Format(RequiredNotFoundAttribute, id, trace);
						return false;
					}
					position += 8;
				}
			}
			else
			{
				List<XmlNode> nodes = new List<XmlNode>();
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (childNode.Name == id)
					{
						nodes.Add(childNode);
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
				if (nodes.Count != 0)
				{
					FileHelper.SetInt(nodes.Count, position, asset.Content);
					position += 4;
					trace += '.' + id;
					BinaryAsset assetList = null;
					int subPosition = 0;
					switch (AssetType)
					{
						case String:
							assetList = new BinaryAsset(nodes.Count * 8);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								int stringLength = value.Length;
								FileHelper.SetInt(stringLength, subPosition, assetList.Content);
								subPosition += 4;
								stringLength += 4 - (stringLength & 3);
								BinaryAsset stringAsset = new BinaryAsset(stringLength);
								FileHelper.SetString(value, 0, stringAsset.Content);
								assetList.SubAssets.Add(subPosition, stringAsset);
								subPosition += 4;
							}
							break;
						case Byte:
							int byteCount = 4 - (nodes.Count & 3);
							if (byteCount == 4)
							{
								assetList = new BinaryAsset(nodes.Count);
							}
							else
							{
								assetList = new BinaryAsset(nodes.Count + byteCount);
							}
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								FileHelper.SetByte(byte.Parse(value), subPosition, assetList.Content);
								++subPosition;
							}
							break;
						case Angle:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.Angle angle = new Types.Angle(value);
								FileHelper.SetFloat(angle.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case Percentage:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.Percentage percentage = new Types.Percentage(value);
								FileHelper.SetFloat(percentage.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case SageBool:
							int sageBoolCount = 4 - (nodes.Count & 3);
							if (sageBoolCount == 4)
							{
								assetList = new BinaryAsset(nodes.Count);
							}
							else
							{
								assetList = new BinaryAsset(nodes.Count + sageBoolCount);
							}
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.SageBool sageBool = new Types.SageBool(value);
								FileHelper.SetBool(sageBool.Value, subPosition, assetList.Content);
								++subPosition;
							}
							break;
						case SageInt:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.SageInt sageInt = new Types.SageInt(value);
								FileHelper.SetInt(sageInt.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case SageReal:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.SageReal sageReal = new Types.SageReal(value);
								FileHelper.SetFloat(sageReal.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case SageUnsignedInt:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.SageUnsignedInt sageUnsignedInt = new Types.SageUnsignedInt(value);
								FileHelper.SetUInt(sageUnsignedInt.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case SageUnsignedShort:
							int sageUnsignedShortCount = 2 - (nodes.Count & 1);
							if (sageUnsignedShortCount == 2)
							{
								assetList = new BinaryAsset(nodes.Count * 2);
							}
							else
							{
								assetList = new BinaryAsset((nodes.Count + sageUnsignedShortCount) * 2);
							}
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.SageUnsignedShort sageUnsignedShort = new Types.SageUnsignedShort(value);
								FileHelper.SetUShort(sageUnsignedShort.Value, subPosition, assetList.Content);
								subPosition += 2;
							}
							break;
						case Time:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.Time time = new Types.Time(value);
								FileHelper.SetFloat(time.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case Velocity:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.Velocity velocity = new Types.Velocity(value);
								FileHelper.SetFloat(velocity.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						case DurationUnsignedInt:
							assetList = new BinaryAsset(nodes.Count * 4);
							asset.SubAssets.Add(position, assetList);
							for (int idx = 0; idx < nodes.Count; ++idx)
							{
								string value = nodes[idx].InnerText;
								Types.DurationUnsignedInt durationUnsignedInt = new Types.DurationUnsignedInt(value);
								FileHelper.SetUInt(durationUnsignedInt.Value, subPosition, assetList.Content);
								subPosition += 4;
							}
							break;
						default:
							foreach (BaseAssetType listBaseAsset in game.Assets.AssetTypes)
							{
								if (listBaseAsset.id == AssetType)
								{
									AssetType listAsset = listBaseAsset as AssetType;
									assetList = new BinaryAsset(nodes.Count * listAsset.GetLength(game));
									asset.SubAssets.Add(position, assetList);
									for (int idx = 0; idx < nodes.Count; ++idx)
									{
										foreach (BaseEntryType listBaseEntry in listAsset.Entries)
										{
											if (!listBaseEntry.Compile(baseUri, assetList, nodes[idx], game, trace, ref subPosition, out ErrorDescription))
											{
												return false;
											}
										}
									}
									break;
								}
							}
							break;
					}
					position += 4;
				}
				else
				{
					position += 8;
				}
			}
			ErrorDescription = string.Empty;
			return true;
		}
	}
}
