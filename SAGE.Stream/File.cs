using System;
using System.Collections.Generic;
using System.Linq;
using Files;

namespace SAGE.Stream
{
	public class File
	{
		public Header Header;
		public List<AssetEntry> AssetEntries;
		public List<AssetReference> AssetReferences;
		public List<StreamReference> StreamReferences;
		public Dictionary<uint, string> AssetNames;
		public Dictionary<uint, string> SourceFiles;

		public File(short version)
		{
			switch (version)
			{
				case 5:
					Header = new Header5();
					break;
				case 6:
					Header = new Header6();
					break;
				case 7:
					Header = new Header7();
					break;
				default:
					throw new ArgumentException(string.Format("Version {0} is unknown!", version));
			}
			AssetEntries = new List<AssetEntry>();
			AssetReferences = new List<AssetReference>();
			StreamReferences = new List<StreamReference>();
			AssetNames = new Dictionary<uint, string>();
			SourceFiles = new Dictionary<uint, string>();
		}

		public File(byte[] source)
		{
			int position = 0;
			byte[] header = new byte[0x30];
			Array.Copy(source, position, header, 0, 0x30);
			Header = new Header5(header);
			switch (Header.Version)
			{
				case 5:
					position = 0x30;
					break;
				case 6:
					Header = new Header6(header);
					position = 0x30;
					break;
				default:
					header = new byte[0x34];
					Array.Copy(source, position, header, 0, 0x34);
					Header = new Header7(header);
					position = 0x34;
					if (Header.Version != 7)
					{
						throw new ArgumentException("Unkown stream version!");
					}
					break;
			}
			AssetEntries = new List<AssetEntry>();
			for (int idx = 0; idx < Header.AssetCount; ++idx)
			{
				switch (Header.Version)
				{
					case 5:
						header = new byte[0x2C];
						Array.Copy(source, position, header, 0, 0x2C);
						AssetEntries.Add(new AssetEntry5(header));
						position += 0x2C;
						break;
					case 6:
						header = new byte[0x30];
						Array.Copy(source, position, header, 0, 0x30);
						AssetEntries.Add(new AssetEntry6(header));
						position += 0x30;
						break;
					case 7:
						header = new byte[0x30];
						Array.Copy(source, position, header, 0, 0x30);
						AssetEntries.Add(new AssetEntry7(header));
						position += 0x30;
						break;
				}
			}
			AssetReferences = new List<AssetReference>();
			int bufferPosition = position;
			for (int idx = 0; idx < Header.AssetReferenceBufferSize / 8; ++idx)
			{
				header = new byte[0x08];
				Array.Copy(source, position, header, 0, 0x08);
				AssetReferences.Add(new AssetReference(header));
				position += 0x08;
			}
			StreamReferences = new List<StreamReference>();
			bufferPosition += (int)(Header.AssetReferenceBufferSize);
			int length = 0;
			for (int idx = 0; idx < Header.ReferencedManifestNameBufferSize; ++idx)
			{
				if (source[bufferPosition + idx] == 0x00)
				{
					length = bufferPosition + idx - position;
					header = new byte[length];
					Array.Copy(source, position, header, 0, length);
					StreamReferences.Add(new StreamReference(header));
					position += length + 1;
				}
			}
			AssetNames = new Dictionary<uint, string>();
			bufferPosition += (int)(Header.ReferencedManifestNameBufferSize);
			for (int idx = 0; idx < Header.AssetCount; ++idx)
			{
				if (!AssetNames.ContainsKey(AssetEntries[idx].NameOffset))
				{
					AssetNames.Add(AssetEntries[idx].NameOffset, FileHelper.GetString(bufferPosition + (int)(AssetEntries[idx].NameOffset), source));
				}
			}
			SourceFiles = new Dictionary<uint, string>();
			bufferPosition += (int)(Header.AssetNameBufferSize);
			for (int idx = 0; idx < Header.AssetCount; ++idx)
			{
				if (!SourceFiles.ContainsKey(AssetEntries[idx].SourceFileNameOffset))
				{
					SourceFiles.Add(AssetEntries[idx].SourceFileNameOffset, FileHelper.GetString(bufferPosition + (int)(AssetEntries[idx].SourceFileNameOffset), source));
				}
			}
		}

		public void Write(System.IO.BinaryWriter writer)
		{
			Header.Write(writer);
			foreach (AssetEntry entry in AssetEntries)
			{
				entry.Write(writer);
			}
			foreach (AssetReference reference in AssetReferences)
			{
				reference.Write(writer);
			}
			foreach (StreamReference reference in StreamReferences)
			{
				reference.Write(writer);
			}
			var orderedNames = from c in AssetNames.Keys
							   orderby c
							   select c;
			foreach (uint assetNameOffset in orderedNames.ToList())
			{
				string assetName = AssetNames[assetNameOffset];
				byte[] stringBuffer = new byte[assetName.Length + 1];
				FileHelper.SetString(assetName, 0, stringBuffer);
				writer.Write(stringBuffer);
			}
			orderedNames = from c in SourceFiles.Keys
						   orderby c
						   select c;
			foreach (uint sourceFileOffset in orderedNames.ToList())
			{
				string sourceName = SourceFiles[sourceFileOffset];
				byte[] stringBuffer = new byte[sourceName.Length + 1];
				FileHelper.SetString(sourceName, 0, stringBuffer);
				writer.Write(stringBuffer);
			}
		}
	}
}
