using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Files;
using SAGE.WrathEdXML.AssetDefinition;

namespace SAGE
{
	public abstract class BaseAssetType
	{
		protected const string String = "String";
		protected const string Byte = "Byte";
		protected const string Angle = "Angle";
		protected const string Percentage = "Percentage";
		protected const string SageBool = "SageBool";
		protected const string SageInt = "SageInt";
		protected const string SageReal = "SageReal";
		protected const string SageUnsignedInt = "SageUnsignedInt";
		protected const string SageUnsignedShort = "SageUnsignedShort";
		protected const string Time = "Time";
		protected const string Velocity = "Velocity";
		protected const string DurationUnsignedInt = "DurationUnsignedInt";

		public string id { get; protected set; }
		public BaseAssetType Superclass { get; set; }
		public List<BaseAssetType> Subclasses { get; protected set; }

		public BaseAssetType(WrathEdXML.AssetDefinition.BaseAssetType asset)
		{
			id = asset.id;
			Subclasses = new List<BaseAssetType>();
		}

		public abstract int GetLength(GameDefinition game);
	}

	public class EnumAssetType : BaseAssetType
	{
		public string[] Enums { get; protected set; }
		public bool IsUsingAll { get; protected set; }
		public bool IsStartingNegative { get; protected set; }

		public EnumAssetType(WrathEdXML.AssetDefinition.EnumAssetType asset)
			: base(asset)
		{
			Enums = asset.Entry;
			IsUsingAll = asset.IsUsingAll;
			IsStartingNegative = asset.IsStartingNegative;
		}

		public override int GetLength(GameDefinition game)
		{
			return 4;
		}

		public string GetValue(int index)
		{
			if (IsStartingNegative)
			{
				return Enums[index + 1];
			}
			return Enums[index];
		}

		public string GetValue(uint index)
		{
			if (IsStartingNegative)
			{
				return Enums[index + 1];
			}
			return Enums[index];
		}

		public uint GetValue(string name)
		{
			if (name == string.Empty)
			{
				if (IsStartingNegative)
				{
					return 0xFFFFFFFF;
				}
				return 0x00000000;
			}
			for (uint idx = 0; idx < Enums.Length; ++idx)
			{
				if (Enums[idx] == name)
				{
					if (IsStartingNegative)
					{
						return idx - 1;
					}
					return idx;
				}
			}
			if (IsStartingNegative)
			{
				return 0xFFFFFFFF;
			}
			return 0x00000000;
		}
	}

	public class FlagsAssetType : BaseAssetType
	{
		public const int BitsInSpan = 32;

		public string BaseEnum { get; protected set; }


		public FlagsAssetType(WrathEdXML.AssetDefinition.FlagsAssetType asset)
			: base(asset)
		{
			BaseEnum = asset.BaseEnum;
		}

		public override int GetLength(GameDefinition game)
		{
			return NumSpans(game) * 4;
		}

		private EnumAssetType GetBaseEnum(GameDefinition game)
		{
			foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
			{
				if (baseAssetType.id == BaseEnum)
				{
					return baseAssetType as EnumAssetType;
				}
			}
			return null;
		}

		public int NumSpans(GameDefinition game)
		{
			EnumAssetType enumAsset = GetBaseEnum(game);
			return (enumAsset.Enums.Length + BitsInSpan - ((enumAsset.IsStartingNegative) ? 2 : 1)) / BitsInSpan;
		}

		public bool GetUsingAll(GameDefinition game)
		{
			return GetBaseEnum(game).IsUsingAll;
		}

		public string GetValue(int span, int bit, GameDefinition game)
		{
			return GetBaseEnum(game).GetValue(span * BitsInSpan + bit);
		}

		public uint[] GetValue(string value, GameDefinition game)
		{
			int numSpans = NumSpans(game);
			EnumAssetType enumAsset = GetBaseEnum(game);
			for (int span = 0; span < numSpans; ++span)
			{
				for (int bit = 0; bit < BitsInSpan; ++bit)
				{
					if (span * BitsInSpan + bit == enumAsset.Enums.Length - ((enumAsset.IsStartingNegative) ? 1 : 0))
					{
						return null;
					}
					if (value == enumAsset.GetValue(span * BitsInSpan + bit))
					{
						return new uint[] { (uint)span, (uint)(0x01 << bit) };
					}
				}
			}
			return null;
		}
	}

	public class AssetType : BaseAssetType
	{
		public BaseEntryType[] Entries { get; protected set; }

		public AssetType(WrathEdXML.AssetDefinition.AssetType asset)
			: base(asset)
		{
			if (asset.Entries == null)
			{
				return;
			}
			Entries = new BaseEntryType[asset.Entries.Length];
			for (int idx = 0; idx < Entries.Length; ++idx)
			{
				Type entryType = asset.Entries[idx].GetType();
				if (entryType == typeof(WrathEdXML.AssetDefinition.EntryPoidType))
				{
					Entries[idx] = new EntryPoidType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryPoidType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryFileType))
				{
					Entries[idx] = new EntryFileType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryFileType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryReferenceType))
				{
					Entries[idx] = new EntryReferenceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryReferenceType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryWeakReferenceType))
				{
					Entries[idx] = new EntryWeakReferenceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryWeakReferenceType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryType))
				{
					Entries[idx] = new EntryType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryRelocationType))
				{
					Entries[idx] = new EntryRelocationType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryRelocationType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryListType))
				{
					Entries[idx] = new EntryListType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryListType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryChoiceType))
				{
					Entries[idx] = new EntryChoiceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryChoiceType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryInheritanceType))
				{
					Entries[idx] = new EntryInheritanceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryInheritanceType);
				}
			}
		}

		public override int GetLength(GameDefinition game)
		{
			int length = 0;
			if (Entries != null)
			{
				foreach (BaseEntryType entry in Entries)
				{
					if (entry.IsVoid)
					{
						length += 4 - (length % 4);
					}
					else
					{
						Type entryType = entry.GetType();
						if (entryType == typeof(EntryInheritanceType))
						{
							EntryInheritanceType currentEntry = entry as EntryInheritanceType;
							bool isFound = false;
							foreach (GameAssetType gameAssetType in game.Assets.GameAssetTypes)
							{
								if (gameAssetType.id == currentEntry.AssetType)
								{
									isFound = true;
									length += gameAssetType.GetLength(game);
									break;
								}
							}
							if (!isFound)
							{
								foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
								{
									if (baseAssetType.id == currentEntry.AssetType)
									{
										isFound = true;
										length += baseAssetType.GetLength(game);
										break;
									}
								}
							}
						}
						else if (entryType == typeof(EntryType))
						{
							EntryType currentEntry = entry as EntryType;
							switch (currentEntry.AssetType)
							{
								case String:
									length += 8;
									break;
								case Angle:
								case Percentage:
								case SageInt:
								case SageReal:
								case SageUnsignedInt:
								case DurationUnsignedInt:
								case Time:
								case Velocity:
									length += 4;
									break;
								case SageUnsignedShort:
									length += 2;
									break;
								case Byte:
								case SageBool:
									++length;
									break;
								default:
									foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
									{
										if (baseAssetType.id == currentEntry.AssetType)
										{
											length += baseAssetType.GetLength(game);
											break;
										}
									}
									break;
							}
						}
						else if (entryType == typeof(EntryChoiceType))
						{
							EntryChoiceType currentEntry = entry as EntryChoiceType;
							if (currentEntry.MaxLength == 1)
							{
								length += 4;
							}
							else
							{
								length += 8;
							}
						}
						else if (entryType == typeof(EntryFileType)
							|| entryType == typeof(EntryListType))
						{
							length += 8;
						}
						else if (entryType == typeof(EntryPoidType)
							|| entryType == typeof(EntryReferenceType)
							|| entryType == typeof(EntryWeakReferenceType)
							|| entryType == typeof(EntryRelocationType))
						{
							length += 4;
						}
					}
				}
			}
			/*
			if ((length & 3) != 0)
			{
				length += 4 - (length & 3);
			}
			*/
			return length;
		}
	}

	public class RuntimeType
	{
		public BaseEntryType[] Entries { get; protected set; }

		public RuntimeType(WrathEdXML.AssetDefinition.RuntimeType asset)
		{
			Entries = new BaseEntryType[asset.Entries.Length];
			for (int idx = 0; idx < Entries.Length; ++idx)
			{
				Type entryType = asset.Entries[idx].GetType();
				if (entryType == typeof(WrathEdXML.AssetDefinition.EntryPoidType))
				{
					Entries[idx] = new EntryPoidType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryPoidType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryFileType))
				{
					Entries[idx] = new EntryFileType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryFileType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryReferenceType))
				{
					Entries[idx] = new EntryReferenceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryReferenceType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryWeakReferenceType))
				{
					Entries[idx] = new EntryWeakReferenceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryWeakReferenceType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryType))
				{
					Entries[idx] = new EntryType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryRelocationType))
				{
					Entries[idx] = new EntryRelocationType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryRelocationType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryListType))
				{
					Entries[idx] = new EntryListType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryListType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryChoiceType))
				{
					Entries[idx] = new EntryChoiceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryChoiceType);
				}
				else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryInheritanceType))
				{
					Entries[idx] = new EntryInheritanceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryInheritanceType);
				}
			}
		}
	}

	public class GameAssetType : BaseAssetType
	{
		public BaseEntryType[] Entries { get; protected set; }
		public RuntimeType Runtime { get; protected set; }
		public uint TypeHash { get; protected set; }
		public bool IsTokenized { get; protected set; }
		public bool HasSpecialCompileHandling { get; protected set; }

		public GameAssetType(WrathEdXML.AssetDefinition.GameAssetType asset)
			: base(asset)
		{
			if (asset.Entries != null)
			{
				Entries = new BaseEntryType[asset.Entries.Length];
				for (int idx = 0; idx < Entries.Length; ++idx)
				{
					Type entryType = asset.Entries[idx].GetType();
					if (entryType == typeof(WrathEdXML.AssetDefinition.EntryPoidType))
					{
						Entries[idx] = new EntryPoidType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryPoidType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryFileType))
					{
						Entries[idx] = new EntryFileType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryFileType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryReferenceType))
					{
						Entries[idx] = new EntryReferenceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryReferenceType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryWeakReferenceType))
					{
						Entries[idx] = new EntryWeakReferenceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryWeakReferenceType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryType))
					{
						Entries[idx] = new EntryType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryRelocationType))
					{
						Entries[idx] = new EntryRelocationType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryRelocationType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryListType))
					{
						Entries[idx] = new EntryListType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryListType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryChoiceType))
					{
						Entries[idx] = new EntryChoiceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryChoiceType);
					}
					else if (entryType == typeof(WrathEdXML.AssetDefinition.EntryInheritanceType))
					{
						Entries[idx] = new EntryInheritanceType(asset.Entries[idx] as WrathEdXML.AssetDefinition.EntryInheritanceType);
					}
				}
			}
			if (asset.Runtime != null)
			{
				Runtime = new RuntimeType(asset.Runtime);
			}
			TypeHash = asset.TypeHash;
			HasSpecialCompileHandling = asset.HasSpecialCompileHandling;
		}

		public override int GetLength(GameDefinition game)
		{
			int length = 0;
			BaseEntryType[] entries = null;
			if (Runtime != null)
			{
				entries = Runtime.Entries;
			}
			else
			{
				entries = Entries;
			}
			if (entries != null)
			{
				foreach (BaseEntryType entry in entries)
				{
					Type entryType = entry.GetType();
					if (entryType == typeof(EntryInheritanceType))
					{
						EntryInheritanceType currentEntry = entry as EntryInheritanceType;
						bool isFound = false;
						foreach (GameAssetType gameAssetType in game.Assets.GameAssetTypes)
						{
							if (gameAssetType.id == currentEntry.AssetType)
							{
								isFound = true;
								length += gameAssetType.GetLength(game);
								break;
							}
						}
						if (!isFound)
						{
							foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
							{
								if (baseAssetType.id == currentEntry.AssetType)
								{
									isFound = true;
									length += baseAssetType.GetLength(game);
									break;
								}
							}
						}
					}
					else if (entryType == typeof(EntryType))
					{
						if (entry.IsVoid)
						{
							length += 4 - (length & 3);
							continue;
						}
						EntryType currentEntry = entry as EntryType;
						switch (currentEntry.AssetType)
						{
							case String:
								length += 8;
								break;
							case Angle:
							case Percentage:
							case SageInt:
							case SageReal:
							case SageUnsignedInt:
							case DurationUnsignedInt:
							case Time:
							case Velocity:
								length += 4;
								break;
							case SageUnsignedShort:
								length += 2;
								break;
							case Byte:
							case SageBool:
								++length;
								break;
							default:
								foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
								{
									if (baseAssetType.id == currentEntry.AssetType)
									{
										length += baseAssetType.GetLength(game);
										break;
									}
								}
								break;
						}
					}
					else if (entryType == typeof(EntryChoiceType))
					{
						EntryChoiceType currentEntry = entry as EntryChoiceType;
						if (currentEntry.MaxLength == 1)
						{
							length += 4;
						}
						else
						{
							length += 8;
						}
					}
					else if (entryType == typeof(EntryFileType)
						|| entryType == typeof(EntryListType))
					{
						length += 8;
					}
					else if (entryType == typeof(EntryPoidType)
						|| entryType == typeof(EntryReferenceType)
						|| entryType == typeof(EntryWeakReferenceType)
						|| entryType == typeof(EntryRelocationType))
					{
						length += 4;
					}
				}
			}
			return length;
		}
	}

	public class AssetDefinition
	{
		public List<BaseAssetType> AssetTypes { get; protected set; }
		public List<GameAssetType> GameAssetTypes { get; protected set; }

		public AssetDefinition()
		{
			AssetTypes = new List<BaseAssetType>();
			GameAssetTypes = new List<GameAssetType>();
		}

		public AssetDefinition(WrathEdXML.AssetDefinition.AssetDefinition assetDefintion)
		{
			AssetTypes = new List<BaseAssetType>();
			GameAssetTypes = new List<GameAssetType>();
			if (assetDefintion != null && assetDefintion.Assets != null)
			{
				for (int idx = 0; idx < assetDefintion.Assets.Length; ++idx)
				{
					Type assetType = assetDefintion.Assets[idx].GetType();
					if (assetType == typeof(WrathEdXML.AssetDefinition.AssetType))
					{
						AssetTypes.Add(new AssetType(assetDefintion.Assets[idx] as WrathEdXML.AssetDefinition.AssetType));
					}
					else if (assetType == typeof(WrathEdXML.AssetDefinition.EnumAssetType))
					{
						AssetTypes.Add(new EnumAssetType(assetDefintion.Assets[idx] as WrathEdXML.AssetDefinition.EnumAssetType));
					}
					else if (assetType == typeof(WrathEdXML.AssetDefinition.FlagsAssetType))
					{
						AssetTypes.Add(new FlagsAssetType(assetDefintion.Assets[idx] as WrathEdXML.AssetDefinition.FlagsAssetType));
					}
					else if (assetType == typeof(WrathEdXML.AssetDefinition.GameAssetType))
					{
						GameAssetTypes.Add(new GameAssetType(assetDefintion.Assets[idx] as WrathEdXML.AssetDefinition.GameAssetType));
					}
				}
			}
		}

		public void Merge(AssetDefinition source)
		{
			AssetTypes.AddRange(source.AssetTypes);
			GameAssetTypes.AddRange(source.GameAssetTypes);
		}
	}
}
