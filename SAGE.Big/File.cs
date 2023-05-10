using Files;
using System;
using System.IO;

namespace SAGE.Big
{
    public class File : IDisposable
    {
        const uint big4 = 0x34474942u;
        const uint bigf = 0x46474942u;

        const int minRefPackBufferLength = 0x00400000;

        private byte[] bigHeader;
        private byte[] refPackBuffer;
        private byte[] refPackHeader;
        private byte[] preBuffer;
        private uint preBufferPosition;

        private string _source;
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
            _source = source;
            bigHeader = new byte[0x10];
            refPackHeader = new byte[0x08];
            preBuffer = new byte[0x00800000];
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

        //[DllImport("SAGE.Compression.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern void Decompress(byte[] buffer, [MarshalAs(UnmanagedType.LPStr)] string filename, uint fileOffset, uint length, uint offset);

        public byte[] Decompress(uint fileOffset, uint length, uint offset = 0)
        {
            /*byte[] result = new byte[length];
			Test(result);
			Decompress(result, _source + '\0', fileOffset, length, offset);
			return result;*/
            refPackBuffer = new byte[Math.Max(minRefPackBufferLength, length)];
            uint refPackBufferLength = (uint)(refPackBuffer.Length);
            int preBufferLength = preBuffer.Length;
            stream.Position = fileOffset;
            PreBuffer();
            preBufferPosition += 2 + (uint)((((preBuffer[0] & 0x80) != 0x00) ? 0x04 : 0x03) * (((preBuffer[0] & 0x01) != 0) ? 0x02 : 0x01));
            byte refPack0 = 0;
            byte refPack1 = 0;
            byte refPack2 = 0;
            byte refPack3 = 0;
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
                refPack0 = preBuffer[preBufferPosition++];
                if ((refPack0 & 0x80) == 0)
                {
                    refPack1 = preBuffer[preBufferPosition++];
                    countPart1 = (uint)refPack0 & 0x03;
                    countPart2 = (((uint)refPack0 & 0x1c) >> 0x02) + 0x02;
                    refPackOffset = (position + countPart1 - 0x01) - (refPack1 + (((uint)refPack0 & 0x60) << 0x03));
                    if (position + countPart1 + 2 < offset + length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == offset + length)
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
                        }
                    }
                    if (position + countPart2 + 1 < offset + length)
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                            if (position == offset + length)
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
                        }
                    }
                }
                else if ((refPack0 & 0x40) == 0)
                {
                    refPack1 = preBuffer[preBufferPosition++];
                    refPack2 = preBuffer[preBufferPosition++];
                    countPart1 = (uint)refPack1 >> 0x06;
                    countPart2 = ((uint)refPack0 & 0x3f) + 0x03;
                    refPackOffset = (position + countPart1 - 0x01) - ((((uint)refPack1 & 0x3f) << 0x08) + refPack2);
                    if (position + countPart1 + 2 < offset + length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == offset + length)
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
                        }
                    }
                    if (position + countPart2 + 1 < offset + length)
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                            if (position == offset + length)
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
                        }
                    }
                }
                else if ((refPack0 & 0x20) == 0)
                {
                    refPack1 = preBuffer[preBufferPosition++];
                    refPack2 = preBuffer[preBufferPosition++];
                    refPack3 = preBuffer[preBufferPosition++];
                    countPart1 = (uint)refPack0 & 0x03;
                    countPart2 = (((uint)refPack0 & 0x0c) << 0x06) + refPack3 + 0x04;
                    refPackOffset = (position + countPart1 - 0x01) - ((((uint)refPack0 & 0x10) << 0x0c) + ((uint)refPack1 << 0x08) + refPack2);
                    if (position + countPart1 + 2 < offset + length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == offset + length)
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
                        }
                    }
                    if (position + countPart2 + 1 < offset + length)
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                            if (position == offset + length)
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
                        }
                    }
                }
                else
                {
                    countPart1 = (((uint)refPack0 & 0x1f) << 0x02) + 0x04;
                    if (countPart1 > 0x70)
                    {
                        countPart1 = (uint)refPack0 & 0x03;
                    }
                    if (position + countPart1 + 2 < offset + length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == offset + length)
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
                        }
                    }
                }
            }
        }

        public void DecompressToDisk(BinaryWriter writer, uint fileOffset, uint length)
        {
            refPackBuffer = new byte[minRefPackBufferLength];
            uint refPackBufferLength = (uint)refPackBuffer.Length;
            int preBufferLength = preBuffer.Length;
            stream.Position = fileOffset;
            PreBuffer();
            preBufferPosition += 2 + (uint)((((preBuffer[0] & 0x80) != 0x00) ? 0x04 : 0x03) * (((preBuffer[0] & 0x01) != 0) ? 0x02 : 0x01));
            byte refPack0 = 0;
            byte refPack1 = 0;
            byte refPack2 = 0;
            byte refPack3 = 0;
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
                refPack0 = preBuffer[preBufferPosition++];
                if ((refPack0 & 0x80) == 0)
                {
                    refPack1 = preBuffer[preBufferPosition++];
                    countPart1 = (uint)refPack0 & 0x03;
                    countPart2 = (((uint)refPack0 & 0x1c) >> 0x02) + 0x02;
                    refPackOffset = (position + countPart1 - 0x01) - (refPack1 + (((uint)refPack0 & 0x60) << 0x03));
                    if (position + countPart1 + 2 < length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                    if (position + countPart2 + 1 < length)
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            writer.Write(refPackBuffer[refPackOffset % refPackBufferLength]);
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            writer.Write(refPackBuffer[refPackOffset % refPackBufferLength]);
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                }
                else if ((refPack0 & 0x40) == 0)
                {
                    refPack1 = preBuffer[preBufferPosition++];
                    refPack2 = preBuffer[preBufferPosition++];
                    countPart1 = (uint)refPack1 >> 0x06;
                    countPart2 = ((uint)refPack0 & 0x3f) + 0x03;
                    refPackOffset = (position + countPart1 - 0x01) - ((((uint)refPack1 & 0x3f) << 0x08) + refPack2);
                    if (position + countPart1 + 2 < length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                    if (position + countPart2 + 1 < length)
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            writer.Write(refPackBuffer[refPackOffset % refPackBufferLength]);
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            writer.Write(refPackBuffer[refPackOffset % refPackBufferLength]);
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                }
                else if ((refPack0 & 0x20) == 0)
                {
                    refPack1 = preBuffer[preBufferPosition++];
                    refPack2 = preBuffer[preBufferPosition++];
                    refPack3 = preBuffer[preBufferPosition++];
                    countPart1 = (uint)refPack0 & 0x03;
                    countPart2 = (((uint)refPack0 & 0x0c) << 0x06) + refPack3 + 0x04;
                    refPackOffset = (position + countPart1 - 0x01) - ((((uint)refPack0 & 0x10) << 0x0c) + ((uint)refPack1 << 0x08) + refPack2);
                    if (position + countPart1 + 2 < length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                    if (position + countPart2 + 1 < length)
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            writer.Write(refPackBuffer[refPackOffset % refPackBufferLength]);
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx <= countPart2; ++idx)
                        {
                            writer.Write(refPackBuffer[refPackOffset % refPackBufferLength]);
                            refPackBuffer[position++ % refPackBufferLength] = refPackBuffer[refPackOffset++ % refPackBufferLength];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    countPart1 = (((uint)refPack0 & 0x1f) << 0x02) + 0x04;
                    if (countPart1 > 0x70)
                    {
                        countPart1 = (uint)refPack0 & 0x03;
                    }
                    if (position + countPart1 + 2 < length)
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                        }
                    }
                    else
                    {
                        for (uint idx = 0; idx < countPart1; ++idx)
                        {
                            writer.Write(preBuffer[preBufferPosition]);
                            refPackBuffer[position++ % refPackBufferLength] = preBuffer[preBufferPosition++];
                            if (position == length)
                            {
                                refPackBuffer = null;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public bool GetIsFileCompressed(PackedFile file)
        {
            stream.Position = file.Offset;
            stream.Read(refPackHeader, 0, 0x08);
            return IsFileCompressed();
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
                if (string.Equals(file.Name, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return GetFile(file, length, offset);
                }
            }
            return null;
        }

        public bool Contains(string file)
        {
            for (int idx = 0; idx < Files.Length; ++idx)
            {
                if (file == Files[idx].Name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(PackedFile file)
        {
            for (int idx = 0; idx < Files.Length; ++idx)
            {
                if (file == Files[idx])
                {
                    return true;
                }
            }
            return false;
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
