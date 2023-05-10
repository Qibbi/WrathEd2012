using Files;
using SAGE.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace SAGE.Compiler
{
    public static class StreamCompiler
    {
        private static Stream.File stream;

        private static GameDefinition game;
        private static Uri baseUri;
        private static Func<string, int, bool, bool> setTitle;
        private static Func<string, bool> setFile;
        private static Func<string, bool> setAsset;
        private static Func<double, bool, bool> setBar;

        private static Random rnd;

        static StreamCompiler()
        {
            rnd = new Random();
        }

        public static uint GetRandom()
        {
            return (uint)rnd.Next(-2147483648, 0x7FFFFFFF);
        }

        private static bool FindSuperclassReference(GameAssetType gameAssetType, List<Asset> assetList, uint[] reference)
        {
            bool isFound = false;
            while (gameAssetType.Superclass != null)
            {
                gameAssetType = gameAssetType.Superclass as GameAssetType;
                uint typeId = StringHasher.Hash(gameAssetType.id);
                foreach (Asset otherAsset in assetList)
                {
                    foreach (uint[] instanceReference in otherAsset.AssetReferences)
                    {
                        if (instanceReference[0] == typeId && instanceReference[1] == reference[1])
                        {
                            reference[0] = typeId;
                            isFound = true;
                            break;
                        }
                    }
                    if (isFound)
                    {
                        break;
                    }
                }
                if (isFound)
                {
                    break;
                }
            }
            return isFound;
        }

        private static bool FindSubclassReference(GameAssetType gameAssestType, List<Asset> assetList, ref uint referenceType)
        {
            bool isFound = false;
            foreach (GameAssetType subGameAssetType in gameAssestType.Subclasses)
            {
                uint typeId = StringHasher.Hash(subGameAssetType.id);
                foreach (Asset otherAsset in assetList)
                {
                    if (otherAsset.TypeId == typeId)
                    {
                        referenceType = typeId;
                        isFound = true;
                        break;
                    }
                }
                if (isFound)
                {
                    break;
                }
            }
            if (!isFound)
            {
                foreach (GameAssetType subGameAssetType in gameAssestType.Subclasses)
                {
                    if (FindSubclassReference(subGameAssetType, assetList, ref referenceType))
                    {
                        break;
                    }
                }
            }
            return isFound;
        }

        private static bool FindSubclassReference(GameAssetType gameAssestType, List<Stream.AssetEntry> assetList, ref uint referenceType)
        {
            bool isFound = false;
            foreach (GameAssetType subGameAssetType in gameAssestType.Subclasses)
            {
                uint typeId = StringHasher.Hash(subGameAssetType.id);
                foreach (Stream.AssetEntry otherAsset in assetList)
                {
                    if (otherAsset.TypeId == typeId)
                    {
                        referenceType = typeId;
                        isFound = true;
                        break;
                    }
                }
                if (isFound)
                {
                    break;
                }
            }
            if (!isFound)
            {
                foreach (GameAssetType subGameAssetType in gameAssestType.Subclasses)
                {
                    if (FindSubclassReference(subGameAssetType, assetList, ref referenceType))
                    {
                        break;
                    }
                }
            }
            return isFound;
        }

        public static bool MoveReferencesInfront(ref int idx, List<Asset> assetList, out string errorDescription)
        {
            Asset asset = assetList[idx];
            for (int idz = 0; idz < asset.AssetReferences.Count; ++idz)
            {
                for (int idy = idx; idy < assetList.Count; ++idy)
                {
                    if (assetList[idy].TypeId == asset.AssetReferences[idz][0] && assetList[idy].InstanceId == asset.AssetReferences[idz][1])
                    {
                        Asset referencedAsset = assetList[idy];
                        if (referencedAsset.Binary == null)
                        {
                            errorDescription = string.Format("Cannot resort {0} referenced from {1} as it would lead to a game crash. Please refernce a renamed copy.",
                                referencedAsset.Name, asset.Name);
                            return false;
                        }
                        assetList.RemoveAt(idy);
                        assetList.Insert(idx, referencedAsset);
                        if (!MoveReferencesInfront(ref idx, assetList, out errorDescription))
                        {
                            return false;
                        }
                        ++idx;
                        break;
                    }
                }
            }
            errorDescription = string.Empty;
            return true;
        }

        private static bool CompileStringHashes(Uri source, Uri outUri,
            Func<string, int, bool, bool> setTitle, Func<string, bool> setFile, Func<string, bool> setAsset, Func<double, bool, bool> setBar, Func<string, bool> setError)
        {
            bool hasNoWarning = true;
            string errorDescription;
            stream = new Stream.File(game.ManifestVersion);
            stream.Header.IsLinked = true;
            stream.Header.Checksum = GetRandom();
            stream.Header.AllTypesHash = game.AllTypesHash;
            string path = source.LocalPath;
            string file = Path.GetFileNameWithoutExtension(path);
            path = path.Substring(0, path.LastIndexOf('.'));
            string outPath = outUri.LocalPath;
            if (outPath.Contains("."))
            {
                string outFolder = outPath.Substring(outPath.IndexOf(Path.DirectorySeparatorChar));
                if (!Directory.Exists(outFolder))
                {
                    Directory.CreateDirectory(outFolder);
                }
                outPath = outPath.Substring(0, outPath.LastIndexOf('.'));
            }
            else
            {
                if (!Directory.Exists(outPath))
                {
                    Directory.CreateDirectory(outPath);
                }
                outPath += file;
            }
            List<string> processedFiles = new List<string>();
            int fileCount = 1;
            int processedFileCount = 0;
            AssetCompiler.Assets.Clear();
            if (!CompileXml(processedFiles, ref fileCount, ref processedFileCount, source, file + ".xml", out errorDescription))
            {
                setTitle("ERROR", 0, false);
                setError(errorDescription);
                setBar(0, true);
                return false;
            }

            List<byte[]> binBuffer = new List<byte[]>();
            List<byte[]> reloBuffer = new List<byte[]>();
            List<byte[]> impBuffer = new List<byte[]>();
            if (game.ManifestVersion == 7)
            {
                binBuffer.Add(new byte[] { 0x00, 0x00, 0xBB, 0xBA });
                reloBuffer.Add(new byte[] { 0x00, 0x00, 0xBE, 0xBA });
                impBuffer.Add(new byte[] { 0x00, 0x00, 0xB1, 0xBA });
            }
            byte[] checksum = new byte[4];
            FileHelper.SetUInt(stream.Header.Checksum, 0, checksum);
            binBuffer.Add(checksum);
            reloBuffer.Add(checksum);
            impBuffer.Add(checksum);

            setBar(0, false);
            setFile(file + ".manifest");
            setAsset(string.Empty);

            uint assetReferenceOffset = 0;
            uint assetNameBufferSize = 0;
            uint sourceFileNameBufferSize = 0;
            uint binaryDataSize = 0;
            List<Asset> assetList = new List<Asset>();
            assetList.AddRange(AssetCompiler.Assets.Values);
            for (int idx = 0; idx < assetList.Count; ++idx)
            {
                double percent = (double)(idx) / (double)(assetList.Count) * 100;
                setTitle("Compiling Manifest", (int)percent, true);
                setBar(percent, true);
                Asset asset = assetList[idx];
                Stream.AssetEntry assetEntry = null;
                switch (game.ManifestVersion)
                {
                    case 5:
                        stream.AssetEntries.Add(assetEntry = new Stream.AssetEntry5());
                        break;
                    case 6:
                        stream.AssetEntries.Add(assetEntry = new Stream.AssetEntry6());
                        break;
                    case 7:
                        stream.AssetEntries.Add(assetEntry = new Stream.AssetEntry7());
                        break;
                }
                assetEntry.TypeId = asset.TypeId;
                assetEntry.InstanceId = asset.InstanceId;
                assetEntry.TypeHash = asset.TypeHash;
                assetEntry.InstanceHash = asset.InstanceHash;
                assetEntry.AssetReferenceOffset = assetReferenceOffset;
                assetEntry.AssetReferenceCount = (uint)asset.AssetReferences.Count;
                for (int idy = 0; idy < asset.AssetReferences.Count; ++idy)
                {
                    byte[] assetReference = new byte[8];
                    FileHelper.SetUInt(asset.AssetReferences[idy][0], 0, assetReference);
                    FileHelper.SetUInt(asset.AssetReferences[idy][1], 4, assetReference);
                    stream.AssetReferences.Add(new Stream.AssetReference(assetReference));
                }
                assetReferenceOffset += (uint)asset.AssetReferences.Count << 3;
                assetEntry.NameOffset = assetNameBufferSize;
                string name = asset.Type + ':' + asset.Name;
                stream.AssetNames.Add(assetEntry.NameOffset, name);
                assetNameBufferSize += (uint)(name.Length + 1);
                if (stream.SourceFiles.ContainsValue(asset.SourceFile))
                {
                    foreach (KeyValuePair<uint, string> sourceFile in stream.SourceFiles)
                    {
                        if (sourceFile.Value == asset.SourceFile)
                        {
                            assetEntry.SourceFileNameOffset = sourceFile.Key;
                            break;
                        }
                    }
                }
                else
                {
                    assetEntry.SourceFileNameOffset = sourceFileNameBufferSize;
                    stream.SourceFiles.Add(assetEntry.SourceFileNameOffset, asset.SourceFile);
                    sourceFileNameBufferSize += (uint)(asset.SourceFile.Length + 1);
                }
                assetEntry.IsTokenized = asset.IsTokenized;
                if (asset.Binary != null)
                {
                    List<byte[]> binary = new List<byte[]>();
                    List<int> imports = new List<int>();
                    List<uint[]> importAssets = new List<uint[]>();
                    List<int> relocations = new List<int>();
                    int binPosition = 0;
                    int binLength = asset.Binary.Content.Length;
                    asset.Binary.GetBinary(game.ManifestVersion, binary, imports, importAssets, relocations, ref binPosition, ref binLength);
                    assetEntry.BinaryDataSize = (uint)binLength;
                    if (assetEntry.BinaryDataSize > stream.Header.MaxBinaryChunkSize)
                    {
                        stream.Header.MaxBinaryChunkSize = assetEntry.BinaryDataSize;
                    }
                    binBuffer.AddRange(binary);
                    binaryDataSize += (uint)binLength;
                    if (relocations.Count != 0)
                    {
                        assetEntry.RelocationsDataSize = (uint)((relocations.Count + 1) << 2);
                        byte[] relocationsBuffer = new byte[assetEntry.RelocationsDataSize];
                        for (int idy = 0; idy < relocations.Count; ++idy)
                        {
                            FileHelper.SetInt(relocations[idy], idy << 2, relocationsBuffer);
                        }
                        FileHelper.SetInt(-1, relocations.Count << 2, relocationsBuffer);
                        if (assetEntry.RelocationsDataSize > stream.Header.MaxRelocationsChunkSize)
                        {
                            stream.Header.MaxRelocationsChunkSize = assetEntry.RelocationsDataSize;
                        }
                        reloBuffer.Add(relocationsBuffer);
                    }
                    if (imports.Count != 0)
                    {
                        assetEntry.ImportsDataSize = (uint)((imports.Count + 1) << 2);
                        byte[] importsBuffer = new byte[assetEntry.ImportsDataSize];
                        for (int idy = 0; idy < imports.Count; ++idy)
                        {
                            FileHelper.SetInt(imports[idy], idy << 2, importsBuffer);
                        }
                        FileHelper.SetInt(-1, imports.Count << 2, importsBuffer);
                        if (assetEntry.ImportsDataSize > stream.Header.MaxImportsChunkSize)
                        {
                            stream.Header.MaxImportsChunkSize = assetEntry.ImportsDataSize;
                        }
                        impBuffer.Add(importsBuffer);
                    }
                    if (asset.Binary.CData != null)
                    {
                        List<byte[]> cdataBinary = new List<byte[]>();
                        List<int> cdataImports = new List<int>();
                        List<uint[]> cdataImportAssets = new List<uint[]>();
                        List<int> cdataRelocations = new List<int>();
                        int cdataBinPosition = 0;
                        int cdataBinLength = asset.Binary.CData.Content.Length;
                        asset.Binary.CData.GetBinary(game.ManifestVersion, cdataBinary, cdataImports, cdataImportAssets, cdataRelocations, ref cdataBinPosition, ref cdataBinLength);
                        byte[] cdataHead = null;
                        if (asset.Binary.IsWritingCDataHead)
                        {
                            cdataHead = new byte[0x20];
                            FileHelper.SetUInt(asset.TypeId, 0x00, cdataHead);
                            FileHelper.SetUInt(asset.TypeHash, 0x04, cdataHead);
                            FileHelper.SetUInt(asset.InstanceId, 0x08, cdataHead);
                            FileHelper.SetUInt(asset.InstanceHash, 0x0C, cdataHead);
                            FileHelper.SetInt(cdataBinLength, 0x10, cdataHead);
                            if (cdataRelocations.Count != 0)
                            {
                                FileHelper.SetInt((cdataRelocations.Count << 2) + 4, 0x14, cdataHead);
                            }
                            if (cdataImports.Count != 0)
                            {
                                FileHelper.SetInt((cdataImports.Count << 2) + 4, 0x18, cdataHead);
                            }
                            if (cdataImportAssets.Count != 0)
                            {
                                FileHelper.SetInt(cdataImportAssets.Count << 3, 0x1C, cdataHead);
                            }
                        }
                        if (!Directory.Exists(string.Format("{0}{1}{2}cdata", outPath, string.Empty, Path.DirectorySeparatorChar)))
                        {
                            Directory.CreateDirectory(string.Format("{0}{1}{2}cdata", outPath, string.Empty, Path.DirectorySeparatorChar));
                        }
                        using (FileStream cdataFile = new FileStream(
                            string.Format("{0}{1}{2}cdata{2}{3:x08}.{4:x08}.{5:x08}.{6:x08}.cdata", outPath, string.Empty, Path.DirectorySeparatorChar,
                            asset.TypeId, asset.TypeHash, asset.InstanceId, asset.InstanceHash),
                            FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (BinaryWriter cdataWriter = new BinaryWriter(cdataFile))
                            {
                                if (asset.Binary.IsWritingCDataHead)
                                {
                                    cdataWriter.Write(cdataHead);
                                }
                                foreach (byte[] cdataBin in cdataBinary)
                                {
                                    cdataWriter.Write(cdataBin);
                                }
                                if (cdataRelocations.Count != 0)
                                {
                                    foreach (int cdataRelo in cdataRelocations)
                                    {
                                        cdataWriter.Write(cdataRelo);
                                    }
                                    cdataWriter.Write(-1);
                                }
                                if (cdataImports.Count != 0)
                                {
                                    foreach (int cdataImp in cdataImports)
                                    {
                                        cdataWriter.Write(cdataImp);
                                    }
                                    cdataWriter.Write(-1);
                                }
                                if (cdataImportAssets.Count != 0)
                                {
                                    foreach (uint[] cdataImp in cdataImportAssets)
                                    {
                                        cdataWriter.Write(cdataImp[0]);
                                        cdataWriter.Write(cdataImp[1]);
                                    }
                                }
                                cdataWriter.Flush();
                            }
                        }
                    }
                }
            }
            stream.Header.AssetCount = (uint)assetList.Count;
            stream.Header.BinaryDataSize = binaryDataSize;
            stream.Header.AssetReferenceBufferSize = assetReferenceOffset;
            stream.Header.AssetNameBufferSize = assetNameBufferSize;
            stream.Header.SourceFileNameBufferSize = sourceFileNameBufferSize;

            setTitle("Writing...", 0, false);
            using (FileStream manifestFile = new FileStream(string.Format("{0}{1}.manifest", outPath, string.Empty), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter manifestWriter = new BinaryWriter(manifestFile))
                {
                    stream.Write(manifestWriter);
                    manifestWriter.Flush();
                }
            }
            setFile(file + ".bin");
            using (FileStream binFile = new FileStream(string.Format("{0}{1}.bin", outPath, string.Empty), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter binWriter = new BinaryWriter(binFile))
                {
                    foreach (byte[] bin in binBuffer)
                    {
                        binWriter.Write(bin);
                    }
                    binWriter.Flush();
                }
            }
            setFile(file + ".relo");
            using (FileStream reloFile = new FileStream(string.Format("{0}{1}.relo", outPath, string.Empty), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter reloWriter = new BinaryWriter(reloFile))
                {
                    foreach (byte[] relo in reloBuffer)
                    {
                        reloWriter.Write(relo);
                    }
                    reloWriter.Flush();
                }
            }
            setFile(file + ".imp");
            using (FileStream impFile = new FileStream(string.Format("{0}{1}.imp", outPath, string.Empty), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter impWriter = new BinaryWriter(impFile))
                {
                    foreach (byte[] imp in impBuffer)
                    {
                        impWriter.Write(imp);
                    }
                    impWriter.Flush();
                }
            }
            return hasNoWarning;
        }

        public static bool CompileStream(GameDefinition game, Uri source, Uri baseUri, Uri outUri, string version, string basePatchStream, bool isMapCompile, string lLod, string mLod,
            Func<string, int, bool, bool> setTitle, Func<string, bool> setFile, Func<string, bool> setAsset, Func<double, bool, bool> setBar, Func<string, bool> setError, bool noStringHashes)
        {
            bool hasNoWarning = true;
            string ErrorDescription;
            stream = new Stream.File(game.ManifestVersion);
            stream.Header.IsLinked = true;
            stream.Header.Checksum = GetRandom();
            stream.Header.AllTypesHash = game.AllTypesHash;

            StreamCompiler.game = game;
            StreamCompiler.baseUri = baseUri;
            StreamCompiler.setTitle = setTitle;
            StreamCompiler.setFile = setFile;
            StreamCompiler.setAsset = setAsset;
            StreamCompiler.setBar = setBar;

            List<Asset> assetList = new List<Asset>();

            string path = source.LocalPath;
            string file = Path.GetFileNameWithoutExtension(path);
            string outPath = outUri.LocalPath;
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            outPath += file;

            List<string> processedFiles = new List<string>();
            int fileCount = 1;
            int processedFileCount = 0;
            if (!CompileXml(processedFiles, ref fileCount, ref processedFileCount, source, file + ".xml", out ErrorDescription))
            {
                setTitle("ERROR", 0, false);
                setError(ErrorDescription);
                setBar(0, true);
                return false;
            }

            List<byte[]> binBuffer = new List<byte[]>();
            List<byte[]> reloBuffer = new List<byte[]>();
            List<byte[]> impBuffer = new List<byte[]>();
            if (game.ManifestVersion == 7)
            {
                binBuffer.Add(new byte[] { 0x00, 0x00, 0xBB, 0xBA });
                reloBuffer.Add(new byte[] { 0x00, 0x00, 0xBE, 0xBA });
                impBuffer.Add(new byte[] { 0x00, 0x00, 0xB1, 0xBA });
            }
            byte[] checksum = new byte[4];
            FileHelper.SetUInt(stream.Header.Checksum, 0, checksum);
            binBuffer.Add(checksum);
            reloBuffer.Add(checksum);
            impBuffer.Add(checksum);

            setBar(0, false);
            setFile(file + ".manifest");
            setAsset(string.Empty);
            if (isMapCompile)
            {
                setTitle("Patching...", 0, false);
                string versionString = string.Format("worldbuilder{0}.manifest", game.WorldBuilderVersion);
                stream.StreamReferences.Insert(0, new Stream.StreamReference(Stream.StreamReferenceType.PATCH, versionString));
                stream.Header.ReferencedManifestNameBufferSize += (uint)(versionString.Length + 2);
                Stream.File worldbuilderStream = null;
                string worldbuilderStreamPath = string.Format("{0}{1}worldbuilder{2}.manifest", game.DefinitionPath, Path.DirectorySeparatorChar, game.WorldBuilderVersion);
                if (File.Exists(worldbuilderStreamPath))
                {
                    using (FileStream worldbuilderFile = new FileStream(worldbuilderStreamPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader worldbuilderReader = new BinaryReader(worldbuilderFile))
                        {
                            worldbuilderStream = new Stream.File(worldbuilderReader.ReadBytes((int)(worldbuilderFile.Length)));
                        }
                    }
                }
                stream.Header.BinaryDataSize = worldbuilderStream.Header.BinaryDataSize;
                stream.Header.MaxBinaryChunkSize = worldbuilderStream.Header.MaxBinaryChunkSize;
                stream.Header.MaxImportsChunkSize = worldbuilderStream.Header.MaxImportsChunkSize;
                stream.Header.MaxRelocationsChunkSize = worldbuilderStream.Header.MaxRelocationsChunkSize;
                for (int idx = worldbuilderStream.AssetEntries.Count - 1; idx >= 0; --idx)
                {
                    string name = worldbuilderStream.AssetNames[worldbuilderStream.AssetEntries[idx].NameOffset];
                    if (AssetCompiler.Assets.ContainsKey(name))
                    {
                        Asset asset = AssetCompiler.Assets[name];
                        AssetCompiler.Assets.Remove(name);
                        assetList.Insert(0, asset);
                    }
                    else
                    {
                        Stream.AssetEntry assetEntry = worldbuilderStream.AssetEntries[idx];
                        bool isFound = false;
                        foreach (Asset streamAsset in assetList)
                        {
                            foreach (uint[] assetReference in streamAsset.AssetReferences)
                            {
                                if (assetEntry.TypeId == assetReference[0] && assetEntry.InstanceId == assetReference[1])
                                {
                                    isFound = true;
                                    break;
                                }
                            }
                            if (isFound)
                            {
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            foreach (Asset streamAsset in AssetCompiler.Assets.Values)
                            {
                                foreach (uint[] assetReference in streamAsset.AssetReferences)
                                {
                                    if (assetEntry.TypeId == assetReference[0] && assetEntry.InstanceId == assetReference[1])
                                    {
                                        isFound = true;
                                        break;
                                    }
                                }
                                if (!isFound)
                                {
                                    foreach (uint[] assetReference in streamAsset.WeakAssetReferences)
                                    {
                                        if (assetEntry.TypeId == assetReference[0] && assetEntry.InstanceId == assetReference[1])
                                        {
                                            isFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (!isFound)
                                {
                                    uint[] reference = new uint[] { assetEntry.TypeId, assetEntry.InstanceId };
                                    foreach (GameAssetType gameAssestType in game.Assets.GameAssetTypes)
                                    {
                                        if (StringHasher.Hash(gameAssestType.id) == reference[0])
                                        {
                                            isFound = FindSuperclassReference(gameAssestType, assetList, reference);
                                            break;
                                        }
                                    }
                                    if (!isFound)
                                    {
                                        List<Asset> compiledList = new List<Asset>();
                                        compiledList.AddRange(AssetCompiler.Assets.Values);
                                        foreach (GameAssetType gameAssestType in game.Assets.GameAssetTypes)
                                        {
                                            if (StringHasher.Hash(gameAssestType.id) == reference[0])
                                            {
                                                isFound = FindSuperclassReference(gameAssestType, compiledList, reference);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (isFound)
                        {
                            string[] typeName = name.Split(':');
                            Asset asset = new Asset(typeName[0],
                                worldbuilderStream.AssetEntries[idx].TypeHash,
                                typeName[1],
                                worldbuilderStream.AssetEntries[idx].InstanceHash,
                                worldbuilderStream.SourceFiles[worldbuilderStream.AssetEntries[idx].SourceFileNameOffset],
                                worldbuilderStream.AssetEntries[idx].IsTokenized);
                            for (uint idy = 0; idy < worldbuilderStream.AssetEntries[idx].AssetReferenceCount; ++idy)
                            {
                                Stream.AssetReference reference = worldbuilderStream.AssetReferences[(int)((worldbuilderStream.AssetEntries[idx].AssetReferenceOffset >> 3) + idy)];
                                asset.AssetReferences.Add(new uint[] { reference.TypeId, reference.InstanceId });
                            }
                            assetList.Insert(0, asset);
                        }
                    }
                }
            }
            else if (basePatchStream != null)
            {
                setTitle("Reading Base Manifest...", 0, false);
                string[] bps = basePatchStream.Split(',');
                stream.StreamReferences.Insert(0, new Stream.StreamReference(Stream.StreamReferenceType.PATCH, bps[0]));
                stream.Header.ReferencedManifestNameBufferSize += (uint)(bps[0].Length + 2);
                Stream.File baseStream = null;
                if (File.Exists(bps[1]))
                {
                    using (FileStream baseFile = new FileStream(bps[1], FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader baseReader = new BinaryReader(baseFile))
                        {
                            baseStream = new Stream.File(baseReader.ReadBytes((int)(baseFile.Length)));
                        }
                    }
                }
                stream.Header.BinaryDataSize = baseStream.Header.BinaryDataSize;
                stream.Header.MaxBinaryChunkSize = baseStream.Header.MaxBinaryChunkSize;
                stream.Header.MaxImportsChunkSize = baseStream.Header.MaxImportsChunkSize;
                stream.Header.MaxRelocationsChunkSize = baseStream.Header.MaxRelocationsChunkSize;
                for (int idx = 0; idx < baseStream.AssetEntries.Count; ++idx)
                {
                    string name = baseStream.AssetNames[baseStream.AssetEntries[idx].NameOffset];
                    if (AssetCompiler.Assets.ContainsKey(name))
                    {
                        Asset asset = AssetCompiler.Assets[name];
                        AssetCompiler.Assets.Remove(name);
                        assetList.Add(asset);
                    }
                    else
                    {
                        string[] typeName = name.Split(':');
                        Asset asset = new Asset(typeName[0],
                            baseStream.AssetEntries[idx].TypeHash,
                            typeName[1],
                            baseStream.AssetEntries[idx].InstanceHash,
                            baseStream.SourceFiles[baseStream.AssetEntries[idx].SourceFileNameOffset],
                            baseStream.AssetEntries[idx].IsTokenized);
                        for (uint idy = 0; idy < baseStream.AssetEntries[idx].AssetReferenceCount; ++idy)
                        {
                            Stream.AssetReference reference = baseStream.AssetReferences[(int)((baseStream.AssetEntries[idx].AssetReferenceOffset >> 3) + idy)];
                            asset.AssetReferences.Add(new uint[] { reference.TypeId, reference.InstanceId });
                        }
                        assetList.Add(asset);
                    }
                }
            }
            assetList.AddRange(AssetCompiler.Assets.Values);
            for (int idx = 0; idx < assetList.Count; ++idx)
            {
                double percent = (double)(idx) / (double)(assetList.Count) * 100;
                setTitle("Resolving references", (int)percent, true);
                if (!assetList[idx].IsNew)
                {
                    continue;
                }
                bool isFound = false;
                if (assetList[idx].AssetReferences.Count != 0)
                {
                    foreach (uint[] reference in assetList[idx].AssetReferences)
                    {
                        isFound = false;
                        List<Asset> nameAssetList = new List<Asset>();
                        foreach (Asset otherAsset in assetList)
                        {
                            if (otherAsset.InstanceId == reference[1])
                            {
                                nameAssetList.Add(otherAsset);
                                if (otherAsset.TypeId == reference[0])
                                {
                                    isFound = true;
                                    break;
                                }
                            }
                        }
                        if (!isFound)
                        {
                            List<Stream.File> referencedStreams = new List<Stream.File>();
                            foreach (Stream.StreamReference streamReference in stream.StreamReferences)
                            {
                                if (streamReference.ReferenceType == Stream.StreamReferenceType.REFERENCE)
                                {
                                    if (streamReference.Path.StartsWith("static"))
                                    {
                                        string staticStreamPath = string.Format("{0}{1}static{2}.manifest", game.DefinitionPath, Path.DirectorySeparatorChar, game.WorldBuilderVersion);
                                        if (File.Exists(staticStreamPath))
                                        {
                                            using (FileStream staticFile = new FileStream(staticStreamPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                            {
                                                using (BinaryReader staticReader = new BinaryReader(staticFile))
                                                {
                                                    referencedStreams.Add(new Stream.File(staticReader.ReadBytes((int)(staticFile.Length))));
                                                }
                                            }
                                        }
                                    }
                                    else if (streamReference.Path.StartsWith("global"))
                                    {
                                        string globalStreamPath = string.Format("{0}{1}global{2}.manifest", game.DefinitionPath, Path.DirectorySeparatorChar, game.WorldBuilderVersion);
                                        if (File.Exists(globalStreamPath))
                                        {
                                            using (FileStream globalFile = new FileStream(globalStreamPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                            {
                                                using (BinaryReader globalReader = new BinaryReader(globalFile))
                                                {
                                                    referencedStreams.Add(new Stream.File(globalReader.ReadBytes((int)(globalFile.Length))));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (GameAssetType gameAssestType in game.Assets.GameAssetTypes)
                            {
                                if (StringHasher.Hash(gameAssestType.id) == reference[0])
                                {
                                    if (!FindSubclassReference(gameAssestType, nameAssetList, ref reference[0]))
                                    {
                                        foreach (Stream.File streamFile in referencedStreams)
                                        {
                                            if (isFound = FindSubclassReference(gameAssestType, streamFile.AssetEntries, ref reference[0]))
                                            {
                                                break;
                                            }
                                        }
                                        if (!isFound)
                                        {
                                            //setError(string.Format("Could not resolve Asset {0:X08}:{1:X08}", reference[0], reference[1]));
                                            //hasNoWarning = false;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            // Sort
            for (int idx = 0; idx < assetList.Count; ++idx)
            {
                if (assetList[idx].Binary != null)
                {
                    double percent = (double)(idx) / (double)(assetList.Count) * 100;
                    setTitle("Sorting", (int)percent, true);
                    setBar(percent, true);
                    if (!MoveReferencesInfront(ref idx, assetList, out ErrorDescription))
                    {
                        setTitle("ERROR", 0, false);
                        setError(ErrorDescription);
                        setBar(0, true);
                        return false;
                    }
                }
            }
            // end Sort
            uint assetReferenceOffset = 0;
            uint assetNameBufferSize = 0;
            uint sourceFileNameBufferSize = 0;
            uint binaryDataSize = 0;
            for (int idx = 0; idx < assetList.Count; ++idx)
            {
                double percent = (double)(idx) / (double)(assetList.Count) * 100;
                setTitle("Compiling Manifest", (int)percent, true);
                setBar(percent, true);
                Asset asset = assetList[idx];
                Stream.AssetEntry assetEntry = null;
                switch (game.ManifestVersion)
                {
                    case 5:
                        stream.AssetEntries.Add(assetEntry = new Stream.AssetEntry5());
                        break;
                    case 6:
                        stream.AssetEntries.Add(assetEntry = new Stream.AssetEntry6());
                        break;
                    case 7:
                        stream.AssetEntries.Add(assetEntry = new Stream.AssetEntry7());
                        break;
                }
                assetEntry.TypeId = asset.TypeId;
                assetEntry.InstanceId = asset.InstanceId;
                assetEntry.TypeHash = asset.TypeHash;
                assetEntry.InstanceHash = asset.InstanceHash;
                assetEntry.AssetReferenceOffset = assetReferenceOffset;
                assetEntry.AssetReferenceCount = (uint)asset.AssetReferences.Count;
                for (int idy = 0; idy < asset.AssetReferences.Count; ++idy)
                {
                    byte[] assetReference = new byte[8];
                    FileHelper.SetUInt(asset.AssetReferences[idy][0], 0, assetReference);
                    FileHelper.SetUInt(asset.AssetReferences[idy][1], 4, assetReference);
                    stream.AssetReferences.Add(new Stream.AssetReference(assetReference));
                }
                assetReferenceOffset += (uint)asset.AssetReferences.Count << 3;
                assetEntry.NameOffset = assetNameBufferSize;
                string name = asset.Type + ':' + asset.Name;
                stream.AssetNames.Add(assetEntry.NameOffset, name);
                assetNameBufferSize += (uint)(name.Length + 1);
                if (stream.SourceFiles.ContainsValue(asset.SourceFile))
                {
                    foreach (KeyValuePair<uint, string> sourceFile in stream.SourceFiles)
                    {
                        if (sourceFile.Value == asset.SourceFile)
                        {
                            assetEntry.SourceFileNameOffset = sourceFile.Key;
                            break;
                        }
                    }
                }
                else
                {
                    assetEntry.SourceFileNameOffset = sourceFileNameBufferSize;
                    stream.SourceFiles.Add(assetEntry.SourceFileNameOffset, asset.SourceFile);
                    sourceFileNameBufferSize += (uint)(asset.SourceFile.Length + 1);
                }
                assetEntry.IsTokenized = asset.IsTokenized;
                if (asset.Binary != null)
                {
                    List<byte[]> binary = new List<byte[]>();
                    List<int> imports = new List<int>();
                    List<uint[]> importAssets = new List<uint[]>();
                    List<int> relocations = new List<int>();
                    int binPosition = 0;
                    int binLength = asset.Binary.Content.Length;
                    asset.Binary.GetBinary(game.ManifestVersion, binary, imports, importAssets, relocations, ref binPosition, ref binLength);
                    assetEntry.BinaryDataSize = (uint)binLength;
                    if (assetEntry.BinaryDataSize > stream.Header.MaxBinaryChunkSize)
                    {
                        stream.Header.MaxBinaryChunkSize = assetEntry.BinaryDataSize;
                    }
                    binBuffer.AddRange(binary);
                    binaryDataSize += (uint)binLength;
                    if (relocations.Count != 0)
                    {
                        assetEntry.RelocationsDataSize = (uint)((relocations.Count + 1) << 2);
                        byte[] relocationsBuffer = new byte[assetEntry.RelocationsDataSize];
                        for (int idy = 0; idy < relocations.Count; ++idy)
                        {
                            FileHelper.SetInt(relocations[idy], idy << 2, relocationsBuffer);
                        }
                        FileHelper.SetInt(-1, relocations.Count << 2, relocationsBuffer);
                        if (assetEntry.RelocationsDataSize > stream.Header.MaxRelocationsChunkSize)
                        {
                            stream.Header.MaxRelocationsChunkSize = assetEntry.RelocationsDataSize;
                        }
                        reloBuffer.Add(relocationsBuffer);
                    }
                    if (imports.Count != 0)
                    {
                        assetEntry.ImportsDataSize = (uint)((imports.Count + 1) << 2);
                        byte[] importsBuffer = new byte[assetEntry.ImportsDataSize];
                        for (int idy = 0; idy < imports.Count; ++idy)
                        {
                            FileHelper.SetInt(imports[idy], idy << 2, importsBuffer);
                        }
                        FileHelper.SetInt(-1, imports.Count << 2, importsBuffer);
                        if (assetEntry.ImportsDataSize > stream.Header.MaxImportsChunkSize)
                        {
                            stream.Header.MaxImportsChunkSize = assetEntry.ImportsDataSize;
                        }
                        impBuffer.Add(importsBuffer);
                    }
                    if (asset.Binary.CData != null)
                    {
                        List<byte[]> cdataBinary = new List<byte[]>();
                        List<int> cdataImports = new List<int>();
                        List<uint[]> cdataImportAssets = new List<uint[]>();
                        List<int> cdataRelocations = new List<int>();
                        int cdataBinPosition = 0;
                        int cdataBinLength = asset.Binary.CData.Content.Length;
                        asset.Binary.CData.GetBinary(game.ManifestVersion, cdataBinary, cdataImports, cdataImportAssets, cdataRelocations, ref cdataBinPosition, ref cdataBinLength);
                        byte[] cdataHead = null;
                        if (asset.Binary.IsWritingCDataHead)
                        {
                            cdataHead = new byte[0x20];
                            FileHelper.SetUInt(asset.TypeId, 0x00, cdataHead);
                            FileHelper.SetUInt(asset.TypeHash, 0x04, cdataHead);
                            FileHelper.SetUInt(asset.InstanceId, 0x08, cdataHead);
                            FileHelper.SetUInt(asset.InstanceHash, 0x0C, cdataHead);
                            FileHelper.SetInt(cdataBinLength, 0x10, cdataHead);
                            if (cdataRelocations.Count != 0)
                            {
                                FileHelper.SetInt((cdataRelocations.Count << 2) + 4, 0x14, cdataHead);
                            }
                            if (cdataImports.Count != 0)
                            {
                                FileHelper.SetInt((cdataImports.Count << 2) + 4, 0x18, cdataHead);
                            }
                            if (cdataImportAssets.Count != 0)
                            {
                                FileHelper.SetInt(cdataImportAssets.Count << 3, 0x1C, cdataHead);
                            }
                        }
                        if (!Directory.Exists(string.Format("{0}{1}{2}cdata", outPath, version, Path.DirectorySeparatorChar)))
                        {
                            Directory.CreateDirectory(string.Format("{0}{1}{2}cdata", outPath, version, Path.DirectorySeparatorChar));
                        }
                        using (FileStream cdataFile = new FileStream(
                            string.Format("{0}{1}{2}cdata{2}{3:x08}.{4:x08}.{5:x08}.{6:x08}.cdata", outPath, version, Path.DirectorySeparatorChar,
                            asset.TypeId, asset.TypeHash, asset.InstanceId, asset.InstanceHash),
                            FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (BinaryWriter cdataWriter = new BinaryWriter(cdataFile))
                            {
                                if (asset.Binary.IsWritingCDataHead)
                                {
                                    cdataWriter.Write(cdataHead);
                                }
                                foreach (byte[] cdataBin in cdataBinary)
                                {
                                    cdataWriter.Write(cdataBin);
                                }
                                if (cdataRelocations.Count != 0)
                                {
                                    foreach (int cdataRelo in cdataRelocations)
                                    {
                                        cdataWriter.Write(cdataRelo);
                                    }
                                    cdataWriter.Write(-1);
                                }
                                if (cdataImports.Count != 0)
                                {
                                    foreach (int cdataImp in cdataImports)
                                    {
                                        cdataWriter.Write(cdataImp);
                                    }
                                    cdataWriter.Write(-1);
                                }
                                if (cdataImportAssets.Count != 0)
                                {
                                    foreach (uint[] cdataImp in cdataImportAssets)
                                    {
                                        cdataWriter.Write(cdataImp[0]);
                                        cdataWriter.Write(cdataImp[1]);
                                    }
                                }
                                cdataWriter.Flush();
                            }
                        }
                    }
                }
            }
            stream.Header.AssetCount = (uint)assetList.Count;
            stream.Header.BinaryDataSize += binaryDataSize;
            stream.Header.AssetReferenceBufferSize = assetReferenceOffset;
            stream.Header.AssetNameBufferSize = assetNameBufferSize;
            stream.Header.SourceFileNameBufferSize = sourceFileNameBufferSize;

            setTitle("Writing...", 0, false);
            using (FileStream manifestFile = new FileStream(string.Format("{0}{1}.manifest", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter manifestWriter = new BinaryWriter(manifestFile))
                {
                    stream.Write(manifestWriter);
                    manifestWriter.Flush();
                }
            }
            setFile(file + ".bin");
            using (FileStream binFile = new FileStream(string.Format("{0}{1}.bin", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter binWriter = new BinaryWriter(binFile))
                {
                    foreach (byte[] bin in binBuffer)
                    {
                        binWriter.Write(bin);
                    }
                    binWriter.Flush();
                }
            }
            setFile(file + ".relo");
            using (FileStream reloFile = new FileStream(string.Format("{0}{1}.relo", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter reloWriter = new BinaryWriter(reloFile))
                {
                    foreach (byte[] relo in reloBuffer)
                    {
                        reloWriter.Write(relo);
                    }
                    reloWriter.Flush();
                }
            }
            setFile(file + ".imp");
            using (FileStream impFile = new FileStream(string.Format("{0}{1}.imp", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter impWriter = new BinaryWriter(impFile))
                {
                    foreach (byte[] imp in impBuffer)
                    {
                        impWriter.Write(imp);
                    }
                    impWriter.Flush();
                }
            }
            if (lLod != null || mLod != null)
            {
                foreach (Stream.AssetEntry assetEntry in stream.AssetEntries)
                {
                    assetEntry.BinaryDataSize = 0;
                    assetEntry.RelocationsDataSize = 0;
                    assetEntry.ImportsDataSize = 0;
                }
                if (mLod != null)
                {
                    setTitle("Writing Medium LOD...", 0, false);
                    stream.Header.Checksum = GetRandom();
                    FileHelper.SetUInt(stream.Header.Checksum, 0, checksum);
                    if (stream.StreamReferences[0].ReferenceType == Stream.StreamReferenceType.PATCH)
                    {
                        stream.Header.ReferencedManifestNameBufferSize -= (uint)(stream.StreamReferences[0].Path.Length + 1);
                        stream.StreamReferences.RemoveAt(0);
                    }
                    stream.StreamReferences.Insert(0, new Stream.StreamReference(Stream.StreamReferenceType.PATCH, mLod));
                    stream.Header.ReferencedManifestNameBufferSize += (uint)(mLod.Length + 2);
                    using (FileStream manifestFile = new FileStream(string.Format("{0}_m{1}.manifest", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter manifestWriter = new BinaryWriter(manifestFile))
                        {
                            stream.Write(manifestWriter);
                            manifestWriter.Flush();
                        }
                    }
                    setFile(file + ".bin");
                    using (FileStream binFile = new FileStream(string.Format("{0}_m{1}.bin", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter binWriter = new BinaryWriter(binFile))
                        {
                            binWriter.Write(checksum);
                            binWriter.Flush();
                        }
                    }
                    setFile(file + ".relo");
                    using (FileStream reloFile = new FileStream(string.Format("{0}_m{1}.relo", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter reloWriter = new BinaryWriter(reloFile))
                        {
                            reloWriter.Write(checksum);
                            reloWriter.Flush();
                        }
                    }
                    setFile(file + ".imp");
                    using (FileStream impFile = new FileStream(string.Format("{0}_m{1}.imp", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter impWriter = new BinaryWriter(impFile))
                        {
                            impWriter.Write(checksum);
                            impWriter.Flush();
                        }
                    }
                    if (version != null)
                    {
                        using (FileStream versionFile = new FileStream(string.Format("{0}_m.version", outPath), FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (BinaryWriter versionWriter = new BinaryWriter(versionFile))
                            {
                                byte[] versionBuffer = new byte[version.Length + 1];
                                FileHelper.SetString(version + '\n', 0, versionBuffer);
                                versionWriter.Write(versionBuffer);
                                versionWriter.Flush();
                            }
                        }
                    }
                }
                if (lLod != null)
                {
                    setTitle("Writing Low LOD...", 0, false);
                    stream.Header.Checksum = GetRandom();
                    FileHelper.SetUInt(stream.Header.Checksum, 0, checksum);
                    if (stream.StreamReferences[0].ReferenceType == Stream.StreamReferenceType.PATCH)
                    {
                        stream.Header.ReferencedManifestNameBufferSize -= (uint)(stream.StreamReferences[0].Path.Length + 1);
                        stream.StreamReferences.RemoveAt(0);
                    }
                    stream.StreamReferences.Insert(0, new Stream.StreamReference(Stream.StreamReferenceType.PATCH, lLod));
                    stream.Header.ReferencedManifestNameBufferSize += (uint)(lLod.Length + 2);
                    using (FileStream manifestFile = new FileStream(string.Format("{0}_l{1}.manifest", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter manifestWriter = new BinaryWriter(manifestFile))
                        {
                            stream.Write(manifestWriter);
                            manifestWriter.Flush();
                        }
                    }
                    setFile(file + ".bin");
                    using (FileStream binFile = new FileStream(string.Format("{0}_l{1}.bin", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter binWriter = new BinaryWriter(binFile))
                        {
                            binWriter.Write(checksum);
                            binWriter.Flush();
                        }
                    }
                    setFile(file + ".relo");
                    using (FileStream reloFile = new FileStream(string.Format("{0}_l{1}.relo", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter reloWriter = new BinaryWriter(reloFile))
                        {
                            reloWriter.Write(checksum);
                            reloWriter.Flush();
                        }
                    }
                    setFile(file + ".imp");
                    using (FileStream impFile = new FileStream(string.Format("{0}_l{1}.imp", outPath, version), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (BinaryWriter impWriter = new BinaryWriter(impFile))
                        {
                            impWriter.Write(checksum);
                            impWriter.Flush();
                        }
                    }
                    if (version != null)
                    {
                        using (FileStream versionFile = new FileStream(string.Format("{0}_l.version", outPath), FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (BinaryWriter versionWriter = new BinaryWriter(versionFile))
                            {
                                byte[] versionBuffer = new byte[version.Length + 1];
                                FileHelper.SetString(version + '\n', 0, versionBuffer);
                                versionWriter.Write(versionBuffer);
                                versionWriter.Flush();
                            }
                        }
                    }
                }
            }
            if (version != null)
            {
                using (FileStream versionFile = new FileStream(string.Format("{0}.version", outPath), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter versionWriter = new BinaryWriter(versionFile))
                    {
                        byte[] versionBuffer = new byte[version.Length + 1];
                        FileHelper.SetString(version + '\n', 0, versionBuffer);
                        versionWriter.Write(versionBuffer);
                        versionWriter.Flush();
                    }
                }
            }
            setTitle("Done.", 0, false);
            setFile(string.Empty);
            setBar(100, true);
            if (noStringHashes)
            {
                return hasNoWarning;
            }
            if (source.AbsolutePath.EndsWith("stringhashes.xml", StringComparison.OrdinalIgnoreCase))
            {
                StreamCompiler.game = null;
                StreamCompiler.baseUri = null;
                StreamCompiler.setTitle = null;
                StreamCompiler.setFile = null;
                StreamCompiler.setAsset = null;
                StreamCompiler.setBar = null;
                return hasNoWarning;
            }
            setTitle("Gathering Hash Table", 0, false);
            setBar(0, false);
            Dictionary<uint, string> stringAndHash = new Dictionary<uint, string>();
            string shFile = Path.Combine(game.DefinitionPath, "stringhashes.bin");
            if (File.Exists(shFile))
            {
                using (FileStream stringHashes = new FileStream(shFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] buffer = new byte[stringHashes.Length];
                    stringHashes.Read(buffer, 0, buffer.Length);
                    int count = FileHelper.GetInt(8, buffer);
                    int offset = 0x10;
                    for (int idx = 0; idx < count; ++idx)
                    {
                        uint hash = FileHelper.GetUInt(offset, buffer);
                        offset += 4;
                        int length = FileHelper.GetInt(offset, buffer);
                        offset += 4;
                        stringAndHash.Add(hash, FileHelper.GetString(FileHelper.GetInt(offset, buffer) + 4, buffer, length));
                        offset += 4;
                    }
                }
            }
            foreach (Asset asset in assetList)
            {
                if (!stringAndHash.ContainsKey(asset.InstanceId))
                {
                    stringAndHash.Add(asset.InstanceId, asset.Name);
                }
            }
            shFile = Path.Combine(Environment.CurrentDirectory, "stringhashes.xml");
            using (FileStream stringHashes = new FileStream(shFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (XmlWriter stringHashesWriter = XmlWriter.Create(stringHashes, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = "    "
                }))
                {
                    stringHashesWriter.WriteStartDocument();
                    stringHashesWriter.WriteStartElement("AssetDeclaration", "uri:ea.com:eala:asset");
                    stringHashesWriter.WriteStartElement("StringHashTable");
                    stringHashesWriter.WriteAttributeString("id", "TheStringTable");
                    foreach (KeyValuePair<uint, string> sh in stringAndHash)
                    {
                        stringHashesWriter.WriteStartElement("StringAndHash");
                        stringHashesWriter.WriteAttributeString("Hash", sh.Key.ToString());
                        stringHashesWriter.WriteAttributeString("Text", sh.Value);
                        stringHashesWriter.WriteEndElement();
                    }
                    stringHashesWriter.WriteEndElement();
                    stringHashesWriter.WriteEndElement();
                    stringHashesWriter.WriteEndDocument();
                }
            }
            hasNoWarning |= CompileStringHashes(new Uri(shFile), outUri, setTitle, setFile, setAsset, setBar, setError);
            StreamCompiler.game = null;
            StreamCompiler.baseUri = null;
            StreamCompiler.setTitle = null;
            StreamCompiler.setFile = null;
            StreamCompiler.setAsset = null;
            StreamCompiler.setBar = null;
            return hasNoWarning;
        }

        private static bool CompileXml(List<string> processedFiles, ref int fileCount, ref int processedFileCount,
            Uri currentFile, string sourceMacro, out string ErrorDescription, bool isTempXml = false)
        {
            AssetDeclaration currentAssetDeclaration;
            if (!processedFiles.Contains(currentFile.ToString()))
            {
                List<Uri> tempFiles = new List<Uri>();
                if ((currentAssetDeclaration = AssetDeclaration.Load(currentFile.LocalPath, sourceMacro)) != null)
                {
                    if (currentAssetDeclaration.Includes != null && currentAssetDeclaration.Includes.Include != null)
                    {
                        foreach (Include include in currentAssetDeclaration.Includes.Include)
                        {
                            if (include.Type == IncludeType.REFERENCE)
                            {
                                string includeSource = include.Source.Substring(0, include.Source.Length - 3) + "manifest";
                                if (includeSource.Contains(":"))
                                {
                                    includeSource = includeSource.Substring(includeSource.IndexOf(':') + 1);
                                }
                                stream.StreamReferences.Add(new Stream.StreamReference(Stream.StreamReferenceType.REFERENCE, includeSource));
                                stream.Header.ReferencedManifestNameBufferSize += (uint)(includeSource.Length + 2);
                            }
                            else
                            {
                                Uri newUri = Macro.Parse(include.Source);
                                if (!newUri.IsAbsoluteUri)
                                {
                                    newUri = new Uri(currentFile, newUri);
                                }
                                if (File.Exists(newUri.LocalPath))
                                {
                                    if (include.Type == IncludeType.ALL)
                                    {
                                        ++fileCount;
                                        if (!CompileXml(processedFiles, ref fileCount, ref processedFileCount, newUri, include.Source, out ErrorDescription))
                                        {
                                            return false;
                                        }
                                    }
                                    else if (include.Type == IncludeType.INSTANCE)
                                    {
                                        tempFiles.Add(newUri);
                                    }
                                }
                            }
                        }
                    }
                    if (!isTempXml)
                    {
                        setFile(baseUri.MakeRelativeUri(currentFile).ToString());
                    }
                    if (currentAssetDeclaration.Defines != null && currentAssetDeclaration.Defines.Define != null)
                    {
                        foreach (Define define in currentAssetDeclaration.Defines.Define)
                        {
                            if (define.Override)
                            {
                                if (Defines.Define.ContainsKey(define.Name))
                                {
                                    Defines.Define[define.Name] = define.Value;
                                    break;
                                }
                            }
                            else
                            {
                                if (Defines.Define.ContainsKey(define.Name))
                                {
                                    break;
                                }
                            }
                            Defines.Define.Add(define.Name, define.Value);
                        }
                    }
                    if (tempFiles.Count != 0)
                    {
                    }
                    if (!AssetCompiler.Compile(currentFile, currentAssetDeclaration, game, out ErrorDescription, setAsset, isTempXml))
                    {
                        return false;
                    }
                    if (!isTempXml)
                    {
                        AssetCompiler.TempAssets.Clear();
                        processedFiles.Add(currentFile.ToString());
                    }
                }
            }
            if (!isTempXml)
            {
                double percentage = ((double)++processedFileCount / (double)fileCount) * 100;
                setTitle("Compiling Assets", (int)percentage, true);
                setBar(percentage, true);
            }
            ErrorDescription = string.Empty;
            return true;
        }
    }
}
