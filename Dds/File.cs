using System;
using Files;

namespace DDS
{
	public class File
	{
		public const string DDS_MAGIC = "DDS ";

		private byte[] header;
		private byte[] content;

		public Header Header;
		public Color Content;
		public DDSType DDSType;

		public string Identifier
		{
			get
			{
				return FileHelper.GetString(0x00, header, 0x04);
			}
			set
			{
				FileHelper.SetString(value, 0x00, header);
			}
		}

		public byte[] Binary
		{
			get
			{
				byte[] result = new byte[header.Length + content.Length];
				Array.Copy(header, result, header.Length);
				Array.Copy(content, 0, result, header.Length, content.Length);
				return result;
			}
		}

		public File(uint width, uint height, int mipMaps, DDSType type, byte[] source, bool isNormalMap = false)
		{
			header = new byte[0x80];
			FileHelper.SetString(DDS_MAGIC, 0, header);
			Header = new Header(header);
			Header.Size = 0x7C;
			Header.Flags = DDSFlags.CAPS | DDSFlags.HEIGHT | DDSFlags.WIDTH | DDSFlags.PIXELFORMAT;
			Header.Height = height;
			Header.Width = width;
			Header.Caps = DDSCaps.TEXTURE;
			Header.DDSPF.Size = 0x20;
			switch (type)
			{
				case DDS.DDSType.A8R8G8B8:
					Header.DDSPF.Flags = DDSPixelFormatFlags.ALPHAPIXELS | DDSPixelFormatFlags.RGB;
					Header.DDSPF.BitCount = 0x20;
					Header.DDSPF.RBitMask = 0x00FF0000;
					Header.DDSPF.GBitMask = 0x0000FF00;
					Header.DDSPF.BBitMask = 0x000000FF;
					Header.DDSPF.ABitMask = 0xFF000000;
					break;
				case DDS.DDSType.A1R5G5B5:
					Header.DDSPF.Flags = DDSPixelFormatFlags.ALPHAPIXELS | DDSPixelFormatFlags.RGB;
					Header.DDSPF.BitCount = 0x10;
					Header.DDSPF.RBitMask = 0x00007C00;
					Header.DDSPF.GBitMask = 0x000003E0;
					Header.DDSPF.BBitMask = 0x0000001F;
					Header.DDSPF.ABitMask = 0x00008000;
					break;
				case DDS.DDSType.A4R4G4B4:
					Header.DDSPF.Flags = DDSPixelFormatFlags.ALPHAPIXELS | DDSPixelFormatFlags.RGB;
					Header.DDSPF.BitCount = 0x10;
					Header.DDSPF.RBitMask = 0x00000F00;
					Header.DDSPF.GBitMask = 0x000000F0;
					Header.DDSPF.BBitMask = 0x0000000F;
					Header.DDSPF.ABitMask = 0x0000F000;
					break;
				case DDS.DDSType.R8G8B8:
					Header.DDSPF.Flags = DDSPixelFormatFlags.RGB;
					Header.DDSPF.BitCount = 0x18;
					Header.DDSPF.RBitMask = 0x00FF0000;
					Header.DDSPF.GBitMask = 0x0000FF00;
					Header.DDSPF.BBitMask = 0x000000FF;
					Header.DDSPF.ABitMask = 0x00000000;
					break;
				case DDS.DDSType.R5G6B5:
					Header.DDSPF.Flags = DDSPixelFormatFlags.RGB;
					Header.DDSPF.BitCount = 0x10;
					Header.DDSPF.RBitMask = 0x0000F800;
					Header.DDSPF.GBitMask = 0x000007E0;
					Header.DDSPF.BBitMask = 0x0000001F;
					Header.DDSPF.ABitMask = 0x00000000;
					break;
				case DDS.DDSType.R5G5B5:
					Header.DDSPF.Flags = DDSPixelFormatFlags.RGB;
					Header.DDSPF.BitCount = 0x10;
					Header.DDSPF.RBitMask = 0x00007C00;
					Header.DDSPF.GBitMask = 0x000003E0;
					Header.DDSPF.BBitMask = 0x0000001F;
					Header.DDSPF.ABitMask = 0x00000000;
					break;
				case DDS.DDSType.DXT1:
					Header.DDSPF.Flags = DDSPixelFormatFlags.FOURCC;
					Header.DDSPF.FourCC = "DXT1";
					break;
				case DDS.DDSType.DXT3:
					Header.DDSPF.Flags = DDSPixelFormatFlags.FOURCC;
					Header.DDSPF.FourCC = "DXT3";
					break;
				case DDS.DDSType.DXT5:
					Header.DDSPF.Flags = DDSPixelFormatFlags.FOURCC;
					Header.DDSPF.FourCC = "DXT5";
					break;

			}
			if (mipMaps > 1)
			{
				Header.Flags |= DDSFlags.MIPMAPCOUNT | DDSFlags.LINEARSIZE;
				Header.PitchOrLinearSize = width * height * (Header.DDSPF.BitCount >> 3);
				Header.MipMapCount = (uint)mipMaps;
				Header.Caps |= DDSCaps.COMPLEX | DDSCaps.MIPMAP;
			}
			ARGBMask mask = new ARGBMask(new uint[] { Header.DDSPF.ABitMask, Header.DDSPF.RBitMask, Header.DDSPF.GBitMask, Header.DDSPF.BBitMask });
			switch (type)
			{
				case DDS.DDSType.A8R8G8B8:
					Content = new ColorA8R8G8B8(mask, isNormalMap);
					break;
				case DDS.DDSType.A1R5G5B5:
					Content = new ColorA1R5G5B5(mask, isNormalMap);
					break;
				case DDS.DDSType.A4R4G4B4:
					Content = new ColorA4R4G4B4(mask, isNormalMap);
					break;
				case DDS.DDSType.R8G8B8:
					Content = new ColorR8G8B8(mask, isNormalMap);
					break;
				case DDS.DDSType.R5G6B5:
					Content = new ColorR5G6B5(mask, isNormalMap);
					break;
				case DDS.DDSType.R5G5B5:
					Content = new ColorR5G5B5(mask, isNormalMap);
					break;
				case DDS.DDSType.DXT1:
					Content = new ColorDXT1(mask, isNormalMap);
					break;
				case DDS.DDSType.DXT3:
					Content = new ColorDXT3(mask, isNormalMap);
					break;
				case DDS.DDSType.DXT5:
					Content = new ColorDXT5(mask, isNormalMap);
					break;
			}
			content = Content.CompressColor(source, width, height, mipMaps);
		}

		public File(byte[] source, bool isNormalMap = false)
		{
			header = new byte[0x80];
			Array.Copy(source, header, 0x80);
			Header = new Header(header);
			content = new byte[source.Length - 0x80];
			Array.Copy(source, 0x80, content, 0x00, content.Length);
			DDSType = GetDDSType();
			ARGBMask mask = new ARGBMask(new uint[] { Header.DDSPF.ABitMask, Header.DDSPF.RBitMask, Header.DDSPF.GBitMask, Header.DDSPF.BBitMask });
			switch (DDSType)
			{
				case DDS.DDSType.A8R8G8B8:
					Content = new ColorA8R8G8B8(content, mask, isNormalMap);
					break;
				case DDS.DDSType.R8G8B8:
					Content = new ColorR8G8B8(content, mask, isNormalMap);
					break;
				case DDS.DDSType.A1R5G5B5:
					Content = new ColorA1R5G5B5(content, mask, isNormalMap);
					break;
				case DDS.DDSType.A4R4G4B4:
					Content = new ColorA4R4G4B4(content, mask, isNormalMap);
					break;
				case DDS.DDSType.R5G6B5:
					Content = new ColorR5G6B5(content, mask, isNormalMap);
					break;
				case DDS.DDSType.R5G5B5:
					Content = new ColorR5G5B5(content, mask, isNormalMap);
					break;
				case DDS.DDSType.DXT1:
					Content = new ColorDXT1(content, null, isNormalMap);
					break;
				case DDS.DDSType.DXT3:
					Content = new ColorDXT3(content, null, isNormalMap);
					break;
				case DDS.DDSType.DXT5:
					Content = new ColorDXT5(content, null, isNormalMap);
					break;
			}
		}

		public DDSType GetDDSType()
		{
			if (Header.DDSPF.RBitMask == 0x7C00 && Header.DDSPF.GBitMask == 0x03E0 && Header.DDSPF.BBitMask == 0x001F)
			{
				return DDSType.A1R5G5B5;
			}
			if (Header.DDSPF.Flags.HasFlag(DDSPixelFormatFlags.RGB))
			{
				if (Header.DDSPF.Flags.HasFlag(DDSPixelFormatFlags.ALPHAPIXELS))
				{
					switch (Header.DDSPF.BitCount)
					{
						case 0x10:
							return (Header.DDSPF.ABitMask == 0x00F00000) ? DDSType.A4R4G4B4 : DDSType.A1R5G5B5;
						case 0x20:
							return DDSType.A8R8G8B8;
					}
				}
				else
				{
					switch (Header.DDSPF.BitCount)
					{
						case 0x10:
							return (Header.DDSPF.RBitMask == 0x00007C00) ? DDSType.R5G5B5 : DDS.DDSType.R5G6B5;
						case 0x18:
							return DDSType.R8G8B8;
					}
				}
			}
			else if (Header.DDSPF.Flags.HasFlag(DDSPixelFormatFlags.FOURCC))
			{
				switch (Header.DDSPF.FourCC)
				{
					case "DXT1":
						return DDSType.DXT1;
					case "DXT3":
						return DDSType.DXT3;
					case "DXT5":
						return DDSType.DXT5;
				}
			}
			return DDSType.UNKNOWN;
		}

		public bool HasAlpha()
		{
			switch (DDSType)
			{
				case DDS.DDSType.R8G8B8:
				case DDS.DDSType.R5G6B5:
				case DDS.DDSType.R5G5B5:
					return false;
				default:
					return true;
			}
		}

		public bool IsCubeMap()
		{
			return Header.Caps2.HasFlag(DDSCaps2.CUBEMAP);
		}
	}
}
