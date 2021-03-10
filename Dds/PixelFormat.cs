using System;
using Files;

namespace DDS
{
	public class PixelFormat
	{
		protected const int offset = 0x4C;

		protected byte[] header;

		public uint Size
		{
			get
			{
				return FileHelper.GetUInt(offset + 0x00, header);
			}
			set
			{
				FileHelper.SetUInt(value, offset + 0x00, header);
			}
		}

		public DDSPixelFormatFlags Flags
		{
			get
			{
				return (DDSPixelFormatFlags)FileHelper.GetUInt(offset + 0x04, header);
			}
			set
			{
				FileHelper.SetUInt((uint)value, offset + 0x04, header);
			}
		}

		public string FourCC
		{
			get
			{
				return FileHelper.GetString(offset + 0x08, header, 0x04);
			}
			set
			{
				FileHelper.SetString(value, offset + 0x08, header);
			}
		}

		public uint BitCount
		{
			get
			{
				return FileHelper.GetUInt(offset + 0x0C, header);
			}
			set
			{
				FileHelper.SetUInt(value, offset + 0x0C, header);
			}
		}

		public uint RBitMask
		{
			get
			{
				return FileHelper.GetUInt(offset + 0x10, header);
			}
			set
			{
				FileHelper.SetUInt(value, offset + 0x10, header);
			}
		}

		public uint GBitMask
		{
			get
			{
				return FileHelper.GetUInt(offset + 0x14, header);
			}
			set
			{
				FileHelper.SetUInt(value, offset + 0x14, header);
			}
		}

		public uint BBitMask
		{
			get
			{
				return FileHelper.GetUInt(offset + 0x18, header);
			}
			set
			{
				FileHelper.SetUInt(value, offset + 0x18, header);
			}
		}

		public uint ABitMask
		{
			get
			{
				return FileHelper.GetUInt(offset + 0x1C, header);
			}
			set
			{
				FileHelper.SetUInt(value, offset + 0x1C, header);
			}
		}

		public PixelFormat(byte[] source)
		{
			header = source;
		}
	}
}
