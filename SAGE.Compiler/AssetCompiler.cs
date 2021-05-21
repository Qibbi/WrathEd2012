using Files;
using SAGE.Stream;
using SAGE.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SAGE.Compiler
{
    public static class AssetCompiler
    {
        public static Dictionary<string, Asset> TempAssets;
        public static Dictionary<string, Asset> Assets;

        private static GameDefinition game;
        private const string xmlNameSpace = "uri:ea.com:eala:asset";

        private const string DDS = "DDS";

        // MOVE
        private const string String = "String";
        private const string Byte = "Byte";
        private const string SByte = "SByte";
        private const string Angle = "Angle";
        private const string Percentage = "Percentage";
        private const string SageBool = "SageBool";
        private const string SageInt = "SageInt";
        private const string SageReal = "SageReal";
        private const string SageUnsignedInt = "SageUnsignedInt";
        private const string SageUnsignedShort = "SageUnsignedShort";
        private const string Time = "Time";
        private const string Velocity = "Velocity";
        private const string DurationUnsignedInt = "DurationUnsignedInt";

        static AssetCompiler()
        {
            TempAssets = new Dictionary<string, Asset>();
            Assets = new Dictionary<string, Asset>();
        }

        private static void DecompileEntry(Stream.File stream, List<Stream.File> referencedStreams,
            BaseEntryType baseEntry, XmlDocument document, XmlNode entryNode, List<AssetReference> assetReferences,
            byte[] bin, byte[] imp, byte[] relo, ref int binPosition)
        {
            if (baseEntry.IsVoid)
            {
                binPosition += 4 - (binPosition % 4);
                return;
            }
            Type entryType = baseEntry.GetType();
            if (baseEntry.IsAttribute)
            {
                XmlAttribute attribute = document.CreateAttribute(baseEntry.id);
                if (entryType == typeof(EntryPoidType))
                {
                    uint poid = FileHelper.GetUInt(binPosition, bin);
                    binPosition += 4;
                    if (poid != 0)
                    {
                        attribute.Value += string.Format("{0:X08}", poid);
                        entryNode.Attributes.Append(attribute);
                    }
                }
                if (entryType == typeof(EntryReferenceType))
                {
                    int referenceIndex = FileHelper.GetInt(binPosition, bin);
                    if (game.ManifestVersion != 5)
                    {
                        --referenceIndex;
                    }
                    bool isImport = false;
                    for (int idx = 0; idx < imp.Length; idx += 4)
                    {
                        if (binPosition == FileHelper.GetInt(idx, imp))
                        {
                            isImport = true;
                            break;
                        }
                    }
                    if (isImport)
                    {
                        bool isFound = false;
                        foreach (AssetEntry assetEntry in stream.AssetEntries)
                        {
                            if (assetEntry.TypeId == assetReferences[referenceIndex].TypeId && assetEntry.InstanceId == assetReferences[referenceIndex].InstanceId)
                            {
                                attribute.Value += stream.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound && referencedStreams != null)
                        {
                            foreach (Stream.File file in referencedStreams)
                            {
                                foreach (AssetEntry assetEntry in file.AssetEntries)
                                {
                                    if (assetEntry.TypeId == assetReferences[referenceIndex].TypeId && assetEntry.InstanceId == assetReferences[referenceIndex].InstanceId)
                                    {
                                        attribute.Value += file.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                        isFound = true;
                                        break;
                                    }
                                }
                                if (isFound)
                                {
                                    break;
                                }
                            }
                        }
                        if (!isFound)
                        {
                            attribute.Value += string.Format("Asset not found in stream: [{0:X08}:{1:X08}]", assetReferences[referenceIndex].TypeId, assetReferences[referenceIndex].InstanceId);
                        }
                    }
                    binPosition += 4;
                    entryNode.Attributes.Append(attribute);
                }
                else if (entryType == typeof(EntryWeakReferenceType))
                {
                    uint typeId = 0;
                    if ((baseEntry as EntryWeakReferenceType).AssetType != "BaseAssetType")
                    {
                        typeId = StringHasher.Hash((baseEntry as EntryWeakReferenceType).AssetType);
                    }
                    uint reference = FileHelper.GetUInt(binPosition, bin);
                    binPosition += 4;
                    if (reference != 0)
                    {
                        bool isFound = false;
                        foreach (AssetEntry assetEntry in stream.AssetEntries)
                        {
                            if ((typeId == 0 || assetEntry.TypeId == typeId) && assetEntry.InstanceId == reference)
                            {
                                attribute.Value += stream.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound && referencedStreams != null)
                        {
                            foreach (Stream.File file in referencedStreams)
                            {
                                foreach (AssetEntry assetEntry in file.AssetEntries)
                                {
                                    if ((typeId == 0 || assetEntry.TypeId == typeId) && assetEntry.InstanceId == reference)
                                    {
                                        attribute.Value += file.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                        isFound = true;
                                        break;
                                    }
                                }
                                if (isFound)
                                {
                                    break;
                                }
                            }
                        }
                        if (!isFound)
                        {
                            attribute.Value += string.Format("Asset not found in stream: [{0:X08}]", reference);
                        }
                    }
                    entryNode.Attributes.Append(attribute);
                }
                else if (entryType == typeof(EntryType))
                {
                    string defaultValue = string.Empty;
                    string typeDefaultValue = string.Empty;
                    EntryType entry = baseEntry as EntryType;
                    foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
                    {
                        if (baseAssetType.id == entry.AssetType)
                        {
                            Type assetTypeType = baseAssetType.GetType();
                            if (assetTypeType == typeof(EnumAssetType))
                            {
                                EnumAssetType asset = baseAssetType as EnumAssetType;
                                attribute.Value = asset.GetValue(FileHelper.GetUInt(binPosition, bin));
                                binPosition += 4;
                            }
                            else if (assetTypeType == typeof(FlagsAssetType))
                            {
                                FlagsAssetType asset = baseAssetType as FlagsAssetType;
                                int numSpans = asset.NumSpans(game);
                                uint[] flags = new uint[numSpans];
                                for (int idx = 0; idx < numSpans; ++idx)
                                {
                                    flags[idx] = FileHelper.GetUInt(binPosition, bin);
                                    binPosition += 4;
                                }
                                bool isAll = false;
                                if (asset.GetUsingAll(game))
                                {
                                    isAll = true;
                                    for (int idx = 0; idx < numSpans; ++idx)
                                    {
                                        if (flags[idx] != 0xFFFFFFFF)
                                        {
                                            isAll = false;
                                            break;
                                        }
                                    }
                                }
                                if (isAll)
                                {
                                    attribute.Value += "ALL";
                                }
                                else
                                {
                                    StringBuilder flagsStringBuilder = new StringBuilder();
                                    for (int idx = 0; idx < numSpans; ++idx)
                                    {
                                        for (int idy = 0; idy < FlagsAssetType.BitsInSpan; ++idy)
                                        {
                                            if ((flags[idx] & (1 << idy)) != 0)
                                            {
                                                flagsStringBuilder.Append(asset.GetValue(idx, idy, game));
                                                flagsStringBuilder.Append(" ");
                                            }
                                        }
                                    }
                                    if (flagsStringBuilder.Length != 0)
                                    {
                                        flagsStringBuilder.Remove(flagsStringBuilder.Length - 1, 1);
                                    }
                                    attribute.Value += flagsStringBuilder.ToString();
                                }
                            }
                            break;
                        }
                    }
                    switch (entry.AssetType)
                    {
                        case String:
                            if (entry.Default != null)
                            {
                                defaultValue = entry.Default;
                            }
                            int stringLength = FileHelper.GetInt(binPosition, bin);
                            binPosition += 4;
                            int stringOffset = FileHelper.GetInt(binPosition, bin);
                            binPosition += 4;
                            if (stringOffset != 0)
                            {
                                attribute.Value += FileHelper.GetString(stringOffset, bin, stringLength);
                            }
                            else
                            {
                                return;
                            }
                            break;
                        case Byte:
                            if (entry.Default != null)
                            {
                                defaultValue = entry.Default;
                            }
                            typeDefaultValue = "0";
                            attribute.Value += FileHelper.GetByte(binPosition, bin).ToString();
                            ++binPosition;
                            break;
                        case Angle:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.Angle(entry.Default).Value.ToString() + 'd';
                            }
                            typeDefaultValue = SAGE.Types.Angle.DefaultValue;
                            attribute.Value += (FileHelper.GetFloat(binPosition, bin) * Types.Constants.DEGREES_PER_RAD).ToString() + 'd';
                            binPosition += 4;
                            break;
                        case Percentage:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.Percentage(entry.Default).Value.ToString() + '%';
                            }
                            typeDefaultValue = SAGE.Types.Percentage.DefaultValue;
                            attribute.Value += (FileHelper.GetFloat(binPosition, bin) * 100).ToString() + '%';
                            binPosition += 4;
                            break;
                        case SageBool:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.SageBool(entry.Default).Value.ToString();
                            }
                            typeDefaultValue = SAGE.Types.SageBool.DefaultValue;
                            attribute.Value += FileHelper.GetBool(binPosition, bin).ToString();
                            ++binPosition;
                            break;
                        case SageInt:
                            if (entry.Default != null)
                            {
                                defaultValue = entry.Default;
                            }
                            typeDefaultValue = SAGE.Types.SageInt.DefaultValue;
                            attribute.Value += FileHelper.GetInt(binPosition, bin).ToString();
                            binPosition += 4;
                            break;
                        case SageReal:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.SageReal(entry.Default).Value.ToString();
                            }
                            typeDefaultValue = SAGE.Types.SageReal.DefaultValue;
                            attribute.Value += FileHelper.GetFloat(binPosition, bin).ToString();
                            binPosition += 4;
                            break;
                        case SageUnsignedInt:
                            if (entry.Default != null)
                            {
                                defaultValue = entry.Default;
                            }
                            typeDefaultValue = SAGE.Types.SageUnsignedInt.DefaultValue;
                            attribute.Value += FileHelper.GetUInt(binPosition, bin).ToString();
                            binPosition += 4;
                            break;
                        case SageUnsignedShort:
                            if (entry.Default != null)
                            {
                                defaultValue = entry.Default;
                            }
                            typeDefaultValue = SAGE.Types.SageUnsignedShort.DefaultValue;
                            attribute.Value += FileHelper.GetUShort(binPosition, bin).ToString();
                            binPosition += 2;
                            break;
                        case Time:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.Time(entry.Default).Value.ToString() + 's';
                            }
                            typeDefaultValue = SAGE.Types.Time.DefaultValue;
                            attribute.Value += FileHelper.GetFloat(binPosition, bin).ToString() + "s";
                            binPosition += 4;
                            break;
                        case Velocity:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.Velocity(entry.Default).Value.ToString();
                            }
                            typeDefaultValue = SAGE.Types.Velocity.DefaultValue;
                            attribute.Value += (FileHelper.GetFloat(binPosition, bin) * Types.Constants.LOGICFRAMES_PER_SECONDS_REAL).ToString();
                            binPosition += 4;
                            break;
                        case DurationUnsignedInt:
                            if (entry.Default != null)
                            {
                                defaultValue = new SAGE.Types.DurationUnsignedInt(entry.Default).Value.ToString();
                            }
                            typeDefaultValue = SAGE.Types.DurationUnsignedInt.DefaultValue;
                            attribute.Value += (FileHelper.GetUInt(binPosition, bin) * Types.Constants.MSEC_PER_LOGICFRAMES_REAL).ToString();
                            binPosition += 4;
                            break;
                    }
                    entryNode.Attributes.Append(attribute);
                }
                else if (entryType == typeof(EntryRelocationType))
                {
                    string typeDefaultValue = string.Empty;
                    int relocationOffset = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    if (relocationOffset == 0)
                    {
                        return;
                    }
                    EntryRelocationType entry = baseEntry as EntryRelocationType;
                    foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
                    {
                        if (baseAssetType.id == entry.AssetType)
                        {
                            Type assetTypeType = baseAssetType.GetType();
                            if (assetTypeType == typeof(EnumAssetType))
                            {
                                EnumAssetType asset = baseAssetType as EnumAssetType;
                                attribute.Value += asset.GetValue(FileHelper.GetUInt(relocationOffset, bin));
                                relocationOffset += 4;
                            }
                            else if (assetTypeType == typeof(FlagsAssetType))
                            {
                                FlagsAssetType asset = baseAssetType as FlagsAssetType;
                                int numSpans = asset.NumSpans(game);
                                uint[] flags = new uint[numSpans];
                                for (int idx = 0; idx < numSpans; ++idx)
                                {
                                    flags[idx] = FileHelper.GetUInt(relocationOffset, bin);
                                    relocationOffset += 4;
                                }
                                StringBuilder flagsStringBuilder = new StringBuilder();
                                for (int idx = 0; idx < numSpans; ++idx)
                                {
                                    for (int idy = 0; idy < FlagsAssetType.BitsInSpan; ++idy)
                                    {
                                        if ((flags[idx] & (1 << idy)) != 0)
                                        {
                                            flagsStringBuilder.Append(asset.GetValue(idx, idy, game));
                                            flagsStringBuilder.Append(" ");
                                        }
                                    }
                                }
                                if (flagsStringBuilder.Length != 0)
                                {
                                    flagsStringBuilder.Remove(flagsStringBuilder.Length - 1, 1);
                                }
                                attribute.Value += flagsStringBuilder.ToString();
                            }
                            if (assetTypeType == typeof(AssetType))
                            {
                                AssetType assetType = baseAssetType as AssetType;
                                foreach (BaseEntryType assetEntry in assetType.Entries)
                                {
                                    DecompileEntry(stream, referencedStreams, assetEntry, document, attribute, assetReferences, bin, imp, relo, ref relocationOffset);
                                }
                            }
                            break;
                        }
                    }
                    switch (entry.AssetType)
                    {
                        case String:
                            int stringLength = FileHelper.GetInt(relocationOffset, bin);
                            relocationOffset += 4;
                            int stringOffset = FileHelper.GetInt(relocationOffset, bin);
                            if (stringOffset != 0)
                            {
                                attribute.Value = FileHelper.GetString(stringOffset, bin, stringLength);
                            }
                            break;
                        case Byte:
                            typeDefaultValue = "0";
                            attribute.Value += FileHelper.GetByte(relocationOffset, bin).ToString();
                            break;
                        case Angle:
                            typeDefaultValue = SAGE.Types.Angle.DefaultValue;
                            attribute.Value += (FileHelper.GetFloat(relocationOffset, bin) * Types.Constants.DEGREES_PER_RAD).ToString() + "d";
                            break;
                        case Percentage:
                            typeDefaultValue = SAGE.Types.Percentage.DefaultValue;
                            attribute.Value += (FileHelper.GetFloat(relocationOffset, bin) * 100).ToString() + "%";
                            break;
                        case SageBool:
                            typeDefaultValue = SAGE.Types.SageBool.DefaultValue;
                            attribute.Value += FileHelper.GetBool(relocationOffset, bin).ToString();
                            break;
                        case SageInt:
                            typeDefaultValue = SAGE.Types.SageInt.DefaultValue;
                            attribute.Value += FileHelper.GetInt(relocationOffset, bin).ToString();
                            break;
                        case SageReal:
                            typeDefaultValue = SAGE.Types.SageReal.DefaultValue;
                            attribute.Value += FileHelper.GetFloat(relocationOffset, bin).ToString();
                            break;
                        case SageUnsignedInt:
                            typeDefaultValue = SAGE.Types.SageUnsignedInt.DefaultValue;
                            attribute.Value += FileHelper.GetUInt(relocationOffset, bin).ToString();
                            break;
                        case SageUnsignedShort:
                            typeDefaultValue = SAGE.Types.SageUnsignedShort.DefaultValue;
                            attribute.Value += FileHelper.GetUShort(relocationOffset, bin).ToString();
                            break;
                        case Time:
                            typeDefaultValue = SAGE.Types.Time.DefaultValue;
                            attribute.Value += FileHelper.GetFloat(relocationOffset, bin).ToString() + "s";
                            break;
                        case Velocity:
                            typeDefaultValue = SAGE.Types.Velocity.DefaultValue;
                            attribute.Value += (FileHelper.GetFloat(relocationOffset, bin) * Types.Constants.LOGICFRAMES_PER_SECONDS_REAL).ToString();
                            break;
                        case DurationUnsignedInt:
                            typeDefaultValue = SAGE.Types.DurationUnsignedInt.DefaultValue;
                            attribute.Value += (FileHelper.GetUInt(relocationOffset, bin) * Types.Constants.MSEC_PER_LOGICFRAMES_REAL).ToString();
                            break;
                    }
                    entryNode.Attributes.Append(attribute);
                }
                else if (entryType == typeof(EntryListType))
                {
                    int listLength = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    int listOffset = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    if (listOffset == 0)
                    {
                        return;
                    }
                    EntryListType entry = baseEntry as EntryListType;
                    for (int idx = 0; idx < listLength; ++idx)
                    {
                        switch (entry.AssetType)
                        {
                            case String:
                                int stringLength = FileHelper.GetInt(listOffset, bin);
                                listOffset += 4;
                                int stringOffset = FileHelper.GetInt(listOffset, bin);
                                listOffset += 4;
                                if (stringOffset != 0)
                                {
                                    attribute.Value += FileHelper.GetString(stringOffset, bin, stringLength);
                                }
                                break;
                            case Byte:
                                attribute.Value += FileHelper.GetByte(listOffset, bin);
                                ++listOffset;
                                break;
                            case Angle:
                                attribute.Value += FileHelper.GetFloat(listOffset, bin) + 'r';
                                listOffset += 4;
                                break;
                            case Percentage:
                                attribute.Value += FileHelper.GetFloat(listOffset, bin) + '%';
                                listOffset += 4;
                                break;
                            case SageBool:
                                attribute.Value += FileHelper.GetBool(listOffset, bin);
                                ++listOffset;
                                break;
                            case SageInt:
                                attribute.Value += FileHelper.GetInt(listOffset, bin);
                                listOffset += 4;
                                break;
                            case SageReal:
                                attribute.Value += FileHelper.GetFloat(listOffset, bin);
                                listOffset += 4;
                                break;
                            case SageUnsignedInt:
                                attribute.Value += FileHelper.GetUInt(listOffset, bin);
                                listOffset += 4;
                                break;
                            case SageUnsignedShort:
                                attribute.Value += FileHelper.GetUShort(listOffset, bin);
                                listOffset += 2;
                                break;
                            case Time:
                                attribute.Value += FileHelper.GetFloat(listOffset, bin) + 's';
                                listOffset += 4;
                                break;
                            case Velocity:
                                attribute.Value += FileHelper.GetFloat(listOffset, bin);
                                listOffset += 4;
                                break;
                            case DurationUnsignedInt:
                                attribute.Value += FileHelper.GetUInt(listOffset, bin);
                                listOffset += 4;
                                break;
                        }
                        if (idx < listLength - 1)
                        {
                            attribute.Value += " ";
                        }
                    }
                    entryNode.Attributes.Append(attribute);
                }
            }
            else
            {
                if (entryType == typeof(EntryPoidType))
                {
                    XmlNode element = entryNode;
                    uint poid = FileHelper.GetUInt(binPosition, bin);
                    binPosition += 4;
                    if (poid != 0)
                    {
                        element.InnerXml += string.Format("{0:X08}", poid);
                    }
                }
                else if (entryType == typeof(EntryFileType))
                {
                    int fileOffset = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    int fileLength = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    EntryFileType entry = baseEntry as EntryFileType;
                    switch (entry.AssetType)
                    {
                        case DDS:
                            byte[] imageBuffer = new byte[fileLength];
                            Array.Copy(bin, fileOffset, imageBuffer, 0, fileLength);
                            DDS.File image = new DDS.File(imageBuffer);/*
                            if (image.Content != null)
                            {
                                uint width = image.Header.Width;
                                uint height = image.Header.Height;
                                byte[] buffer = image.Content.GetColor(width, height);
                                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)(width), (int)(height));
                                int position = 0;
                                for (int idy = 0; idy < height; ++idy)
                                {
                                    for (int idx = 0; idx < width; ++idx)
                                    {
                                        bitmap.SetPixel(idx, idy, System.Drawing.Color.FromArgb(buffer[position++], buffer[position++], buffer[position++], buffer[position++]));
                                    }
                                }
                                bitmap.Save("out.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                                bitmap.Save("out.png", System.Drawing.Imaging.ImageFormat.Png);
                            }*/
                            FileStream fs = new FileStream("out.dds", FileMode.Create, FileAccess.Write);
                            BinaryWriter writer = new BinaryWriter(fs);
                            writer.Write(imageBuffer);
                            writer.Flush();
                            writer.Close();
                            fs.Close();
                            break;
                    }
                }
                else if (entryType == typeof(EntryReferenceType))
                {
                    XmlNode element = entryNode;
                    int referenceIndex = FileHelper.GetInt(binPosition, bin);
                    if (game.ManifestVersion != 5)
                    {
                        --referenceIndex;
                    }
                    bool isImport = false;
                    for (int idx = 0; idx < imp.Length; idx += 4)
                    {
                        if (binPosition == FileHelper.GetInt(idx, imp))
                        {
                            isImport = true;
                            break;
                        }
                    }
                    if (isImport)
                    {
                        bool isFound = false;
                        foreach (AssetEntry assetEntry in stream.AssetEntries)
                        {
                            if (assetEntry.TypeId == assetReferences[referenceIndex].TypeId && assetEntry.InstanceId == assetReferences[referenceIndex].InstanceId)
                            {
                                element.InnerXml = stream.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound && referencedStreams != null)
                        {
                            foreach (Stream.File file in referencedStreams)
                            {
                                foreach (AssetEntry assetEntry in file.AssetEntries)
                                {
                                    if (assetEntry.TypeId == assetReferences[referenceIndex].TypeId && assetEntry.InstanceId == assetReferences[referenceIndex].InstanceId)
                                    {
                                        element.InnerXml = file.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                        isFound = true;
                                        break;
                                    }
                                }
                                if (isFound)
                                {
                                    break;
                                }
                            }
                        }
                        if (!isFound)
                        {
                            element.InnerXml = string.Format("Asset not found in stream: [{0:X08}:{1:X08}]", assetReferences[referenceIndex].TypeId, assetReferences[referenceIndex].InstanceId);
                        }
                    }
                    binPosition += 4;
                }
                else if (entryType == typeof(EntryWeakReferenceType))
                {
                    XmlNode element = entryNode;
                    uint typeId = 0;
                    if ((baseEntry as EntryWeakReferenceType).AssetType != "BaseAssetType")
                    {
                        typeId = StringHasher.Hash((baseEntry as EntryWeakReferenceType).AssetType);
                    }
                    uint reference = FileHelper.GetUInt(binPosition, bin);
                    binPosition += 4;
                    if (reference != 0)
                    {
                        bool isFound = false;
                        foreach (AssetEntry assetEntry in stream.AssetEntries)
                        {
                            if ((typeId == 0 || assetEntry.TypeId == typeId) && assetEntry.InstanceId == reference)
                            {
                                element.InnerXml = stream.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound && referencedStreams != null)
                        {
                            foreach (Stream.File file in referencedStreams)
                            {
                                foreach (AssetEntry assetEntry in file.AssetEntries)
                                {
                                    if ((typeId == 0 || assetEntry.TypeId == typeId) && assetEntry.InstanceId == reference)
                                    {
                                        element.InnerXml = file.AssetNames[assetEntry.NameOffset].Split(':')[1];
                                        isFound = true;
                                        break;
                                    }
                                }
                                if (isFound)
                                {
                                    break;
                                }
                            }
                        }
                        if (!isFound)
                        {
                            element.InnerXml = string.Format("Asset not found in stream: [{0:X08}]", reference);
                        }
                    }
                }
                else if (entryType == typeof(EntryType))
                {
                    XmlNode element = document.CreateElement(baseEntry.id, xmlNameSpace);
                    entryNode.AppendChild(element);
                    EntryType entry = baseEntry as EntryType;
                    foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
                    {
                        if (baseAssetType.id == entry.AssetType)
                        {
                            Type assetTypeType = baseAssetType.GetType();
                            if (assetTypeType == typeof(AssetType))
                            {
                                AssetType assetType = baseAssetType as AssetType;
                                foreach (BaseEntryType assetEntry in assetType.Entries)
                                {
                                    DecompileEntry(stream, referencedStreams, assetEntry, document, element, assetReferences, bin, imp, relo, ref binPosition);
                                }
                            }
                            return;
                        }
                    }
                    switch (entry.AssetType)
                    {
                        case String:
                            int stringLength = FileHelper.GetInt(binPosition, bin);
                            binPosition += 4;
                            int stringOffset = FileHelper.GetInt(binPosition, bin);
                            binPosition += 4;
                            if (stringOffset != 0)
                            {
                                element.InnerXml = FileHelper.GetString(stringOffset, bin, stringLength);
                            }
                            break;
                        case Byte:
                            element.InnerXml = FileHelper.GetByte(binPosition, bin).ToString();
                            ++binPosition;
                            break;
                        case Angle:
                            element.InnerXml = (FileHelper.GetFloat(binPosition, bin) * Types.Constants.DEGREES_PER_RAD).ToString() + "d";
                            binPosition += 4;
                            break;
                        case Percentage:
                            element.InnerXml = (FileHelper.GetFloat(binPosition, bin) * 100).ToString() + "%";
                            binPosition += 4;
                            break;
                        case SageBool:
                            element.InnerXml = FileHelper.GetBool(binPosition, bin).ToString();
                            ++binPosition;
                            break;
                        case SageInt:
                            element.InnerXml = FileHelper.GetInt(binPosition, bin).ToString();
                            binPosition += 4;
                            break;
                        case SageReal:
                            element.InnerXml = FileHelper.GetFloat(binPosition, bin).ToString();
                            binPosition += 4;
                            break;
                        case SageUnsignedInt:
                            element.InnerXml = FileHelper.GetUInt(binPosition, bin).ToString();
                            binPosition += 4;
                            break;
                        case SageUnsignedShort:
                            element.InnerXml = FileHelper.GetUShort(binPosition, bin).ToString();
                            binPosition += 2;
                            break;
                        case Time:
                            element.InnerXml = FileHelper.GetFloat(binPosition, bin).ToString() + "s";
                            binPosition += 4;
                            break;
                        case Velocity:
                            element.InnerXml = (FileHelper.GetFloat(binPosition, bin) * Types.Constants.LOGICFRAMES_PER_SECONDS_REAL).ToString();
                            binPosition += 4;
                            break;
                        case DurationUnsignedInt:
                            element.InnerXml = (FileHelper.GetUInt(binPosition, bin) * Types.Constants.MSEC_PER_LOGICFRAMES_REAL).ToString();
                            binPosition += 4;
                            break;
                    }
                }
                else if (entryType == typeof(EntryRelocationType))
                {
                    int relocationOffset = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    if (relocationOffset == 0)
                    {
                        return;
                    }
                    XmlNode element = document.CreateElement(baseEntry.id, xmlNameSpace);
                    entryNode.AppendChild(element);
                    EntryRelocationType entry = baseEntry as EntryRelocationType;
                    foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
                    {
                        if (baseAssetType.id == entry.AssetType)
                        {
                            Type assetTypeType = baseAssetType.GetType();
                            if (assetTypeType == typeof(EnumAssetType))
                            {
                                EnumAssetType asset = baseAssetType as EnumAssetType;
                                element.InnerText += asset.GetValue(FileHelper.GetUInt(relocationOffset, bin));
                                relocationOffset += 4;
                            }
                            else if (assetTypeType == typeof(FlagsAssetType))
                            {
                                FlagsAssetType asset = baseAssetType as FlagsAssetType;
                                int numSpans = asset.NumSpans(game);
                                uint[] flags = new uint[numSpans];
                                for (int idx = 0; idx < numSpans; ++idx)
                                {
                                    flags[idx] = FileHelper.GetUInt(relocationOffset, bin);
                                    relocationOffset += 4;
                                }
                                StringBuilder flagsStringBuilder = new StringBuilder();
                                for (int idx = 0; idx < numSpans; ++idx)
                                {
                                    for (int idy = 0; idy < FlagsAssetType.BitsInSpan; ++idy)
                                    {
                                        if ((flags[idx] & (1 << idy)) != 0)
                                        {
                                            flagsStringBuilder.Append(asset.GetValue(idx, idy, game));
                                            flagsStringBuilder.Append(" ");
                                        }
                                    }
                                }
                                if (flagsStringBuilder.Length != 0)
                                {
                                    flagsStringBuilder.Remove(flagsStringBuilder.Length - 1, 1);
                                }
                                element.InnerText += flagsStringBuilder.ToString();
                            }
                            else if (assetTypeType == typeof(AssetType))
                            {
                                AssetType assetType = baseAssetType as AssetType;
                                foreach (BaseEntryType assetEntry in assetType.Entries)
                                {
                                    DecompileEntry(stream, referencedStreams, assetEntry, document, element, assetReferences, bin, imp, relo, ref relocationOffset);
                                }
                            }
                            return;
                        }
                    }
                    string typeDefaultValue = string.Empty;
                    switch (entry.AssetType)
                    {
                        case String:
                            int stringLength = FileHelper.GetInt(relocationOffset, bin);
                            relocationOffset += 4;
                            int stringOffset = FileHelper.GetInt(relocationOffset, bin);
                            if (stringOffset != 0)
                            {
                                element.InnerXml = FileHelper.GetString(stringOffset, bin, stringLength);
                            }
                            break;
                        case Byte:
                            typeDefaultValue = "0";
                            element.InnerXml += FileHelper.GetByte(relocationOffset, bin).ToString();
                            break;
                        case Angle:
                            typeDefaultValue = SAGE.Types.Angle.DefaultValue;
                            element.InnerXml += (FileHelper.GetFloat(relocationOffset, bin) * Types.Constants.DEGREES_PER_RAD).ToString() + "d";
                            break;
                        case Percentage:
                            typeDefaultValue = SAGE.Types.Percentage.DefaultValue;
                            element.InnerXml += (FileHelper.GetFloat(relocationOffset, bin) * 100).ToString() + "%";
                            break;
                        case SageBool:
                            typeDefaultValue = SAGE.Types.SageBool.DefaultValue;
                            element.InnerXml += FileHelper.GetBool(relocationOffset, bin).ToString();
                            break;
                        case SageInt:
                            typeDefaultValue = SAGE.Types.SageInt.DefaultValue;
                            element.InnerXml += FileHelper.GetInt(relocationOffset, bin).ToString();
                            break;
                        case SageReal:
                            typeDefaultValue = SAGE.Types.SageReal.DefaultValue;
                            element.InnerXml += FileHelper.GetFloat(relocationOffset, bin).ToString();
                            break;
                        case SageUnsignedInt:
                            typeDefaultValue = SAGE.Types.SageUnsignedInt.DefaultValue;
                            element.InnerXml += FileHelper.GetUInt(relocationOffset, bin).ToString();
                            break;
                        case SageUnsignedShort:
                            typeDefaultValue = SAGE.Types.SageUnsignedShort.DefaultValue;
                            element.InnerXml += FileHelper.GetUShort(relocationOffset, bin).ToString();
                            break;
                        case Time:
                            typeDefaultValue = SAGE.Types.Time.DefaultValue;
                            element.InnerXml += FileHelper.GetFloat(relocationOffset, bin).ToString() + "s";
                            break;
                        case Velocity:
                            typeDefaultValue = SAGE.Types.Velocity.DefaultValue;
                            element.InnerXml += (FileHelper.GetFloat(relocationOffset, bin) * Types.Constants.LOGICFRAMES_PER_SECONDS_REAL).ToString();
                            break;
                        case DurationUnsignedInt:
                            typeDefaultValue = SAGE.Types.DurationUnsignedInt.DefaultValue;
                            element.InnerXml += (FileHelper.GetUInt(relocationOffset, bin) * Types.Constants.MSEC_PER_LOGICFRAMES_REAL).ToString();
                            break;
                    }
                }
                else if (entryType == typeof(EntryListType))
                {
                    int listLength = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    int listOffset = FileHelper.GetInt(binPosition, bin);
                    binPosition += 4;
                    if (listOffset == 0)
                    {
                        return;
                    }
                    EntryListType entry = baseEntry as EntryListType;
                    foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
                    {
                        if (baseAssetType.id == entry.AssetType)
                        {
                            Type assetTypeType = baseAssetType.GetType();
                            for (int idx = 0; idx < listLength; ++idx)
                            {
                                XmlNode element = document.CreateElement(baseEntry.id, xmlNameSpace);
                                entryNode.AppendChild(element);
                                if (assetTypeType == typeof(EnumAssetType))
                                {
                                    EnumAssetType asset = baseAssetType as EnumAssetType;
                                    element.InnerText += asset.GetValue(FileHelper.GetUInt(listOffset, bin));
                                    listOffset += 4;
                                }
                                else if (assetTypeType == typeof(FlagsAssetType))
                                {
                                    FlagsAssetType asset = baseAssetType as FlagsAssetType;
                                    int numSpans = asset.NumSpans(game);
                                    uint[] flags = new uint[numSpans];
                                    for (int idy = 0; idy < numSpans; ++idy)
                                    {
                                        flags[idy] = FileHelper.GetUInt(listOffset, bin);
                                        listOffset += 4;
                                    }
                                    StringBuilder flagsStringBuilder = new StringBuilder();
                                    for (int idy = 0; idy < numSpans; ++idy)
                                    {
                                        for (int idz = 0; idz < FlagsAssetType.BitsInSpan; ++idz)
                                        {
                                            if ((flags[idy] & (1 << idz)) != 0)
                                            {
                                                flagsStringBuilder.Append(asset.GetValue(idy, idz, game));
                                                flagsStringBuilder.Append(" ");
                                            }
                                        }
                                    }
                                    if (flagsStringBuilder.Length != 0)
                                    {
                                        flagsStringBuilder.Remove(flagsStringBuilder.Length - 1, 1);
                                    }
                                    element.InnerText += flagsStringBuilder.ToString();
                                }
                                else if (assetTypeType == typeof(AssetType))
                                {
                                    AssetType assetType = baseAssetType as AssetType;
                                    foreach (BaseEntryType assetEntry in assetType.Entries)
                                    {
                                        DecompileEntry(stream, referencedStreams, assetEntry, document, element, assetReferences, bin, imp, relo, ref listOffset);
                                    }
                                }
                            }
                            return;
                        }
                    }
                    for (int idx = 0; idx < listLength; ++idx)
                    {
                        XmlNode element = document.CreateElement(baseEntry.id, xmlNameSpace);
                        entryNode.AppendChild(element);
                        switch (entry.AssetType)
                        {
                            case String:
                                int stringLength = FileHelper.GetInt(listOffset, bin);
                                listOffset += 4;
                                int stringOffset = FileHelper.GetInt(listOffset, bin);
                                listOffset += 4;
                                if (stringOffset != 0)
                                {
                                    element.InnerXml = FileHelper.GetString(stringOffset, bin, stringLength);
                                }
                                break;
                            case Byte:
                                element.InnerXml = FileHelper.GetByte(listOffset, bin).ToString();
                                ++listOffset;
                                break;
                            case Angle:
                                element.InnerXml = (FileHelper.GetFloat(listOffset, bin) * Types.Constants.DEGREES_PER_RAD).ToString() + "d";
                                listOffset += 4;
                                break;
                            case Percentage:
                                element.InnerXml = (FileHelper.GetFloat(listOffset, bin) * 100).ToString() + "%";
                                listOffset += 4;
                                break;
                            case SageBool:
                                element.InnerXml = FileHelper.GetBool(listOffset, bin).ToString();
                                ++listOffset;
                                break;
                            case SageInt:
                                element.InnerXml = FileHelper.GetInt(listOffset, bin).ToString();
                                listOffset += 4;
                                break;
                            case SageReal:
                                element.InnerXml = FileHelper.GetFloat(listOffset, bin).ToString();
                                listOffset += 4;
                                break;
                            case SageUnsignedInt:
                                element.InnerXml = FileHelper.GetUInt(listOffset, bin).ToString();
                                listOffset += 4;
                                break;
                            case SageUnsignedShort:
                                element.InnerXml = FileHelper.GetUShort(listOffset, bin).ToString();
                                listOffset += 2;
                                break;
                            case Time:
                                element.InnerXml = FileHelper.GetFloat(listOffset, bin).ToString() + "s";
                                listOffset += 4;
                                break;
                            case Velocity:
                                element.InnerXml = (FileHelper.GetFloat(listOffset, bin) * Types.Constants.LOGICFRAMES_PER_SECONDS_REAL).ToString();
                                listOffset += 4;
                                break;
                            case DurationUnsignedInt:
                                element.InnerXml = (FileHelper.GetUInt(listOffset, bin) * Types.Constants.MSEC_PER_LOGICFRAMES_REAL).ToString();
                                listOffset += 4;
                                break;
                        }
                    }
                }
                else if (entryType == typeof(EntryChoiceType))
                {
                    EntryChoiceType entry = baseEntry as EntryChoiceType;
                    int listLength = 0;
                    int listOffset = 0;
                    if (entry.MaxLength == 1)
                    {
                        listLength = 1;
                        listOffset = binPosition;
                    }
                    else
                    {
                        listLength = FileHelper.GetInt(binPosition, bin);
                        binPosition += 4;
                        listOffset = FileHelper.GetInt(binPosition, bin);
                        binPosition += 4;
                    }
                    for (int idx = 0; idx < listLength; ++idx)
                    {
                        int objectOffset = listOffset;
                        if (entry.MaxLength != 1)
                        {
                            objectOffset = FileHelper.GetInt(listOffset, bin);
                            listOffset += 4;
                        }
                        uint typeHash = FileHelper.GetUInt(objectOffset, bin);
                        objectOffset += 4;
                        foreach (EntryType assetEntry in entry.Entries)
                        {
                            if (StringHasher.Hash(assetEntry.AssetType) == typeHash)
                            {
                                DecompileEntry(stream, referencedStreams, assetEntry, document, entryNode, assetReferences, bin, imp, relo, ref objectOffset);
                                break;
                            }
                        }
                    }
                }
                else if (entryType == typeof(EntryInheritanceType))
                {
                    EntryInheritanceType entry = baseEntry as EntryInheritanceType;
                    bool isFound = false;
                    foreach (GameAssetType gameAssetType in game.Assets.GameAssetTypes)
                    {
                        if (gameAssetType.id == entry.AssetType)
                        {
                            isFound = true;
                            if (gameAssetType.Entries != null)
                            {
                                foreach (BaseEntryType assetEntry in gameAssetType.Entries)
                                {
                                    DecompileEntry(stream, referencedStreams, assetEntry, document, entryNode, assetReferences, bin, imp, relo, ref binPosition);
                                }
                                if (binPosition % 4 != 0)
                                {
                                    binPosition += 4 - (binPosition % 4);
                                }
                            }
                            break;
                        }
                    }
                    if (!isFound)
                    {
                        foreach (BaseAssetType baseAssetType in game.Assets.AssetTypes)
                        {
                            if (baseAssetType.id == entry.AssetType)
                            {
                                AssetType assetType = baseAssetType as AssetType;
                                if (assetType.Entries != null)
                                {
                                    foreach (BaseEntryType assetEntry in assetType.Entries)
                                    {
                                        DecompileEntry(stream, referencedStreams, assetEntry, document, entryNode, assetReferences, bin, imp, relo, ref binPosition);
                                    }
                                    if (binPosition % 4 != 0)
                                    {
                                        binPosition += 4 - (binPosition % 4);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool Decompile(out string output, GameDefinition game, Stream.File stream, AssetEntry asset, List<AssetReference> assetReferences,
            byte[] bin, byte[] imp, byte[] relo, List<Stream.File> referencedStreams = null)
        {
            if (bin.Length == 0)
            {
                output = "This asset is referenced from a base stream.";
                return false;
            }
            foreach (GameAssetType assetType in game.Assets.GameAssetTypes)
            {
                if (assetType.TypeHash == asset.TypeHash)
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                    AssetCompiler.game = game;
                    StringBuilder stringBuilder = new StringBuilder();
                    XmlWriterSettings writerSettings = new XmlWriterSettings();
                    writerSettings.Encoding = Encoding.UTF8;
                    writerSettings.Indent = true;
                    writerSettings.IndentChars = "\t";
                    writerSettings.NewLineOnAttributes = true;
                    XmlWriter writer = XmlWriter.Create(stringBuilder, writerSettings);
                    XmlDocument document = new XmlDocument();
                    XmlNode root = document.CreateElement("AssetDeclaration", xmlNameSpace);
                    document.AppendChild(root);
                    XmlNode xmlAsset = document.CreateElement(assetType.id, xmlNameSpace);
                    root.AppendChild(xmlAsset);
                    XmlAttribute attribute = document.CreateAttribute("id");
                    xmlAsset.Attributes.Append(attribute);
                    attribute.Value = stream.AssetNames[asset.NameOffset].Split(':')[1];

                    int binPosition = 4;
                    if (assetType.Runtime == null)
                    {
                        foreach (BaseEntryType entry in assetType.Entries)
                        {
                            DecompileEntry(stream, referencedStreams, entry, document, xmlAsset, assetReferences, bin, imp, relo, ref binPosition);
                        }
                    }
                    else
                    {
                        bool isRuntimeExport = true;
                        // handle special cases first
                        if (assetType.HasSpecialCompileHandling)
                        {
                            string exeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider(
                                new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
                            System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
                            parameters.GenerateExecutable = false;
                            parameters.GenerateInMemory = true;
#if DEBUG
                            parameters.IncludeDebugInformation = true;
#endif
                            parameters.CompilerOptions = "-unsafe";
                            parameters.ReferencedAssemblies.Add("System.dll");
                            parameters.ReferencedAssemblies.Add("System.Xml.dll");
                            parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "Files.dll");
                            parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "Dds.dll");
                            parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "SAGE.dll");
                            parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "SAGE.Compiler.dll");
                            parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "SAGE.Stream.dll");
                            System.CodeDom.Compiler.CompilerResults results = provider.CompileAssemblyFromFile(parameters,
                                string.Format("{0}{1}Games{1}{2}{1}{3}.cs", exeLocation, Path.DirectorySeparatorChar, game.id, assetType.id));
                            if (!results.Errors.HasErrors)
                            {
                                System.Reflection.Assembly assembly = results.CompiledAssembly;
                                Type[] handlerTypes = assembly.GetTypes();
                                foreach (Type handlerType in handlerTypes)
                                {
                                    if (handlerType.IsSubclassOf(typeof(CompileHandler)))
                                    {
                                        CompileHandler handler = assembly.CreateInstance(handlerType.FullName) as CompileHandler;
                                        if (handler.HasSpecialDecompileHandling())
                                        {
                                            isRuntimeExport = false;
                                            handler.Decompile(stream, referencedStreams, document, xmlAsset, assetReferences, bin, imp, relo);
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                int i = 1;
                            }
                        }
                        // end - handle special cases first
                        if (isRuntimeExport)
                        {
                            XmlComment comment = document.CreateComment(" Runtime Export! ");
                            root.InsertBefore(comment, xmlAsset);
                            foreach (BaseEntryType entry in assetType.Runtime.Entries)
                            {
                                DecompileEntry(stream, referencedStreams, entry, document, xmlAsset, assetReferences, bin, imp, relo, ref binPosition);
                            }
                        }
                    }
                    document.Save(writer);
                    stringBuilder.Replace("utf-16", "utf-8");
                    output = stringBuilder.ToString();
                    return true;
                }
            }
            output = "Assets of this type are not defined. Maybe you tried to load an outdated version.";
            return false;
        }

        public static bool Compile(Uri baseUri, AssetDeclaration file, GameDefinition game, out string ErrorDescription, Func<string, bool> setAsset, bool isTempAsset = false)
        {
            if (file.Elements != null)
            {
                foreach (XmlElement element in file.Elements)
                {
                    foreach (GameAssetType assetType in game.Assets.GameAssetTypes)
                    {
                        if (assetType.id.Equals(element.LocalName))
                        {
                            if (assetType.TypeHash == 0u)
                            {
                                break;
                            }
                            Asset asset = new Asset(assetType.id, assetType.TypeHash, element.Attributes["id"].Value, StreamCompiler.GetRandom(), file.Source);
                            asset.IsNew = true;
                            setAsset(assetType.id + ":" + asset.Name);
                            int position = 4;
                            int size = assetType.GetLength(game);
                            asset.Binary = new BinaryAsset(size + 4);
                            // handle special cases first
                            if (assetType.HasSpecialCompileHandling)
                            {
                                string exeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                                Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider(
                                    new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
                                System.CodeDom.Compiler.CompilerParameters parameters = new System.CodeDom.Compiler.CompilerParameters();
                                parameters.GenerateExecutable = false;
                                parameters.GenerateInMemory = true;
#if DEBUG
                                parameters.IncludeDebugInformation = true;
#endif
                                parameters.CompilerOptions = "-unsafe";
                                parameters.ReferencedAssemblies.Add("System.dll");
                                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                                parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "Files.dll");
                                parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "Dds.dll");
                                parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "SAGE.dll");
                                parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "SAGE.Compiler.dll");
                                parameters.ReferencedAssemblies.Add(exeLocation + Path.DirectorySeparatorChar + "SAGE.Stream.dll");
                                System.CodeDom.Compiler.CompilerResults results = provider.CompileAssemblyFromFile(parameters,
                                    string.Format("{0}{1}Games{1}{2}{1}{3}.cs", exeLocation, Path.DirectorySeparatorChar, game.id, assetType.id));
                                if (results.Errors.HasErrors)
                                {
                                    System.CodeDom.Compiler.CompilerError error = results.Errors[results.Errors.Count - 1];
                                    ErrorDescription = string.Format("{0}.cs Line: {1}: {2}", assetType.id, error.Line, error.ErrorText);
                                    return false;
                                }
                                System.Reflection.Assembly assembly = results.CompiledAssembly;
                                Type[] handlerTypes = assembly.GetTypes();
                                foreach (Type handlerType in handlerTypes)
                                {
                                    if (handlerType.IsSubclassOf(typeof(CompileHandler)))
                                    {
                                        CompileHandler handler = assembly.CreateInstance(handlerType.FullName) as CompileHandler;
                                        if (!handler.Compile(assetType, baseUri, asset.Binary, element, game, asset.Name, ref position, out ErrorDescription))
                                        {
                                            return false;
                                        }
                                        break;
                                    }
                                }
                            }
                            else // end - handle special cases first
                            {
                                if (assetType.Runtime != null)
                                {
                                    if (assetType.Runtime.Entries != null)
                                    {
                                        foreach (BaseEntryType entry in assetType.Runtime.Entries)
                                        {
                                            if (!entry.Compile(baseUri, asset.Binary, element, game, asset.Name, ref position, out ErrorDescription))
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (assetType.Entries != null)
                                    {
                                        foreach (BaseEntryType entry in assetType.Entries)
                                        {
                                            if (!entry.Compile(baseUri, asset.Binary, element, game, asset.Name, ref position, out ErrorDescription))
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            if (isTempAsset)
                            {
                                TempAssets.Add(assetType.id + ":" + asset.Name, asset);
                            }
                            else
                            {
                                List<uint[]> references = new List<uint[]>();
                                asset.Binary.GatherReferences(references);
                                asset.AssetReferences.AddRange(references);
                                Assets.Add(assetType.id + ":" + asset.Name, asset);
                            }
                            break;
                        }
                    }
                }
            }
            ErrorDescription = string.Empty;
            return true;
        }
    }
}
