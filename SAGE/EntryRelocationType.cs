using System;
using System.Collections.Generic;
using System.Xml;
using Files;

namespace SAGE
{
	public class EntryRelocationType : BaseEntryType
	{
		public string AssetType { get; protected set; }
		public string Description { get; protected set; }

		public EntryRelocationType(WrathEdXML.AssetDefinition.EntryRelocationType entry)
			: base(entry)
		{
			AssetType = entry.AssetType;
			Description = entry.Description;
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
						string value;
						switch (AssetType)
						{
							case String:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(8);
									asset.SubAssets.Add(position, assetRelocation);
									int stringLength = value.Length;
									FileHelper.SetInt(stringLength, 0, assetRelocation.Content);
									stringLength += 4 - (stringLength & 3);
									BinaryAsset stringAsset = new BinaryAsset(stringLength);
									FileHelper.SetString(value, 0, stringAsset.Content);
									assetRelocation.SubAssets.Add(4, stringAsset);
								}
								break;
							case Byte:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									FileHelper.SetByte(byte.Parse(value), 0, assetRelocation.Content);
								}
								break;
							case Angle:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Angle angle = new Types.Angle(value);
									FileHelper.SetFloat(angle.Value, 0, assetRelocation.Content);
								}
								break;
							case Percentage:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Percentage percentage = new Types.Percentage(value);
									FileHelper.SetFloat(percentage.Value, 0, assetRelocation.Content);
								}
								break;
							case SageBool:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageBool sageBool = new Types.SageBool(value);
									FileHelper.SetBool(sageBool.Value, 0, assetRelocation.Content);
								}
								break;
							case SageInt:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageInt sageInt = new Types.SageInt(value);
									FileHelper.SetInt(sageInt.Value, 0, assetRelocation.Content);
								}
								break;
							case SageReal:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageReal sageReal = new Types.SageReal(value);
									FileHelper.SetFloat(sageReal.Value, 0, assetRelocation.Content);
								}
								break;
							case SageUnsignedInt:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageUnsignedInt sageUInt = new Types.SageUnsignedInt(value);
									FileHelper.SetUInt(sageUInt.Value, 0, assetRelocation.Content);
								}
								break;
							case SageUnsignedShort:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageUnsignedShort sageUShort = new Types.SageUnsignedShort(value);
									FileHelper.SetUShort(sageUShort.Value, 0, assetRelocation.Content);
								}
								break;
							case Time:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Time time = new Types.Time(value);
									FileHelper.SetFloat(time.Value, 0, assetRelocation.Content);
								}
								break;
							case Velocity:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Velocity velocity = new Types.Velocity(value);
									FileHelper.SetFloat(velocity.Value, 0, assetRelocation.Content);
								}
								break;
							case DurationUnsignedInt:
								value = attribute.Value;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.DurationUnsignedInt durationUInt = new Types.DurationUnsignedInt(value);
									FileHelper.SetUInt(durationUInt.Value, 0, assetRelocation.Content);
								}
								break;
						}
						foreach (BaseAssetType relocationBaseAsset in game.Assets.AssetTypes)
						{
							if (relocationBaseAsset.id == AssetType)
							{
								int length = relocationBaseAsset.GetLength(game);
								BinaryAsset assetRelocation = new BinaryAsset(length);
								asset.SubAssets.Add(position, assetRelocation);
								trace += '.' + id;
								Type assetTypeType = relocationBaseAsset.GetType();
								int subPosition = 0;
								if (assetTypeType == typeof(EnumAssetType))
								{
									EnumAssetType enumAsset = relocationBaseAsset as EnumAssetType;
									FileHelper.SetUInt(enumAsset.GetValue(attribute.Value.Trim()), subPosition, assetRelocation.Content);
									subPosition += 4;
								}
								else if (assetTypeType == typeof(FlagsAssetType))
								{
									FlagsAssetType flagsAsset = relocationBaseAsset as FlagsAssetType;
									int numSpans = flagsAsset.NumSpans(game);
									value = attribute.Value.Trim();
									if (value.Length > 0)
									{
										string[] flags = value.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
										foreach (string flag in flags)
										{
											uint[] spanAndBit = flagsAsset.GetValue(flag, game);
											if (spanAndBit != null)
											{
												FileHelper.SetUInt(FileHelper.GetUInt((int)(subPosition + (spanAndBit[0] << 2)), assetRelocation.Content) | spanAndBit[1],
													(int)(subPosition + (spanAndBit[0] << 2)), assetRelocation.Content);
											}
										}
									}
									subPosition += numSpans << 2;
									break;
								}
								else
								{
									AssetType relocationAsset = relocationBaseAsset as AssetType;
									foreach (BaseEntryType relocationBaseEntry in relocationAsset.Entries)
									{
										if (!relocationBaseEntry.Compile(baseUri, assetRelocation, attribute, game, trace, ref subPosition, out ErrorDescription))
										{
											return false;
										}
									}
								}
							}
						}
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
						string value;
						switch (AssetType)
						{
							case String:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(8);
									asset.SubAssets.Add(position, assetRelocation);
									int stringLength = value.Length;
									FileHelper.SetInt(stringLength, 0, assetRelocation.Content);
									stringLength += 4 - (stringLength & 3);
									BinaryAsset stringAsset = new BinaryAsset(stringLength);
									FileHelper.SetString(value, 0, stringAsset.Content);
									assetRelocation.SubAssets.Add(4, stringAsset);
								}
								break;
							case Byte:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									FileHelper.SetByte(byte.Parse(value), 0, assetRelocation.Content);
								}
								break;
							case Angle:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Angle angle = new Types.Angle(value);
									FileHelper.SetFloat(angle.Value, 0, assetRelocation.Content);
								}
								break;
							case Percentage:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Percentage percentage = new Types.Percentage(value);
									FileHelper.SetFloat(percentage.Value, 0, assetRelocation.Content);
								}
								break;
							case SageBool:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageBool sageBool = new Types.SageBool(value);
									FileHelper.SetBool(sageBool.Value, 0, assetRelocation.Content);
								}
								break;
							case SageInt:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageInt sageInt = new Types.SageInt(value);
									FileHelper.SetInt(sageInt.Value, 0, assetRelocation.Content);
								}
								break;
							case SageReal:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageReal sageReal = new Types.SageReal(value);
									FileHelper.SetFloat(sageReal.Value, 0, assetRelocation.Content);
								}
								break;
							case SageUnsignedInt:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageUnsignedInt sageUInt = new Types.SageUnsignedInt(value);
									FileHelper.SetUInt(sageUInt.Value, 0, assetRelocation.Content);
								}
								break;
							case SageUnsignedShort:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.SageUnsignedShort sageUShort = new Types.SageUnsignedShort(value);
									FileHelper.SetUShort(sageUShort.Value, 0, assetRelocation.Content);
								}
								break;
							case Time:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Time time = new Types.Time(value);
									FileHelper.SetFloat(time.Value, 0, assetRelocation.Content);
								}
								break;
							case Velocity:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.Velocity velocity = new Types.Velocity(value);
									FileHelper.SetFloat(velocity.Value, 0, assetRelocation.Content);
								}
								break;
							case DurationUnsignedInt:
								value = childNode.InnerText;
								if (value != string.Empty)
								{
									BinaryAsset assetRelocation = new BinaryAsset(4);
									asset.SubAssets.Add(position, assetRelocation);
									Types.DurationUnsignedInt durationUInt = new Types.DurationUnsignedInt(value);
									FileHelper.SetUInt(durationUInt.Value, 0, assetRelocation.Content);
								}
								break;
						}
						foreach (BaseAssetType relocationBaseAsset in game.Assets.AssetTypes)
						{
							if (relocationBaseAsset.id == AssetType)
							{
								int length = relocationBaseAsset.GetLength(game);
								BinaryAsset assetRelocation = new BinaryAsset(length);
								asset.SubAssets.Add(position, assetRelocation);
								trace += '.' + id;
								Type assetTypeType = relocationBaseAsset.GetType();
								int subPosition = 0;
								if (assetTypeType == typeof(EnumAssetType))
								{
									EnumAssetType enumAsset = relocationBaseAsset as EnumAssetType;
									FileHelper.SetUInt(enumAsset.GetValue(childNode.InnerText.Trim()), subPosition, assetRelocation.Content);
									subPosition += 4;
								}
								else if (assetTypeType == typeof(FlagsAssetType))
								{
									FlagsAssetType flagsAsset = relocationBaseAsset as FlagsAssetType;
									int numSpans = flagsAsset.NumSpans(game);
									value = childNode.InnerText.Trim();
									if (value.Length > 0)
									{
										string[] flags = value.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
										foreach (string flag in flags)
										{
											uint[] spanAndBit = flagsAsset.GetValue(flag, game);
											if (spanAndBit != null)
											{
												FileHelper.SetUInt(FileHelper.GetUInt((int)(subPosition + (spanAndBit[0] << 2)), assetRelocation.Content) | spanAndBit[1],
													(int)(subPosition + (spanAndBit[0] << 2)), assetRelocation.Content);
											}
										}
									}
									subPosition += numSpans << 2;
									break;
								}
								else
								{
									AssetType relocationAsset = relocationBaseAsset as AssetType;
									foreach (BaseEntryType relocationBaseEntry in relocationAsset.Entries)
									{
										if (!relocationBaseEntry.Compile(baseUri, assetRelocation, childNode, game, trace, ref subPosition, out ErrorDescription))
										{
											return false;
										}
									}
								}
							}
						}
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
			if (!isFound)
			{
				//ErrorDescription = string.Format("Critical Error: {0}.:{1} not found, handling not yet implemented!", typeof(EntryListType).Name, AssetType);
				//return false;
			}
			position += 4;
			ErrorDescription = string.Empty;
			return true;
		}
	}
}
