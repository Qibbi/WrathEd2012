using Files;
using Microsoft.Win32;
using SAGE;
using SAGE.WrathEdXML.GameDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace WrathEd
{
    public partial class BigView : Window
    {
        public const int MaxAssetDumpSize = 0x80000;
        public const string Size = "Size:";
        public const string SizeDecompressed = "Decompressed Size:";
        public const string SizeMByte = "MB";
        public const string SizeKByte = "KB";
        public const string SizeByte = "Byte";

        private List<SAGE.Big.File> bigFiles;
        private List<SAGE.Stream.File> streamFiles;
        private List<SAGE.Big.PackedFile> streamPackedFiles;
        private Dictionary<string, string> versionFiles;
        private GameStream gameStream;
        private string singleManifest;

        public BigView()
        {
            uint achievementID = (uint)AchievementType.GETTING_STARTED;
            if (!Globals.Achievements[achievementID].IsAchieved)
            {
                Achievement.Unlock(Globals.Achievements[achievementID]);
            }
            achievementID = (uint)AchievementType.CODE_SPY;
            if (!Globals.Achievements[achievementID].IsAchieved)
            {
                Achievement.Unlock(Globals.Achievements[achievementID]);
            }
            InitializeComponent();
        }

        public void Startup(object empty)
        {
            LoadGameDefinition(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
                + Path.DirectorySeparatorChar
                + Globals.Settings.GameDefinitionPath);
        }

        public void LoadGameDefinition(string source)
        {
            Globals.Game = new GameDefinition(Game.Load(source),
                (string value) =>
                {
                    return true;
                });
            SetWindowTitle();
            SetBarColor((WrathEd.Controls.ProgressBar.ProgressBarColor)(Globals.Game.ThemeColor));
        }

        public void LoadGameDefinition()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "SAGE Game Definitions|*.xml";
            ofd.InitialDirectory = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Games";
            ofd.Multiselect = false;
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                LoadGameDefinition(ofd.FileName);
            }
        }

        private void LoadConfig(Uri source)
        {
            using (FileStream stream = new FileStream(source.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (line != string.Empty)
                        {
                            List<string> command = new List<string>();
                            int position = 0;
                            for (int idx = 0; idx < line.Length; ++idx)
                            {
                                if (line[idx] == ' ')
                                {
                                    command.Add(line.Substring(position, idx - position));
                                    position = idx + 1;
                                    break;
                                }
                            }
                            command.Add(line.Substring(position));
                            switch (command[0])
                            {
                                case "add-config":
                                    Uri configUri = new Uri(command[1], UriKind.RelativeOrAbsolute);
                                    if (!configUri.IsAbsoluteUri)
                                    {
                                        configUri = new Uri(source, configUri);
                                    }
                                    LoadConfig(configUri);
                                    break;
                                case "try-add-big":
                                case "add-big":
                                    Uri bigUri = new Uri(command[1], UriKind.RelativeOrAbsolute);
                                    if (!bigUri.IsAbsoluteUri)
                                    {
                                        bigUri = new Uri(source, bigUri);
                                    }
                                    bigFiles.Add(new SAGE.Big.File(bigUri.LocalPath));
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void LoadStreams()
        {
            streamFiles = new List<SAGE.Stream.File>();
            streamPackedFiles = new List<SAGE.Big.PackedFile>();
            versionFiles = new Dictionary<string, string>();
            int bigFileCount = 0;
            foreach (SAGE.Big.File bigFile in bigFiles)
            {
                foreach (SAGE.Big.PackedFile file in bigFile.Files)
                {
                    string fileName = file.Name;
                    string fileNameExtension = fileName.Substring(fileName.LastIndexOf('.'));
                    string fileNameWithoutExtension = fileName.Substring(0, fileName.LastIndexOf('.'));
                    switch (fileNameExtension)
                    {
                        case ".version":
                            if (!versionFiles.ContainsKey(fileNameWithoutExtension))
                            {
                                versionFiles.Add(fileNameWithoutExtension, FileHelper.GetString(0x00, bigFile.GetFile(file, bigFile.GetFileSize(file))).Trim(new char[] { '\r', '\n' }));
                            }
                            break;
                        case ".manifest":
                            streamFiles.Add(new SAGE.Stream.File(bigFile.GetFile(file, bigFile.GetFileSize(file))));
                            streamPackedFiles.Add(file);
                            break;
                    }
                }
                SetBarToValue((double)(++bigFileCount) / bigFiles.Count * 100);
            }
            List<string> streams = new List<string>();
            foreach (SAGE.Big.PackedFile packedFile in streamPackedFiles)
            {
                string streamName = packedFile.Name;
                bool isFound = false;
                foreach (string versionFile in versionFiles.Keys)
                {
                    if (streamName.Contains(versionFile))
                    {
                        if (!streams.Contains(versionFile))
                        {
                            streams.Add(versionFile);
                            AddStreamToBig(versionFile);
                        }
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                {
                    streamName = streamName.Substring(0, streamName.LastIndexOf('.'));
                    if (!streams.Contains(streamName))
                    {
                        streams.Add(streamName);
                        AddStreamToBig(streamName);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs args)
        {
            ThreadPool.QueueUserWorkItem(Startup);
        }

        private void File_LoadGameDefinition_Click(object sender, RoutedEventArgs args)
        {
            LoadGameDefinition();
            Globals.Settings.GameDefinition = Globals.Game.id;
            Globals.Settings.SaveSettings();
        }

        private void File_OpenManifest_Click(object sender, RoutedEventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "SAGE Manfiest file|*.manifest";
            ofd.Multiselect = false;
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                uint achievementID = (uint)AchievementType.LITTLE_BIGS;
                if (!Globals.Achievements[achievementID].IsAchieved)
                {
                    Achievement.Unlock(Globals.Achievements[achievementID]);
                }
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        SetBarToNoValue();
                        SetEditExport(false);
                        ClearStream();
                        ClearBig();
                        singleManifest = ofd.FileName;
                        streamFiles = new List<SAGE.Stream.File>();
                        streamFiles.Add(new SAGE.Stream.File(System.IO.File.ReadAllBytes(ofd.FileName)));
                        List<string> assets = new List<string>();
                        foreach (SAGE.Stream.AssetEntry asset in streamFiles[0].AssetEntries)
                        {
                            if (asset.BinaryDataSize != 0)
                            {
                                assets.Add(streamFiles[0].AssetNames[asset.NameOffset]);
                            }
                        }
                        AddItemsToStream(assets);
                        SetContentToStreamInfo();
                        SetBarToValue(0.0);
                    });
            }
        }

        private void File_OpenBig_Click(object sender, RoutedEventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "SAGE Big File|*.big";
            ofd.Multiselect = false;
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                uint achievementID = (uint)AchievementType.LITTLE_BIGS;
                if (!Globals.Achievements[achievementID].IsAchieved)
                {
                    Achievement.Unlock(Globals.Achievements[achievementID]);
                }
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        SetBarToNoValue();
                        bigFiles = new List<SAGE.Big.File>();
                        SetEditExport(true);
                        ClearStream();
                        ClearBig();
                        singleManifest = null;
                        bigFiles.Add(new SAGE.Big.File(ofd.FileName));
                        foreach (SAGE.Big.PackedFile file in bigFiles[0].Files)
                        {
                            AddItemToBig(file);
                        }
                        Big.SelectionChanged += Big_SelectionChanged;
                        SetBarToValue(0.0);
                    });
            }
        }

        private void File_OpenSkuDef_Click(object sender, RoutedEventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "SAGE Definitions File|*.skudef";
            ofd.Multiselect = false;
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                uint achievementID = (uint)AchievementType.SHOW_EVERYTHING;
                if (!Globals.Achievements[achievementID].IsAchieved)
                {
                    Achievement.Unlock(Globals.Achievements[achievementID]);
                }
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        SetBarToNoValue();
                        bigFiles = new List<SAGE.Big.File>();
                        SetEditExport(false);
                        ClearStream();
                        ClearBig();
                        singleManifest = null;
                        LoadConfig(new Uri(ofd.FileName));
                        LoadStreams();
                        Big.SelectionChanged += StreamBig_SelectionChanged;
                        SetBarToValue(0.0);
                    });
            }
        }

        private void File_Exit_Click(object sender, RoutedEventArgs args)
        {
            if (bigFiles != null)
            {
                foreach (SAGE.Big.File file in bigFiles)
                {
                    file.Dispose();
                }
            }
            Close();
        }

        private void Edit_Export_Click(object sender, RoutedEventArgs args)
        {
            if (Big.SelectedIndex != -1)
            {
                SAGE.Big.PackedFile item = GetBigItem(Big.SelectedItem);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = item.ToString();
                sfd.OverwritePrompt = true;
                bool? result = sfd.ShowDialog();
                if (result == true)
                {
                    uint achievementID = (uint)AchievementType.MANY_THINGS;
                    if (!Globals.Achievements[achievementID].IsAchieved)
                    {
                        Achievement.Unlock(Globals.Achievements[achievementID]);
                    }
                    ThreadPool.QueueUserWorkItem(
                        (object empty) =>
                        {
                            SetBarToNoValue();
                            using (FileStream fileStream = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                            {
                                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                                {
                                    if (bigFiles[0].GetIsFileCompressed(item))
                                    {
                                        bigFiles[0].DecompressToDisk(binaryWriter, item.Offset, bigFiles[0].GetFileSize(item));
                                    }
                                    else
                                    {
                                        binaryWriter.Write(bigFiles[0].GetFile(item, item.Size));
                                    }
                                    binaryWriter.Flush();
                                }
                            }
                            SetBarToValue(0.0);
                        });
                }
            }
        }

        private void Edit_Export_All_Click(object sender, RoutedEventArgs args)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string value = fbd.SelectedPath;
                    ThreadPool.QueueUserWorkItem(
                        (object empty) =>
                        {
                            SetBarToNoValue();
                            SAGE.Big.PackedFile[] bigItems = GetBigItems();
                            int doneItems = 0;
                            foreach (SAGE.Big.PackedFile item in bigItems)
                            {
                                uint decompressedSize = bigFiles[0].GetFileSize(item);
                                byte[] fileBuffer = bigFiles[0].GetFile(item, decompressedSize);
                                string folderName = fbd.SelectedPath + Path.DirectorySeparatorChar
                                    + item.Name.Substring(0, item.Name.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                                if (!Directory.Exists(folderName))
                                {
                                    Directory.CreateDirectory(folderName);
                                }
                                using (FileStream fileStream = new FileStream(fbd.SelectedPath + Path.DirectorySeparatorChar + item.Name,
                                    FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                                {
                                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                                    {
                                        binaryWriter.Write(fileBuffer);
                                        binaryWriter.Flush();
                                    }
                                }
                                SetBarToValue(doneItems / (double)bigItems.Length * 100.0);
                            }
                            SetBarToValue(0.0);
                        });
                }
            }
        }

        private void Help_About_Click(object sender, RoutedEventArgs args)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Owner = Window.GetWindow(this);
            aboutBox.Show();
        }

        private void StreamBig_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        SetBarToNoValue();
                        ClearStream();
                        string manifestName = string.Empty;
                        Dispatcher.Invoke((Action)(() =>
                        {
                            manifestName = (args.AddedItems[0] as ListBoxItem).Content as string;
                        }));
                        if (versionFiles.ContainsKey(manifestName))
                        {
                            manifestName += versionFiles[manifestName];
                        }
                        manifestName += ".manifest";
                        for (int idx = 0; idx < streamPackedFiles.Count; ++idx)
                        {
                            if (streamPackedFiles[idx].Name == manifestName)
                            {
                                gameStream = new GameStream(streamFiles[idx], streamPackedFiles[idx], streamFiles, streamPackedFiles, versionFiles);
                            }
                        }
                        List<string> assets = new List<string>();
                        foreach (SAGE.Stream.AssetEntry asset in gameStream.StreamManifest.AssetEntries)
                        {
                            assets.Add(gameStream.StreamManifest.AssetNames[asset.NameOffset]);
                        }
                        AddStreamItemsToStream(assets);
                        SetContentToStreamManifestInfo();
                        SetBarToValue(0.0);
                    });
            }
        }

        private void Big_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        SetBarToNoValue();
                        streamFiles = new List<SAGE.Stream.File>();
                        ClearStream();
                        SAGE.Big.PackedFile item = GetBigItem(args.AddedItems[0]);
                        string extension = item.Name.Substring(item.Name.LastIndexOf(".") + 1).ToLowerInvariant();
                        uint decompressedSize = bigFiles[0].GetFileSize(item);
                        if (item.Size == decompressedSize)
                        {
                            SetDecompressedSize(item.Size);
                        }
                        else
                        {
                            SetDecompressedSize(item.Size, decompressedSize);
                        }
                        switch (extension)
                        {
                            case "dep":
                            case "fx":
                            case "inc":
                            case "ini":
                            case "lua":
                            case "scrapeo":
                            case "str":
                            case "txt":
                            case "version":
                            case "wnd":
                            case "w3x":
                            case "xml":
                            case "xsd":
                            case "xsx":
                                byte[] buffer = bigFiles[0].GetFile(item, decompressedSize);
                                SetContent(FileHelper.GetString(0x00, buffer, (int)decompressedSize));
                                break;
                            case "bmp":
                            case "gif":
                            case "jpg":
                            case "png":
                            case "tga":
                            case "dds":
                                SetContent("Image File");
                                break;
                            case "apt":
                                SetContent("APT UI File");
                                break;
                            case "const":
                                SetContent("APT UI constants File");
                                break;
                            case "dat":
                                SetContent("Data File");
                                break;
                            case "fxo":
                                SetContent("Compiled DirectX Shader File");
                                break;
                            case "manifest":
                                streamFiles.Add(new SAGE.Stream.File(bigFiles[0].GetFile(item, decompressedSize)));
                                List<string> assets = new List<string>();
                                foreach (SAGE.Stream.AssetEntry asset in streamFiles[0].AssetEntries)
                                {
                                    if (asset.BinaryDataSize != 0)
                                    {
                                        assets.Add(streamFiles[0].AssetNames[asset.NameOffset]);
                                    }
                                }
                                AddItemsToStream(assets);
                                SetContentToStreamInfo();
                                break;
                            case "bin":
                                buffer = bigFiles[0].GetFile(item, 0x08);
                                SetContent(string.Format("Stream Binary File\nChecksum: {0:X08} {1:X08}", FileHelper.GetUInt(0x00, buffer), FileHelper.GetUInt(0x04, buffer)));
                                break;
                            case "imp":
                                buffer = bigFiles[0].GetFile(item, 0x08);
                                SetContent(string.Format("Stream Imports File\nChecksum: {0:X08} {1:X08}", FileHelper.GetUInt(0x00, buffer), FileHelper.GetUInt(0x04, buffer)));
                                break;
                            case "relo":
                                buffer = bigFiles[0].GetFile(item, 0x08);
                                SetContent(string.Format("Stream Relocations File\nChecksum: {0:X08} {1:X08}", FileHelper.GetUInt(0x00, buffer), FileHelper.GetUInt(0x04, buffer)));
                                break;
                            case "cdata":
                                SetContent("Data File");
                                break;
                            case "map":
                                SetContent("Map File");
                                break;
                            case "otf":
                            case "ttf":
                                SetContent("Font File");
                                break;
                            case "max":
                                SetContent("3ds Max File");
                                break;
                            case "w3d":
                                SetContent("Westwood 3D File");
                                break;
                            case "snd":
                                SetContent("VP6 Sound File");
                                break;
                            case "vp6":
                                SetContent("VP6 Video File");
                                break;
                            default:
                                SetContent("Unknown Extension");
                                break;
                        }
                        SetBarToValue(0.0);
                    });
            }
        }

        private void StreamStream_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();
                        SetBarToNoValue();
                        string assetName = GetSelectedStreamItem();
                        string[] splitAssetName = assetName.Split(':');
                        uint[] assetHashes = new uint[] { StringHasher.Hash(splitAssetName[0]), StringHasher.Hash(splitAssetName[1].ToLowerInvariant()) };
                        SAGE.Stream.AssetEntry asset = null;
                        bool isFound = false;
                        SAGE.Stream.File versionManifest = new SAGE.Stream.File(5);
                        SAGE.Big.PackedFile versionBig = new SAGE.Big.PackedFile();
                        foreach (SAGE.Stream.AssetEntry entry in gameStream.StreamManifest.AssetEntries)
                        {
                            if (entry.TypeId == assetHashes[0] && entry.InstanceId == assetHashes[1])
                            {
                                if (entry.BinaryDataSize != 0)
                                {
                                    versionManifest = gameStream.StreamManifest;
                                    versionBig = gameStream.StreamManifestEntry;
                                    isFound = true;
                                    asset = entry;
                                }
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            for (int idx = 0; idx < gameStream.StreamBases.Count; ++idx)
                            {
                                foreach (SAGE.Stream.AssetEntry entry in gameStream.StreamBases[idx].AssetEntries)
                                {
                                    if (entry.TypeId == assetHashes[0] && entry.InstanceId == assetHashes[1])
                                    {
                                        if (entry.BinaryDataSize != 0)
                                        {
                                            versionManifest = gameStream.StreamBases[idx];
                                            versionBig = gameStream.StreamBaseEntries[idx];
                                            isFound = true;
                                            asset = entry;
                                        }
                                        break;
                                    }
                                }
                                if (isFound)
                                {
                                    break;
                                }
                            }
                        }
                        string sourceName = versionManifest.SourceFiles[asset.SourceFileNameOffset];
                        List<SAGE.Stream.AssetReference> assetReferences = new List<SAGE.Stream.AssetReference>();
                        for (int idx = 0; idx < asset.AssetReferenceCount; ++idx)
                        {
                            assetReferences.Add(versionManifest.AssetReferences[(int)(asset.AssetReferenceOffset) / 8 + idx]);
                        }
                        string fileName = versionBig.Name;
                        string fileNameWithoutExtension = fileName.Substring(0, fileName.LastIndexOf('.'));
                        string fileNameBin = fileNameWithoutExtension + ".bin";
                        string fileNameImp = fileNameWithoutExtension + ".imp";
                        string fileNameRelo = fileNameWithoutExtension + ".relo";
                        uint binOffset = 0x04;
                        uint impOffset = 0x04;
                        uint reloOffset = 0x04;
                        if (streamFiles[0].Header.Version == 7)
                        {
                            binOffset = 0x08;
                            impOffset = 0x08;
                            reloOffset = 0x08;
                        }
                        foreach (SAGE.Stream.AssetEntry assetEntry in versionManifest.AssetEntries)
                        {
                            if (assetEntry == asset)
                            {
                                break;
                            }
                            binOffset += assetEntry.BinaryDataSize;
                            impOffset += assetEntry.ImportsDataSize;
                            reloOffset += assetEntry.RelocationsDataSize;
                        }
                        byte[] binData = null;
                        byte[] impData = null;
                        byte[] reloData = null;
                        foreach (SAGE.Big.File file in bigFiles)
                        {
                            if (file.Contains(versionBig))
                            {
                                binData = file.GetFile(fileNameBin, asset.BinaryDataSize, binOffset);
                                impData = file.GetFile(fileNameImp, asset.ImportsDataSize, impOffset);
                                reloData = file.GetFile(fileNameRelo, asset.RelocationsDataSize, reloOffset);
                            }
                        }
                        string xml;
                        StringBuilder stringBuilder = new StringBuilder();
                        if (SAGE.Compiler.AssetCompiler.Decompile(out xml, Globals.Game, versionManifest, asset, assetReferences, binData, impData, reloData, streamFiles))
                        {
                            stringBuilder.Append("\n");
                            stringBuilder.Append(xml);
                        }
#if !DEBUG
                        else
                        {
#endif
                        stringBuilder.Append(string.Format("\n\nAsset Dump:\n{0}\n{1}\n{2:X08}:{3:X08}:{4:X08}:{5:X08}\n\nAssetReferences: {6}\n",
                            assetName, sourceName,
                            asset.TypeId, asset.InstanceId, asset.TypeHash, asset.InstanceHash,
                            asset.AssetReferenceCount));
                        //                        for (int idx = 0; idx < asset.AssetReferenceCount; ++idx)
                        //                        {
                        //                            SAGE.Stream.AssetEntry reference = versionManifest.AssetEntries.Find(x =>
                        //                                x.TypeId == assetReferences[idx].TypeId && x.InstanceId == assetReferences[idx].InstanceId);
                        //                            if (reference == null)
                        //                            {
                        //                                stringBuilder.Append(string.Format("{0:X08}:{1:X08}\n", assetReferences[idx].TypeId, assetReferences[idx].InstanceId));
                        //                            }
                        //                            else
                        //                            {
                        //                                stringBuilder.Append(string.Format("{0:X08}:{1:X08} ({2})\n",
                        //                                    assetReferences[idx].TypeId, assetReferences[idx].InstanceId, versionManifest.AssetNames[reference.NameOffset]));
                        //                            }
                        //                        }
                        //                        stringBuilder.Append(string.Format("\nImports: {0} Offset: {1}\n", asset.ImportsDataSize, impOffset));
                        //                        for (int idx = 0; idx < impData.Length; ++idx)
                        //                        {
                        //                            if (idx != 0)
                        //                            {
                        //                                if (idx % 0x10 == 0)
                        //                                {
                        //                                    stringBuilder.Append(string.Format("\n{0:X06}: ", idx));
                        //                                }
                        //                                else if (idx % 0x04 == 0)
                        //                                {
                        //                                    stringBuilder.Append(' ');
                        //                                }
                        //                            }
                        //                            else
                        //                            {
                        //                                stringBuilder.Append(string.Format("{0:X06}: ", idx));
                        //                            }
                        //                            stringBuilder.Append(string.Format("{0:X02}", impData[idx]));
                        //                        }
                        //                        stringBuilder.Append(string.Format("\n\nRelocations: {0} Offset: {1}\n", asset.RelocationsDataSize, reloOffset));
                        //                        for (int idx = 0; idx < reloData.Length; ++idx)
                        //                        {
                        //                            if (idx != 0)
                        //                            {
                        //                                if (idx % 0x10 == 0)
                        //                                {
                        //                                    stringBuilder.Append(string.Format("\n{0:X06}: ", idx));
                        //                                }
                        //                                else if (idx % 0x04 == 0)
                        //                                {
                        //                                    stringBuilder.Append(' ');
                        //                                }
                        //                            }
                        //                            else
                        //                            {
                        //                                stringBuilder.Append(string.Format("{0:X06}: ", idx));
                        //                            }
                        //                            stringBuilder.Append(string.Format("{0:X02}", reloData[idx]));
                        //                        }
                        int assetDumpSize = binData.Length;
                        //                        if (binData.Length > MaxAssetDumpSize)
                        //                        {
                        //                            stringBuilder.Append(string.Format("\n\nBinary (up to {0}): {1} Offset: {2}\n", MaxAssetDumpSize, asset.BinaryDataSize, binOffset));
                        //                            assetDumpSize = Math.Min(binData.Length, MaxAssetDumpSize);
                        //                        }
                        //                        else
                        {
                            stringBuilder.Append(string.Format("\n\nBinary: {0} Offset: {1}\n", asset.BinaryDataSize, binOffset));
                        }
                        for (int idx = 0; idx < assetDumpSize; ++idx)
                        {
                            SetBarToValue(idx / (double)(binData.Length) * 100.0);
                            if (idx != 0)
                            {
                                if (idx % 0x10 == 0)
                                {
                                    stringBuilder.Append(string.Format("\n{0:X06}: ", idx));
                                }
                                else if (idx % 0x04 == 0)
                                {
                                    stringBuilder.Append(' ');
                                }
                            }
                            else
                            {
                                stringBuilder.Append(string.Format("{0:X06}: ", idx));
                            }
                            stringBuilder.Append(string.Format("{0:X02}", binData[idx]));
                        }
#if !DEBUG
                        }
#endif
                        stopwatch.Stop();
                        stringBuilder.Insert(0, string.Format("Generated in: {0}\n", stopwatch.Elapsed));
                        SetContent(stringBuilder.ToString());
                        SetBarToValue(0.0);
                    });
            }
        }

        private void Stream_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(
                    (object empty) =>
                    {
                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();
                        SetBarToNoValue();
                        string assetName = GetSelectedStreamItem();
                        string[] splitAssetName = assetName.Split(':');
                        uint[] assetHashes = new uint[] { StringHasher.Hash(splitAssetName[0]), StringHasher.Hash(splitAssetName[1].ToLowerInvariant()) };
                        SAGE.Stream.AssetEntry asset = null;
                        foreach (SAGE.Stream.AssetEntry entry in streamFiles[0].AssetEntries)
                        {
                            if (entry.TypeId == assetHashes[0] && entry.InstanceId == assetHashes[1])
                            {
                                asset = entry;
                                break;
                            }
                        }
                        string sourceName = streamFiles[0].SourceFiles[asset.SourceFileNameOffset];
                        List<SAGE.Stream.AssetReference> assetReferences = new List<SAGE.Stream.AssetReference>();
                        for (int idx = 0; idx < asset.AssetReferenceCount; ++idx)
                        {
                            assetReferences.Add(streamFiles[0].AssetReferences[(int)(asset.AssetReferenceOffset) / 8 + idx]);
                        }
                        string fileName;
                        if (singleManifest is null)
                        {
                            fileName = bigFiles[0].Files[GetSelectedBigIndex()].Name;
                        }
                        else
                        {
                            fileName = singleManifest;
                        }
                        string fileNameWithoutExtension = fileName.Substring(0, fileName.LastIndexOf('.'));
                        string fileNameBin = fileNameWithoutExtension + ".bin";
                        string fileNameImp = fileNameWithoutExtension + ".imp";
                        string fileNameRelo = fileNameWithoutExtension + ".relo";
                        uint binOffset = 0x04;
                        uint impOffset = 0x04;
                        uint reloOffset = 0x04;
                        if (streamFiles[0].Header.Version == 7)
                        {
                            binOffset = 0x08;
                            impOffset = 0x08;
                            reloOffset = 0x08;
                        }
                        foreach (SAGE.Stream.AssetEntry assetEntry in streamFiles[0].AssetEntries)
                        {
                            if (assetEntry == asset)
                            {
                                break;
                            }
                            binOffset += assetEntry.BinaryDataSize;
                            impOffset += assetEntry.ImportsDataSize;
                            reloOffset += assetEntry.RelocationsDataSize;
                        }
                        byte[] binData = null;
                        byte[] impData = null;
                        byte[] reloData = null;
                        if (singleManifest is null)
                        {
                            binData = bigFiles[0].GetFile(fileNameBin, asset.BinaryDataSize, binOffset);
                            impData = bigFiles[0].GetFile(fileNameImp, asset.ImportsDataSize, impOffset);
                            reloData = bigFiles[0].GetFile(fileNameRelo, asset.RelocationsDataSize, reloOffset);
                        }
                        else
                        {
                            using (Stream stream = new FileStream(fileNameBin, FileMode.Open))
                            {
                                binData = new byte[asset.BinaryDataSize];
                                stream.Seek(binOffset, SeekOrigin.Begin);
                                stream.Read(binData, 0, binData.Length);
                            }
                            using (Stream stream = new FileStream(fileNameImp, FileMode.Open))
                            {
                                impData = new byte[asset.ImportsDataSize];
                                stream.Seek(impOffset, SeekOrigin.Begin);
                                stream.Read(impData, 0, impData.Length);
                            }
                            using (Stream stream = new FileStream(fileNameRelo, FileMode.Open))
                            {
                                reloData = new byte[asset.RelocationsDataSize];
                                stream.Seek(reloOffset, SeekOrigin.Begin);
                                stream.Read(reloData, 0, reloData.Length);
                            }
                        }

                        string xml;
                        StringBuilder stringBuilder = new StringBuilder();
                        if (SAGE.Compiler.AssetCompiler.Decompile(out xml, Globals.Game, streamFiles[0], asset, assetReferences, binData, impData, reloData))
                        {
                            stringBuilder.Append("\n");
                            stringBuilder.Append(xml);
                        }
#if !DEBUG
                        else
                        {
#endif
                        stringBuilder.Append(string.Format("\n\nAsset Dump:\n{0}\n{1}\n{2:X08}:{3:X08}:{4:X08}:{5:X08}\n\nAssetReferences: {6} Offset: {7}\n",
                            assetName, sourceName,
                            asset.TypeId, asset.InstanceId, asset.TypeHash, asset.InstanceHash,
                            asset.AssetReferenceCount, asset.AssetReferenceOffset));
                        for (int idx = 0; idx < asset.AssetReferenceCount; ++idx)
                        {
                            stringBuilder.Append(string.Format("{0:X08}:{1:X08}\n", assetReferences[idx].TypeId, assetReferences[idx].InstanceId));
                        }
                        stringBuilder.Append(string.Format("\nImports: {0} Offset: {1}\n", asset.ImportsDataSize, impOffset));
                        for (int idx = 0; idx < impData.Length; ++idx)
                        {
                            if (idx != 0)
                            {
                                if (idx % 0x10 == 0)
                                {
                                    stringBuilder.Append(string.Format("\n{0:X06}: ", idx));
                                }
                                else if (idx % 0x04 == 0)
                                {
                                    stringBuilder.Append(' ');
                                }
                            }
                            else
                            {
                                stringBuilder.Append(string.Format("{0:X06}: ", idx));
                            }
                            stringBuilder.Append(string.Format("{0:X02}", impData[idx]));
                        }
                        stringBuilder.Append(string.Format("\n\nRelocations: {0} Offset: {1}\n", asset.RelocationsDataSize, reloOffset));
                        for (int idx = 0; idx < reloData.Length; ++idx)
                        {
                            if (idx != 0)
                            {
                                if (idx % 0x10 == 0)
                                {
                                    stringBuilder.Append(string.Format("\n{0:X06}: ", idx));
                                }
                                else if (idx % 0x04 == 0)
                                {
                                    stringBuilder.Append(' ');
                                }
                            }
                            else
                            {
                                stringBuilder.Append(string.Format("{0:X06}: ", idx));
                            }
                            stringBuilder.Append(string.Format("{0:X02}", reloData[idx]));
                        }
                        int assetDumpSize = binData.Length;
                        //                        if (binData.Length > MaxAssetDumpSize)
                        //                        {
                        //                            stringBuilder.Append(string.Format("\n\nBinary (up to {0}): {1} Offset: {2}\n", MaxAssetDumpSize, asset.BinaryDataSize, binOffset));
                        //                            assetDumpSize = Math.Min(binData.Length, MaxAssetDumpSize);
                        //                        }
                        //                        else
                        {
                            stringBuilder.Append(string.Format("\n\nBinary: {0} Offset: {1}\n", asset.BinaryDataSize, binOffset));
                        }
                        for (int idx = 0; idx < assetDumpSize; ++idx)
                        {
                            SetBarToValue(idx / (double)(binData.Length) * 100.0);
                            if (idx != 0)
                            {
                                if (idx % 0x10 == 0)
                                {
                                    stringBuilder.Append(string.Format("\n{0:X06}: ", idx));
                                }
                                else if (idx % 0x04 == 0)
                                {
                                    stringBuilder.Append(' ');
                                }
                            }
                            else
                            {
                                stringBuilder.Append(string.Format("{0:X06}: ", idx));
                            }
                            stringBuilder.Append(string.Format("{0:X02}", binData[idx]));
                        }
#if !DEBUG
                        }
#endif
                        stopwatch.Stop();
                        stringBuilder.Insert(0, string.Format("Generated in: {0}\n", stopwatch.Elapsed));
                        SetContent(stringBuilder.ToString());
                        SetBarToValue(0.0);
                    });
            }
        }

        private void Search_Click(object sender, RoutedEventArgs args)
        {
            string filter = SearchAssetNameBox.Text.ToLowerInvariant();
            if (filter.Length > 0)
            {
                if (AssetTypeFilter.SelectedIndex > 0)
                {
                    Stream.Items.Filter = (obj) =>
                    {
                        return (obj as string).ToLowerInvariant().Contains(filter) &&
                            (obj as string).StartsWith(AssetTypeFilter.SelectedItem as string);
                    };
                }
                else
                {
                    Stream.Items.Filter = (obj) =>
                    {
                        return (obj as string).ToLowerInvariant().Contains(filter);
                    };
                }
            }
            else
            {
                AssetTypeFilter_SelectionChanged(null, null);
            }
        }

        private void AssetTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (AssetTypeFilter.SelectedIndex > 0)
            {
                Stream.Items.Filter = (obj) =>
                {
                    return (obj as string).StartsWith((AssetTypeFilter.SelectedItem as string) + ':');
                };
            }
            else
            {
                Stream.Items.Filter = null;
            }
        }

        public void SetWindowTitle()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Title = "WrathEd BigView - Loaded Game Definition: " + Globals.Game.id;
            }));
        }

        public void SetEditExport(bool value)
        {
            Dispatcher.Invoke((Action)(() =>
                {
                    Edit_Export.IsEnabled = value;
                    Edit_Export_All.IsEnabled = value;
                }));
        }

        public void ClearBig()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Big.SelectionChanged -= Big_SelectionChanged;
                Big.SelectionChanged -= StreamBig_SelectionChanged;
                Big.Items.Clear();
            }));
        }

        public void ClearStream()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Stream.SelectionChanged -= Stream_SelectionChanged;
                Stream.SelectionChanged -= StreamStream_SelectionChanged;
                Stream.ItemsSource = null;
                AssetTypeFilter.ItemsSource = null;
            }));
        }

        public int GetSelectedBigIndex()
        {
            int result = -1;
            Dispatcher.Invoke((Action)(() =>
            {
                result = Big.SelectedIndex;
            }));
            return result;
        }

        public string GetSelectedStreamItem()
        {
            string result = string.Empty;
            Dispatcher.Invoke((Action)(() =>
            {
                result = Stream.SelectedItem as string;
            }));
            return result;
        }

        public SAGE.Big.PackedFile GetBigItem(object item)
        {
            SAGE.Big.PackedFile packedFile = null;
            Dispatcher.Invoke((Action)(() =>
            {
                packedFile = (item as ListBoxItem).Content as SAGE.Big.PackedFile;
            }));
            return packedFile;
        }

        public SAGE.Big.PackedFile[] GetBigItems()
        {
            SAGE.Big.PackedFile[] result = null;
            Dispatcher.Invoke((Action)(() =>
            {
                result = new SAGE.Big.PackedFile[Big.Items.Count];
                for (int idx = 0; idx < result.Length; ++idx)
                {
                    result[idx] = (Big.Items[idx] as ListBoxItem).Content as SAGE.Big.PackedFile;
                }
            }));
            return result;
        }

        public void AddItemToBig(SAGE.Big.PackedFile file)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ListBoxItem item = new ListBoxItem();
                if (file.Size > 0x100000)
                {
                    item.ToolTip = string.Format("{0} {1:n02} {2} ({3})", Size, (double)file.Size / 0x100000, SizeMByte, file.Size);
                }
                else if (file.Size > 0x0400)
                {
                    item.ToolTip = string.Format("{0} {1:n02} {2} ({3})", Size, (double)file.Size / 0x0400, SizeKByte, file.Size);
                }
                else
                {
                    item.ToolTip = string.Format("{0} {1} {2}", Size, file.Size, SizeByte);
                }
                item.Content = file;
                Big.Items.Add(item);
            }));
        }

        public void AddStreamToBig(string fileNameWithoutExtension)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = fileNameWithoutExtension;
                if (versionFiles.ContainsKey(fileNameWithoutExtension))
                {
                    item.ToolTip = versionFiles[fileNameWithoutExtension];
                }
                Big.Items.Add(item);
            }));
        }

        public void AddStreamItemsToStream(List<string> assets)
        {
            List<string> categories = new List<string>();
            foreach (string assetName in assets)
            {
                string[] labelCategory = assetName.Split(':');
                if (!categories.Contains(labelCategory[0]))
                {
                    categories.Add(labelCategory[0]);
                }
            }
            var orderedCategories = from c in categories
                                    orderby c
                                    select c;
            Dispatcher.Invoke((Action)(() =>
            {
                Stream.ItemsSource = null;
                Stream.ItemsSource = assets;
                Stream.SelectionChanged += StreamStream_SelectionChanged;
                AssetTypeFilter.ItemsSource = null;
                List<string> assetTypes = new List<string>();
                assetTypes.Add(string.Empty);
                assetTypes.AddRange(orderedCategories.ToList());
                AssetTypeFilter.ItemsSource = assetTypes;
            }));
        }

        public void AddItemsToStream(List<string> assets)
        {
            List<string> categories = new List<string>();
            foreach (string assetName in assets)
            {
                string[] labelCategory = assetName.Split(':');
                if (!categories.Contains(labelCategory[0]))
                {
                    categories.Add(labelCategory[0]);
                }
            }
            var orderedCategories = from c in categories
                                    orderby c
                                    select c;
            Dispatcher.Invoke((Action)(() =>
            {
                Stream.ItemsSource = null;
                Stream.ItemsSource = assets;
                Stream.SelectionChanged += Stream_SelectionChanged;
                AssetTypeFilter.ItemsSource = null;
                List<string> assetTypes = new List<string>();
                assetTypes.Add(string.Empty);
                assetTypes.AddRange(orderedCategories.ToList());
                AssetTypeFilter.ItemsSource = assetTypes;
            }));
        }

        public void SetContent(string value)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Content.Text = value;
            }));
        }

        public void SetContentToStreamManifestInfo()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                string value = string.Format(
                    "IsBigEndian: {0}\nIsLinked: {1}\nVersion: {2}\nChecksum: 0x{3:X08}\nAllTypesHash: 0x{4:X08}\nAssetCount {5}\nBinaryDataSize: {6}\nMaxBinaryChunkSize: {7}\nMaxRelocationsChunkSize: {8}\nMaxImportsChunkSize: {9}\nAssetReferenceBufferSize: {10}\nReferencedManifestNameBufferSize: {11}\nAssetNameBufferSize: {12}\nSourceFileNameBufferSize: {13}",
                    gameStream.StreamManifest.Header.IsBigEndian,
                    gameStream.StreamManifest.Header.IsLinked,
                    gameStream.StreamManifest.Header.Version,
                    gameStream.StreamManifest.Header.Checksum,
                    gameStream.StreamManifest.Header.AllTypesHash,
                    gameStream.StreamManifest.Header.AssetCount,
                    gameStream.StreamManifest.Header.BinaryDataSize,
                    gameStream.StreamManifest.Header.MaxBinaryChunkSize,
                    gameStream.StreamManifest.Header.MaxRelocationsChunkSize,
                    gameStream.StreamManifest.Header.MaxImportsChunkSize,
                    gameStream.StreamManifest.Header.AssetReferenceBufferSize,
                    gameStream.StreamManifest.Header.ReferencedManifestNameBufferSize,
                    gameStream.StreamManifest.Header.AssetNameBufferSize,
                    gameStream.StreamManifest.Header.SourceFileNameBufferSize);
                if (gameStream.StreamManifest.StreamReferences.Count != 0)
                {
                    value += "\n\n";
                    for (int idx = 0; idx < gameStream.StreamManifest.StreamReferences.Count; ++idx)
                    {
                        value = string.Format("{0}\n{1}: {2}", value, gameStream.StreamManifest.StreamReferences[idx].ReferenceType, gameStream.StreamManifest.StreamReferences[idx].Path);
                    }
                }
                Content.Text = value;
            }));
        }

        public void SetContentToStreamInfo()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                string value = string.Format(
                    "IsBigEndian: {0}\nIsLinked: {1}\nVersion: {2}\nChecksum: 0x{3:X08}\nAllTypesHash: 0x{4:X08}\nAssetCount {5}\nBinaryDataSize: {6}\nMaxBinaryChunkSize: {7}\nMaxRelocationsChunkSize: {8}\nMaxImportsChunkSize: {9}\nAssetReferenceBufferSize: {10}\nReferencedManifestNameBufferSize: {11}\nAssetNameBufferSize: {12}\nSourceFileNameBufferSize: {13}",
                    streamFiles[0].Header.IsBigEndian,
                    streamFiles[0].Header.IsLinked,
                    streamFiles[0].Header.Version,
                    streamFiles[0].Header.Checksum,
                    streamFiles[0].Header.AllTypesHash,
                    streamFiles[0].Header.AssetCount,
                    streamFiles[0].Header.BinaryDataSize,
                    streamFiles[0].Header.MaxBinaryChunkSize,
                    streamFiles[0].Header.MaxRelocationsChunkSize,
                    streamFiles[0].Header.MaxImportsChunkSize,
                    streamFiles[0].Header.AssetReferenceBufferSize,
                    streamFiles[0].Header.ReferencedManifestNameBufferSize,
                    streamFiles[0].Header.AssetNameBufferSize,
                    streamFiles[0].Header.SourceFileNameBufferSize);
                if (streamFiles[0].StreamReferences.Count != 0)
                {
                    value += "\n\n";
                    for (int idx = 0; idx < streamFiles[0].StreamReferences.Count; ++idx)
                    {
                        value = string.Format("{0}\n{1}: {2}", value, streamFiles[0].StreamReferences[idx].ReferenceType, streamFiles[0].StreamReferences[idx].Path);
                    }
                }
#if DEBUG
                value += "\n\n";
                StringBuilder sb = new StringBuilder();
                uint binOffset = 0;
                foreach (SAGE.Stream.AssetEntry assetEntry in streamFiles[0].AssetEntries)
                {
                    sb.Append(binOffset + ": " + streamFiles[0].AssetNames[assetEntry.NameOffset]);
                    if (assetEntry.BinaryDataSize == 0)
                    {
                        sb.Append(" - from base manifest");
                    }
                    sb.AppendLine();
                    binOffset += assetEntry.BinaryDataSize;
                }
                value += sb.ToString();
#endif
                Content.Text = value;
            }));
        }

        public void SetDecompressedSize(uint value)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (value > 0x100000)
                {
                    Status_DecompessSize.Text = string.Format("{0} {1:n02} {2} ({3})", Size, (double)value / 0x100000, SizeMByte, value);
                }
                else if (value > 0x0400)
                {
                    Status_DecompessSize.Text = string.Format("{0} {1:n02} {2} ({3})", Size, (double)value / 0x0400, SizeKByte, value);
                }
                else
                {
                    Status_DecompessSize.Text = string.Format("{0} {1} {2}", Size, value, SizeByte);
                }
            }));
        }

        public void SetDecompressedSize(uint compressedValue, uint value)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (compressedValue > 0x100000)
                {
                    Status_DecompessSize.Text = string.Format("{0} {1:n02} {2} ({3})", Size, (double)compressedValue / 0x100000, SizeMByte, compressedValue);
                }
                else if (compressedValue > 0x0400)
                {
                    Status_DecompessSize.Text = string.Format("{0} {1:n02} {2} ({3})", Size, (double)compressedValue / 0x0400, SizeKByte, compressedValue);
                }
                else
                {
                    Status_DecompessSize.Text = string.Format("{0} {1} {2}", Size, compressedValue, SizeByte);
                }
                if (value > 0x100000)
                {
                    Status_DecompessSize.Text += string.Format(" {0} {1:n02} {2} ({3})", SizeDecompressed, (double)value / 0x100000, SizeMByte, value);
                }
                else if (value > 0x0400)
                {
                    Status_DecompessSize.Text += string.Format(" {0} {1:n02} {2} ({3})", SizeDecompressed, (double)value / 0x0400, SizeKByte, value);
                }
                else
                {
                    Status_DecompessSize.Text += string.Format(" {0} {1} {2}", SizeDecompressed, value, SizeByte);
                }
                Status_DecompessSize.Text += string.Format(" {0:n02}% Compression", 100 - compressedValue / (float)value * 100);
            }));
        }

        public void SetBarColor(WrathEd.Controls.ProgressBar.ProgressBarColor color)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Progress.Color = color;
            }));
        }

        public void SetBarToNoValue()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Progress.IsProgress = false;
            }));
        }

        public void SetBarToValue(double value)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Progress.IsProgress = true;
                Progress.Value = value;
            }));
        }
    }
}
