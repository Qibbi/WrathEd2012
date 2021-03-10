using System;
using Files;

namespace DDS
{
	public class Header
	{
		protected byte[] header;
		protected PixelFormat ddsPF;

		public uint Size
		{
			get
			{
				return FileHelper.GetUInt(0x04, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x04, header);
			}
		}

		public DDSFlags Flags
		{
			get
			{
				return (DDSFlags)FileHelper.GetUInt(0x08, header);
			}
			set
			{
				FileHelper.SetUInt((uint)value, 0x08, header);
			}
		}

		public uint Height
		{
			get
			{
				return FileHelper.GetUInt(0x0C, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x0C, header);
			}
		}

		public uint Width
		{
			get
			{
				return FileHelper.GetUInt(0x10, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x10, header);
			}
		}

		public uint PitchOrLinearSize
		{
			get
			{
				return FileHelper.GetUInt(0x14, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x14, header);
			}
		}

		public uint Depth
		{
			get
			{
				return FileHelper.GetUInt(0x18, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x18, header);
			}
		}

		public uint MipMapCount
		{
			get
			{
				return FileHelper.GetUInt(0x1C, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x1C, header);
			}
		}

		public uint[] Reserved
		{
			get
			{
				uint[] result = new uint[0x0B];
				result[0] = FileHelper.GetUInt(0x20, header);
				result[1] = FileHelper.GetUInt(0x24, header);
				result[2] = FileHelper.GetUInt(0x28, header);
				result[3] = FileHelper.GetUInt(0x2C, header);
				result[4] = FileHelper.GetUInt(0x30, header);
				result[5] = FileHelper.GetUInt(0x34, header);
				result[6] = FileHelper.GetUInt(0x38, header);
				result[7] = FileHelper.GetUInt(0x3C, header);
				result[8] = FileHelper.GetUInt(0x40, header);
				result[9] = FileHelper.GetUInt(0x44, header);
				result[10] = FileHelper.GetUInt(0x48, header);
				return result;
			}
		}

		public PixelFormat DDSPF
		{
			get
			{
				return ddsPF;
			}
		}

		public DDSCaps Caps
		{
			get
			{
				return (DDSCaps)FileHelper.GetUInt(0x6C, header);
			}
			set
			{
				FileHelper.SetUInt((uint)value, 0x6C, header);
			}
		}

		public DDSCaps2 Caps2
		{
			get
			{
				return (DDSCaps2)FileHelper.GetUInt(0x70, header);
			}
			set
			{
				FileHelper.SetUInt((uint)value, 0x70, header);
			}
		}

		public uint Caps3
		{
			get
			{
				return FileHelper.GetUInt(0x74, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x74, header);
			}
		}

		public uint Caps4
		{
			get
			{
				return FileHelper.GetUInt(0x78, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x78, header);
			}
		}

		public uint NotUsed
		{
			get
			{
				return FileHelper.GetUInt(0x7C, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x7C, header);
			}
		}

		public Header(byte[] source)
		{
			header = source;
			ddsPF = new PixelFormat(source);
		}
	}
}
