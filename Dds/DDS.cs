using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDS
{
	[Flags]
	public enum DDSFlags
	{
		CAPS = 1 << 0,
		HEIGHT = 1 << 1,
		WIDTH = 1 << 2,
		PITCH = 1 << 3,
		PIXELFORMAT = 1 << 12,
		MIPMAPCOUNT = 1 << 17,
		LINEARSIZE = 1 << 19,
		DEPTH = 1 << 23
	}

	[Flags]
	public enum DDSPixelFormatFlags
	{
		ALPHAPIXELS = 1 << 0,
		ALPHA = 1 << 1,
		FOURCC = 1 << 2,
		RGB = 1 << 6,
		YUV = 1 << 9,
		LUMINANCE = 1 << 17
	}

	[Flags]
	public enum DDSCaps
	{
		COMPLEX = 1 << 3,
		TEXTURE = 1 << 12,
		MIPMAP = 1 << 22
	}

	[Flags]
	public enum DDSCaps2
	{
		CUBEMAP = 1 << 9,
		CUBEMAP_POSITIVEX = 1 << 10,
		CUBEMAP_NEGATIVEX = 1 << 11,
		CUBEMAP_POSITIVEY = 1 << 12,
		CUBEMAP_NEGATIVEY = 1 << 13,
		CUBEMAP_POSITIVEZ = 1 << 14,
		CUBEMAP_NEGATIVEZ = 1 << 15,
		VOLUME = 1 << 21
	}

	public enum DDSType
	{
		A8R8G8B8,
		A1R5G5B5,
		A4R4G4B4,
		R8G8B8,
		R5G6B5,
		R5G5B5,
		DXT1,
		DXT3,
		DXT5,
		UNKNOWN
	}
}
