using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryType : BaseEntryType
	{
		protected const string NotSupportedAssetTypeAttribute = "Critical Error: {0}.AttributeType:{1} not supported!";
		protected const string NotImplementedAssetTypeAttribute = "Critical Error: {0}.AttributeType:{1} not implemented!";
		protected const string NotSupportedAssetTypeElement = "Critical Error: {0}.ElementType:{1} not supported!";
		protected const string NotImplementedAssetTypeElement = "Critical Error: {0}.ElementType:{1} not implemented!";

		public string AssetType { get; protected set; }
		public string Default { get; protected set; }
		public string Description { get; protected set; }

		public EntryType(WrathEdXML.AssetDefinition.EntryType entry)
			: base(entry)
		{
			AssetType = entry.AssetType;
			Default = entry.Default;
			Description = entry.Description;
		}

		public override bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription)
		{
			if (IsVoid)
			{
				int count = 4 - (position & 3);
				if (count != 0)
				{
					if (Default != null)
					{
						for (int idx = 0; idx < count; ++idx)
						{
							asset.Content[position++ + idx] = byte.Parse(Default);
						}
					}
					else
					{
						position += count;
					}
				}
				ErrorDescription = string.Empty;
				return true;
			}
			bool isFound = false;
			if (IsAttribute)
			{
				string value = null;
				foreach (XmlAttribute attribute in node.Attributes)
				{
					if (attribute.Name == id)
					{
						isFound = true;
						value = attribute.Value;
						break;
					}
				}
				if (value == null && Default != null)
				{
					value = Default;
				}
				switch (AssetType)
				{
					case String:
						if (value == null)
						{
							position += 8;
						}
						else
						{
							int stringLength = value.Length;
							FileHelper.SetInt(stringLength, position, asset.Content);
							position += 4;
							stringLength += 4 - (stringLength & 3);
							BinaryAsset stringAsset = new BinaryAsset(stringLength);
							FileHelper.SetString(value, 0, stringAsset.Content);
							asset.SubAssets.Add(position, stringAsset);
							position += 4;
						}
						break;
					case Byte:
						if (value != null)
						{
							FileHelper.SetByte(byte.Parse(value), position, asset.Content);
						}
						++position;
						break;
					case Angle:
						if (value != null)
						{
							Types.Angle angle = new Types.Angle(value);
							FileHelper.SetFloat(angle.Value, position, asset.Content);
						}
						position += 4;
						break;
					case Percentage:
						if (value != null)
						{
							Types.Percentage percentage = new Types.Percentage(value);
							FileHelper.SetFloat(percentage.Value, position, asset.Content);
						}
						position += 4;
						break;
					case SageBool:
						if (value != null)
						{
							Types.SageBool sageBool = new Types.SageBool(value);
							FileHelper.SetBool(sageBool.Value, position, asset.Content);
						}
						++position;
						break;
					case SageInt:
						if (value != null)
						{
							Types.SageInt sageInt = new Types.SageInt(value);
							FileHelper.SetInt(sageInt.Value, position, asset.Content);
						}
						position += 4;
						break;
					case SageReal:
						if (value != null)
						{
							Types.SageReal sageReal = new Types.SageReal(value);
							FileHelper.SetFloat(sageReal.Value, position, asset.Content);
						}
						position += 4;
						break;
					case SageUnsignedInt:
						if (value != null)
						{
							Types.SageUnsignedInt sageUInt = new Types.SageUnsignedInt(value);
							FileHelper.SetUInt(sageUInt.Value, position, asset.Content);
						}
						position += 4;
						break;
					case SageUnsignedShort:
						if (value != null)
						{
							Types.SageUnsignedShort sageUShort = new Types.SageUnsignedShort(value);
							FileHelper.SetUShort(sageUShort.Value, position, asset.Content);
						}
						position += 2;
						break;
					case Time:
						if (value != null)
						{
							Types.Time time = new Types.Time(value);
							FileHelper.SetFloat(time.Value, position, asset.Content);
						}
						position += 4;
						break;
					case Velocity:
						if (value != null)
						{
							Types.Velocity velocity = new Types.Velocity(value);
							FileHelper.SetFloat(velocity.Value, position, asset.Content);
						}
						position += 4;
						break;
					default:
						bool isImplemented = false;
						foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
						{
							if (baseAssetType.id == AssetType)
							{
								isImplemented = true;
								Type assetTypeType = baseAssetType.GetType();
								if (assetTypeType == typeof(EnumAssetType))
								{
									if (value != null)
									{
										EnumAssetType enumAsset = baseAssetType as EnumAssetType;
										FileHelper.SetUInt(enumAsset.GetValue(value), position, asset.Content);
									}
									position += 4;
									break;
								}
								if (assetTypeType == typeof(FlagsAssetType))
								{
									FlagsAssetType flagsAsset = baseAssetType as FlagsAssetType;
									int numSpans = flagsAsset.NumSpans(game);
									if (value != null)
									{
										string[] flags = value.Split(' ');
										if (flagsAsset.GetUsingAll(game) && flags[0] == "ALL")
										{
											for (int idx = 0; idx < numSpans; ++idx)
											{
												FileHelper.SetUInt(0xFFFFFFFF, position + (idx << 2), asset.Content);
											}
										}
										else
										{
											foreach (string flag in flags)
											{
												uint[] spanAndBit = flagsAsset.GetValue(flag, game);
												if (spanAndBit != null)
												{
													FileHelper.SetUInt(FileHelper.GetUInt((int)(position + (spanAndBit[0] << 2)), asset.Content) | spanAndBit[1],
														(int)(position + (spanAndBit[0] << 2)), asset.Content);
												}
											}
										}
									}
									position += numSpans << 2;
									break;
								}
								else
								{
									ErrorDescription = string.Format(NotSupportedAssetTypeAttribute, typeof(EntryType).Name, AssetType);
									return false;
								}
							}
						}
						if (!isImplemented)
						{
							ErrorDescription = string.Format(NotImplementedAssetTypeAttribute, typeof(EntryType).Name, AssetType);
							return false;
						}
						break;
				}
			}
			else
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (childNode.Name == id)
					{
						isFound = true;
						if (AssetType != String
							&& AssetType != Byte
							&& AssetType != Angle
							&& AssetType != Percentage
							&& AssetType != SageBool
							&& AssetType != SageInt
							&& AssetType != SageReal
							&& AssetType != SageUnsignedInt
							&& AssetType != SageUnsignedShort
							&& AssetType != Time
							&& AssetType != Velocity)
						{
							foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
							{
								if (baseAssetType.id == AssetType)
								{
									AssetType baseAsset = baseAssetType as AssetType;
									if (baseAsset.Entries != null)
									{
										trace += '.' + id;
										foreach (BaseEntryType baseEntryType in baseAsset.Entries)
										{
											if (!baseEntryType.Compile(baseUri, asset, childNode, game, trace, ref position, out ErrorDescription))
											{
												return false;
											}
										}
									}
									ErrorDescription = string.Empty;
									return true;
								}
							}
							ErrorDescription = string.Format(NotImplementedAssetTypeElement, typeof(EntryType).Name, AssetType);
							return false;
						}
						string value = childNode.InnerXml;
						if (value == string.Empty && Default != null)
						{
							value = Default;
						}
						switch (AssetType)
						{
							case String:
								int stringLength = value.Length;
								FileHelper.SetInt(stringLength, position, asset.Content);
								position += 4;
								stringLength += 4 - (stringLength & 3);
								BinaryAsset stringAsset = new BinaryAsset(stringLength);
								FileHelper.SetString(value, 0, stringAsset.Content);
								asset.SubAssets.Add(position, stringAsset);
								position += 4;
								break;
							case Byte:
								if (value != string.Empty)
								{
									FileHelper.SetByte(byte.Parse(value), position, asset.Content);
								}
								++position;
								break;
							case Angle:
								if (value != string.Empty)
								{
									Types.Angle angle = new Types.Angle(value);
									FileHelper.SetFloat(angle.Value, position, asset.Content);
								}
								position += 4;
								break;
							case Percentage:
								if (value != string.Empty)
								{
									Types.Percentage percentage = new Types.Percentage(value);
									FileHelper.SetFloat(percentage.Value, position, asset.Content);
								}
								position += 4;
								break;
							case SageBool:
								if (value != string.Empty)
								{
									Types.SageBool sageBool = new Types.SageBool(value);
									FileHelper.SetBool(sageBool.Value, position, asset.Content);
								}
								++position;
								break;
							case SageInt:
								if (value != string.Empty)
								{
									Types.SageInt sageInt = new Types.SageInt(value);
									FileHelper.SetInt(sageInt.Value, position, asset.Content);
								}
								position += 4;
								break;
							case SageReal:
								if (value != string.Empty)
								{
									Types.SageReal sageReal = new Types.SageReal(value);
									FileHelper.SetFloat(sageReal.Value, position, asset.Content);
								}
								position += 4;
								break;
							case SageUnsignedInt:
								if (value != string.Empty)
								{
									Types.SageUnsignedInt sageUInt = new Types.SageUnsignedInt(value);
									FileHelper.SetUInt(sageUInt.Value, position, asset.Content);
								}
								position += 4;
								break;
							case SageUnsignedShort:
								if (value != string.Empty)
								{
									Types.SageUnsignedShort sageUShort = new Types.SageUnsignedShort(value);
									FileHelper.SetUShort(sageUShort.Value, position, asset.Content);
								}
								position += 2;
								break;
							case Time:
								if (value != string.Empty)
								{
									Types.Time time = new Types.Time(value);
									FileHelper.SetFloat(time.Value, position, asset.Content);
								}
								position += 4;
								break;
							case Velocity:
								if (value != string.Empty)
								{
									Types.Velocity velocity = new Types.Velocity(value);
									FileHelper.SetFloat(velocity.Value, position, asset.Content);
								}
								position += 4;
								break;
						}
						break;
					}
				}
				if (!isFound)
				{
					if (AssetType != String
						&& AssetType != Byte
						&& AssetType != Angle
						&& AssetType != Percentage
						&& AssetType != SageBool
						&& AssetType != SageInt
						&& AssetType != SageReal
						&& AssetType != SageUnsignedInt
						&& AssetType != SageUnsignedShort
						&& AssetType != Time
						&& AssetType != Velocity)
					{
						foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
						{
							if (baseAssetType.id == AssetType)
							{
								AssetType baseAsset = baseAssetType as AssetType;
								if (Default != null && baseAsset.Entries != null)
								{
									XmlDocument fakeDocument = new XmlDocument();
									XmlNode fakeNode = fakeDocument.CreateElement(id);
									trace += '.' + id;
									string[] defaults = Default.Split(',');
									int idx = 0;
									if (baseAsset.Entries[idx].GetType() == typeof(EntryInheritanceType))
									{
										string inheritanceAssetType = (baseAsset.Entries[idx] as EntryInheritanceType).AssetType;
										foreach (BaseAssetType baseInheritedAssetType in game.Assets.AssetTypes)
										{
											if (baseInheritedAssetType.id == inheritanceAssetType)
											{
												AssetType inheritedBaseAsset = baseInheritedAssetType as AssetType;
												while (idx < defaults.Length)
												{
													XmlAttribute fakeAttribute = fakeDocument.CreateAttribute(inheritedBaseAsset.Entries[idx].id);
													fakeAttribute.Value = defaults[idx];
													fakeNode.Attributes.Append(fakeAttribute);
													++idx;
												}
												break;
											}
										}
										int idy = 1;
										while (idx < defaults.Length)
										{
											XmlAttribute fakeAttribute = fakeDocument.CreateAttribute(baseAsset.Entries[idy].id);
											fakeAttribute.Value = defaults[idx];
											fakeNode.Attributes.Append(fakeAttribute);
											++idx;
											++idy;
										}
									}
									else
									{
										while (idx < defaults.Length)
										{
											XmlAttribute fakeAttribute = fakeDocument.CreateAttribute(baseAsset.Entries[idx].id);
											fakeAttribute.Value = defaults[idx];
											fakeNode.Attributes.Append(fakeAttribute);
											++idx;
										}
									}
									foreach (BaseEntryType baseEntryType in baseAsset.Entries)
									{
										if (!baseEntryType.Compile(baseUri, asset, fakeNode, game, trace, ref position, out ErrorDescription))
										{
											return false;
										}
									}
								}
								else
								{
									position += baseAsset.GetLength(game);
								}
								break;
							}
						}
					}
					else
					{
						switch (AssetType)
						{
							case String:
								position += 8;
								break;
							case Angle:
							case Percentage:
							case SageInt:
							case SageReal:
							case SageUnsignedInt:
							case Time:
							case Velocity:
								position += 4;
								break;
							case SageUnsignedShort:
								position += 2;
								break;
							case Byte:
							case SageBool:
								++position;
								break;
						}
					}
					//ErrorDescription = string.Format(NotRequiredNotFoundElement, id, trace);
					//return false;
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
			ErrorDescription = string.Empty;
			return true;
		}
	}
}