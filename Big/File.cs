using System;
using System.IO;

namespace Big
{
	public class File : IDisposable
	{
		const uint big4 = 0x34474942u;
		const uint bigf = 0x46474942u;

		const int minRefPackBufferLength = 0x00020000;

		private byte[] bigHeader;
		private byte[] refPackBuffer;
		private byte[] refPackHeader;
		private byte[] preBuffer;
		private uint preBufferPosition;

		private FileStream stream;

		public PackedFile[] Files { get; private set; }

		public uint BigType { get { return FileHelper.GetUInt(0x00, bigHeader); } }
		public uint BigSize { get { return FileHelper.GetUInt(0x04, bigHeader); } }
		public uint FileCount { get { return FileHelper.Invert(FileHelper.GetUInt(0x08, bigHeader)); } }
		public uint HeaderSize { get { return FileHelper.Invert(FileHelper.GetUInt(0x0C, bigHeader)); } }

		public File(string source)
		{
			if (!System.IO.File.Exists(source))
			{
				return;
			}
			bigHeader = new byte[0x10];
			refPackHeader = new byte[0x08];
			preBuffer = new byte[0x800000];
			stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
			stream.Read(bigHeader, 0, 0x10);
			stream.Position = 0;
			uint headerSize = HeaderSize;
			stream.Read((bigHeader = new byte[headerSize]), 0, (int)headerSize);
			Files = new PackedFile[FileCount];
			int position = 0x10;
			for (int idx = 0; idx < FileCount; ++idx)
			{
				Files[idx] = new PackedFile();
				Files[idx].Offset = FileHelper.Invert(FileHelper.GetUInt(position, bigHeader));
				position += 0x04;
				Files[idx].Size = FileHelper.Invert(FileHelper.GetUInt(position, bigHeader));
				position += 0x04;
				Files[idx].Name = FileHelper.GetString(position, bigHeader);
				position += Files[idx].Name.Length + 1;
			}
		}

		private bool IsFileCompressed()
		{
			if ((refPackHeader[0x00] & 0x3e) == 0x10 && refPackHeader[0x01] == 0xfb)
			{
				return true;
			}
			return false;
		}

		public void PreBuffer()
		{
			stream.Read(preBuffer, 0, preBuffer.Length);
			preBufferPosition = 0;
		}

		public byte[] Decompress(uint fileOffset, uint length, uint offset = 0)
		{
			refPackBuffer = new byte[Math.Max(minRefPackBufferLength, length)];
			uint refPackBufferLength = (uint)(refPackBuffer.Length);
			int preBufferLength = preBuffer.Length;
			stream.Position = fileOffset;
			PreBuffer();
			preBufferPosition += 2 + (uint)((((preBuffer[0] & 0x80) != 0x00) ? 0x04 : 0x03) * (((preBuffer[0] & 0x01) != 0) ? 0x02 : 0x01));
			byte[] compressed = new byte[0x04];
#if !SPEED_EXTRACT
			bool isContinue = false;
#endif
			uint countPart1 = 0;
			uint countPart2 = 0;
			uint position = 0;
			uint refPackOffset = 0;
			while (true)
			{
				if (preBufferPosition >= preBufferLength - 0x80)
				{
					stream.Position -= preBufferLength - preBufferPosition;
					PreBuffer();
				}
				compressed[0] = preBuffer[preBufferPosition++];
				if ((compressed[0] & 0x80) == 0)
				{
					compressed[1] = preBuffer[preBufferPosition++];
					countPart1 = (uint)compressed[0] & 0x03;
					refPackOffset = (position + countPart1 - 0x01) - ((uint)compressed[1] + (((uint)compressed[0] & 0x60) << 0x03));
					countPart2 = (((uint)compressed[0] & 0x1c) >> 0x02) + 0x02;
				}
				else if ((compressed[0] & 0x40) == 0)
				{
					compressed[1] = preBuffer[preBufferPosition++];
					compressed[2] = preBuffer[preBufferPosition++];
					countPart1 = (uint)compressed[1] >> 0x06;
					refPackOffset = (position + countPart1 - 0x01) - ((((uint)compressed[1] & 0x3f) << 0x08) + compressed[2]);
					countPart2 = ((uint)compressed[0] & 0x3f) + 0x03;
				}
				else if ((compressed[0] & 0x20) == 0)
				{
					compressed[1] = preBuffer[preBufferPosition++];
					compressed[2] = preBuffer[preBufferPosition++];
					compressed[3] = preBuffer[preBufferPosition++];
					countPart1 = (uint)compressed[0] & 0x03;
					refPackOffset = (position + countPart1 - 0x01) - ((((uint)compressed[0] & 0x10) << 0x0c) + ((uint)compressed[1] << 0x08) + compressed[2]);
					countPart2 = (((uint)compressed[0] & 0x0c) << 0x06) + compressed[3] + 0x04;
				}
				else
				{
					countPart1 = (((uint)compressed[0] & 0x1f) << 0x02) + 0x04;
					if (countPart1 > 0x70)
					{
						countPart1 = (uint)compressed[0] & 0x03;
					}
					isContinue = true;
				}
				for (uint idx = 0; idx < countPart1; ++idx)
				{
					refPackBuffer[position % refPackBufferLength] = preBuffer[preBufferPosition++];
					if (++position == offset + length)
					{
						return ReturnDecompressed(position, offset, length, refPackBufferLength);
					}
				}
				if (isContinue)
				{
					isContinue = false;
					continue;
				}
				for (uint idx = 0; idx <= countPart2; ++idx)
				{
					refPackBuffer[position % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
					if (++position == offset + length)
					{
						return ReturnDecompressed(position, offset, length, refPackBufferLength);
					}
				}
			}
		}

		private byte[] ReturnDecompressed(uint position, uint offset, uint length, uint refPackBufferLength)
		{
			byte[] result = new byte[length];
			if (position % refPackBufferLength < length)
			{
				int part1Length = (int)(refPackBufferLength - (offset % refPackBufferLength));
				Array.Copy(refPackBuffer, (int)(offset % refPackBufferLength), result, 0, part1Length);
				Array.Copy(refPackBuffer, 0, result, part1Length, (int)(length - part1Length));
			}
			else
			{
				Array.Copy(refPackBuffer, (int)(offset % refPackBufferLength), result, 0, (int)length);
			}
			refPackBuffer = null;
			return result;
		}

		public uint GetFileSize(PackedFile file)
		{
			stream.Position = file.Offset;
			stream.Read(refPackHeader, 0, 0x08);
			if (IsFileCompressed())
			{
				int size = (((refPackHeader[0x02] << 0x08) + refPackHeader[0x03]) << 0x08) + refPackHeader[0x04];
				if ((refPackHeader[0x00] & 0x80) != 0)
				{
					size = (size << 0x08) + refPackHeader[0x05];
				}
				return (uint)size;
			}
			return file.Size;
		}

		public byte[] GetFile(PackedFile file, uint length, uint offset = 0)
		{
			byte[] result;
			stream.Position = file.Offset;
			stream.Read(refPackHeader, 0, 0x02);
			if (IsFileCompressed())
			{
				result = Decompress(file.Offset, length, offset);
			}
			else
			{
				result = new byte[length];
				stream.Position = file.Offset + offset;
				stream.Read(result, 0, (int)length);
			}
			return result;
		}

		public byte[] GetFile(string fileName, uint length, uint offset = 0)
		{
			foreach (PackedFile file in Files)
			{
				if (file.Name == fileName)
				{
					return GetFile(file, length, offset);
				}
			}
			return null;
		}

		public void Dispose()
		{
			if (stream != null)
			{
				stream.Close();
			}
		}
	}
}
