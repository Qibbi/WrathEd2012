using System;
using Files;

namespace DDS
{
	public class ARGBMask
	{
		private uint[] masks;

		public uint A { get { return masks[0]; } }
		public uint R { get { return masks[1]; } }
		public uint G { get { return masks[2]; } }
		public uint B { get { return masks[3]; } }

		public ARGBMask(uint[] source)
		{
			masks = source;
		}
	}

	public class ColorBlock
	{
		protected byte[] content;
		protected int offset;

		private uint[] color;
		private long block;

		public ushort Color0 { get { return (ushort)block; } }
		public ushort Color1 { get { return (ushort)(block >> 16); } }
		public bool IsOpaque { get { return Color0 > Color1; } }

		public ColorBlock(byte[] source, int sourceOffset, bool isDXT1 = true)
		{
			content = source;
			offset = sourceOffset;
			block = FileHelper.GetInt64(sourceOffset, source);
			uint channel;
			uint channel2;
			color = new uint[4];
			color[0] = 0x00000000u; // 0xFF000000u
			channel = (uint)(Color0 >> 11) & 0x1F;
			channel = (channel << 3) | (channel >> 2);
			color[0] |= channel << 16;
			channel = (uint)(Color0 >> 5) & 0x3F;
			channel = (channel << 2) | (channel >> 4);
			color[0] |= channel << 8;
			channel = (uint)Color0 & 0x1F;
			channel = (channel << 3) | (channel >> 2);
			color[0] |= channel;
			color[1] = 0x00000000u; // 0xFF000000u
			channel = (uint)(Color1 >> 11) & 0x1F;
			channel = (channel << 3) | (channel >> 2);
			color[1] |= channel << 16;
			channel = (uint)(Color1 >> 5) & 0x3F;
			channel = (channel << 2) | (channel >> 4);
			color[1] |= channel << 8;
			channel = (uint)Color1 & 0x1F;
			channel = (channel << 3) | (channel >> 2);
			color[1] |= channel;
			if (!isDXT1 || IsOpaque)
			{
				color[2] = 0x00000000u; // 0xFF000000u
				color[3] = 0x00000000u; // 0xFF000000u
				channel = (color[0] >> 16) & 0xFF;
				channel2 = (color[1] >> 16) & 0xFF;
				color[2] |= ((2 * channel + channel2 + 1) / 3) << 16;
				color[3] |= ((channel + 2 * channel2 + 1) / 3) << 16;
				channel = (color[0] >> 8) & 0xFF;
				channel2 = (color[1] >> 8) & 0xFF;
				color[2] |= ((2 * channel + channel2 + 1) / 3) << 8;
				color[3] |= ((channel + 2 * channel2 + 1) / 3) << 8;
				channel = color[0] & 0xFF;
				channel2 = color[1] & 0xFF;
				color[2] |= (2 * channel + channel2 + 1) / 3;
				color[3] |= (channel + 2 * channel2 + 1) / 3;
			}
			else
			{
				color[2] = 0xFF000000u;
				color[3] = 0x00000000u;
				channel = (color[0] >> 16) & 0xFF;
				channel2 = (color[1] >> 16) & 0xFF;
				color[2] |= ((channel + channel2) / 2) << 16;
				channel = (color[0] >> 8) & 0xFF;
				channel2 = (color[1] >> 8) & 0xFF;
				color[2] |= ((channel + channel2) / 2) << 8;
				channel = color[0] & 0xFF;
				channel2 = color[1] & 0xFF;
				color[2] |= (channel + channel2) / 2;
			}
		}

		protected byte GetTexel(int x, int y)
		{
			int shifts = ((y << 0x02) + x) << 0x01;
			return (byte)((block >> (shifts + 32)) & 0x03);
		}

		public void GetColor(byte[] target, int x, int y)
		{
			uint value = color[GetTexel(x, y)];
			target[0] = (byte)(value >> 24);
			target[1] = (byte)(value >> 16);
			target[2] = (byte)(value >> 8);
			target[3] = (byte)value;
		}
	}

	public class AlphaBlock
	{
		protected byte[] content;
		protected int offset;

		private byte[] alpha;
		private long block;

		public bool IsCustomExtreme { get { return alpha[0] > alpha[1]; } }

		public AlphaBlock(byte[] source, int sourceOffset)
		{
			content = source;
			offset = sourceOffset;
			block = FileHelper.GetInt64(sourceOffset, source);
			alpha = new byte[8];
			alpha[0] = source[sourceOffset++];
			alpha[1] = source[sourceOffset++];
			if (IsCustomExtreme)
			{
				alpha[2] = (byte)((6 * alpha[0] + alpha[1] + 3) / 7);
				alpha[3] = (byte)((5 * alpha[0] + 2 * alpha[1] + 3) / 7);
				alpha[4] = (byte)((4 * alpha[0] + 3 * alpha[1] + 3) / 7);
				alpha[5] = (byte)((3 * alpha[0] + 4 * alpha[1] + 3) / 7);
				alpha[6] = (byte)((2 * alpha[0] + 5 * alpha[1] + 3) / 7);
				alpha[7] = (byte)((alpha[0] + 6 * alpha[1] + 3) / 7);
			}
			else
			{
				alpha[2] = (byte)((4 * alpha[0] + alpha[1] + 2) / 5);
				alpha[3] = (byte)((3 * alpha[0] + 2 * alpha[1] + 2) / 5);
				alpha[4] = (byte)((2 * alpha[0] + 3 * alpha[1] + 2) / 5);
				alpha[5] = (byte)((alpha[0] + 4 * alpha[1] + 2) / 5);
				alpha[6] = 0x00;
				alpha[7] = 0xFF;
			}
		}

		protected byte GetTexel(int x, int y)
		{
			int shifts = ((y << 0x02) + x) * 3;
			return (byte)((block >> (shifts + 16)) & 0x07);
		}

		public byte GetAlpha(int x, int y)
		{
			return alpha[GetTexel(x, y)];
		}
	}

	public abstract class Color
	{
		protected byte[] content;
		protected ARGBMask ARGBMask;
		protected bool IsNormalMap;

		public Color(ARGBMask mask, bool isNormalMap)
		{
			ARGBMask = mask;
			IsNormalMap = isNormalMap;
		}

		public Color(byte[] source, ARGBMask mask, bool isNormalMap)
		{
			content = source;
			ARGBMask = mask;
			IsNormalMap = isNormalMap;
		}

		protected int GetDistance(byte[] vecA, int vecAOffset, byte[] vecB, int vecBOffset)
		{
			return (vecA[vecAOffset] - vecB[vecBOffset]) * (vecA[vecAOffset] - vecB[vecBOffset])
									+ (vecA[vecAOffset + 1] - vecB[vecBOffset + 1]) * (vecA[vecAOffset + 1] - vecB[vecBOffset + 1])
									+ (vecA[vecAOffset + 2] - vecB[vecBOffset + 2]) * (vecA[vecAOffset + 2] - vecB[vecBOffset + 2]);
		}

		protected int GetDistanceM(byte[] vecA, int vecAOffset, byte[] vecB, int vecBOffset)
		{
			return Math.Abs((vecA[vecAOffset] - vecB[vecBOffset]))
				+ Math.Abs((vecA[vecAOffset + 1] - vecB[vecBOffset + 1]))
				+ Math.Abs((vecA[vecAOffset + 2] - vecB[vecBOffset + 2]));
		}

		protected bool IsSmaller(byte[] vecA, int vecAOffset, byte[] vecB, int vecBOffset)
		{
			int valueA = (vecA[vecAOffset] >> 3) << 0x0B;
			valueA |= (vecA[vecAOffset + 1] >> 2) << 0x05;
			valueA |= vecA[vecAOffset + 2] >> 3;
			int valueB = (vecB[vecBOffset] >> 3) << 0x0B;
			valueB |= (vecB[vecBOffset + 1] >> 2) << 0x05;
			valueB |= vecB[vecBOffset + 2] >> 3;
			if (valueA <= valueB)
			{
				return true;
			}
			return false;
		}

		protected int Luminance(byte[] source, int offset)
		{
			return source[offset] + source[offset + 1] * 2 + source[offset + 2];
		}

		protected void GetMinMaxColorLuminance(byte[] sourceBlock, ref int min, ref int max)
		{
			int maxLuminance = -1;
			int minLuminance = 0x7FFFFFFF;
			for (int idx = 1; idx < 16; idx += 4)
			{
				int luminance = Luminance(sourceBlock, idx);
				if (luminance > maxLuminance)
				{
					maxLuminance = luminance;
					max = idx;
				}
				if (luminance < minLuminance)
				{
					minLuminance = luminance;
					min = idx;
				}
			}
			if (IsSmaller(sourceBlock, max, sourceBlock, min))
			{
				int temp = min;
				min = max;
				max = temp;
			}
		}

		protected void GetMinMaxColor(byte[] sourceBlock, ref int min, ref int max)
		{
			int distance = -1;
			for (int block0 = 1; block0 < 64; block0 += 4)
			{
				for (int block1 = 1; block1 < 64; block1 += 4)
				{
					int currentDistance = GetDistance(sourceBlock, block0, sourceBlock, block1);
					if (currentDistance > distance)
					{
						distance = currentDistance;
						min = block0;
						max = block1;
					}
				}
			}
			if (IsSmaller(sourceBlock, max, sourceBlock, min))
			{
				int temp = min;
				min = max;
				max = temp;
			}
		}

		protected byte[] GetNextMipMap(byte[] source, uint width, uint height)
		{
			byte[] newMipMap = new byte[width * height];
			uint mipWidth = width >> 1;
			uint mipHeight = height >> 1;
			int positionMipMap = 0;
			for (uint idy = 0; idy < mipHeight; ++idy)
			{
				for (uint idx = 0; idx < mipWidth; ++idx)
				{
					int colorA = 0;
					int colorR = 0;
					int colorG = 0;
					int colorB = 0;
					for (int blockY = 0; blockY < 2; ++blockY)
					{
						for (int blockX = 0; blockX < 2; ++blockX)
						{
							colorA += source[idy * width * 8 + blockY * width * 4 + idx * 8 + blockX * 4];
							colorR += source[idy * width * 8 + blockY * width * 4 + idx * 8 + blockX * 4 + 1];
							colorG += source[idy * width * 8 + blockY * width * 4 + idx * 8 + blockX * 4 + 2];
							colorB += source[idy * width * 8 + blockY * width * 4 + idx * 8 + blockX * 4 + 3];
						}
					}
					colorA >>= 2;
					colorR >>= 2;
					colorG >>= 2;
					colorB >>= 2;
					if (IsNormalMap && !(colorR == 0 && colorG == 0 && colorB == 0))
					{
						double colorRN = (colorR - 128) / 128.0;
						double colorGN = (colorG - 128) / 128.0;
						double colorBN = (colorB - 128) / 128.0;
						double length = Math.Sqrt(colorRN * colorRN + colorGN * colorGN + colorBN * colorBN);
						colorR = (int)(colorRN / length * 128) + 128;
						colorG = (int)(colorGN / length * 128) + 128;
						colorB = (int)(colorBN / length * 128) + 128;
					}
					newMipMap[positionMipMap++] = (byte)colorA;
					newMipMap[positionMipMap++] = (byte)colorR;
					newMipMap[positionMipMap++] = (byte)colorG;
					newMipMap[positionMipMap++] = (byte)colorB;
				}
			}
			return newMipMap;
		}

		public abstract byte[] GetColor(uint width, uint height, uint offset = 0);
		public abstract byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1);
	}

	public class ColorA8R8G8B8 : Color
	{
		public ColorA8R8G8B8(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorA8R8G8B8(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			byte[] result = new byte[width * height * 4];
			for (uint idy = 0; idy < height; ++idy)
			{
				for (uint idx = 0; idx < width; ++idx)
				{
					result[idy * width * 4 + idx * 4 + 3] = content[position++];
					result[idy * width * 4 + idx * 4 + 2] = content[position++];
					result[idy * width * 4 + idx * 4 + 1] = content[position++];
					result[idy * width * 4 + idx * 4] = content[position++];
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight * 4;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				for (uint idy = 0; idy < mipHeight; ++idy)
				{
					for (uint idx = 0; idx < mipWidth; ++idx)
					{
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4 + 3];
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4 + 2];
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4 + 1];
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4];
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorR8G8B8 : Color
	{
		public ColorR8G8B8(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorR8G8B8(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			byte[] result = new byte[width * height * 4];
			for (uint idy = 0; idy < height; ++idy)
			{
				for (uint idx = 0; idx < width; ++idx)
				{
					result[idy * width * 4 + idx * 4 + 3] = content[position++];
					result[idy * width * 4 + idx * 4 + 2] = content[position++];
					result[idy * width * 4 + idx * 4 + 1] = content[position++];
					result[idy * width * 4 + idx * 4] = 0xFF;
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight * 3;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				for (uint idy = 0; idy < mipHeight; ++idy)
				{
					for (uint idx = 0; idx < mipWidth; ++idx)
					{
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4 + 3];
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4 + 2];
						content[position++] = sourceMipMap[idy * mipWidth * 4 + idx * 4 + 1];
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorA1R5G5B5 : Color
	{
		public ColorA1R5G5B5(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorA1R5G5B5(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			uint color = 0;
			byte[] result = new byte[width * height * 4];
			int aShifts = 0;
			int rShifts = 0;
			int gShifts = 0;
			int bShifts = 0;
			while (((ARGBMask.A >> aShifts) & 1) == 0)
			{
				++aShifts;
			}
			while (((ARGBMask.R >> rShifts) & 1) == 0)
			{
				++rShifts;
			}
			while (((ARGBMask.G >> gShifts) & 1) == 0)
			{
				++gShifts;
			}
			while (((ARGBMask.B >> bShifts) & 1) == 0)
			{
				++bShifts;
			}
			for (uint idy = 0; idy < height; ++idy)
			{
				for (uint idx = 0; idx < width; ++idx)
				{
					ushort value = (ushort)(content[position++] | (content[position++] << 0x08));
					color = (uint)value >> aShifts;
					result[idy * width * 4 + idx * 4] = (color != 0) ? (byte)0xFF : (byte)0x00;
					color = ((uint)value >> rShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 1] = (byte)((color << 3) | (color >> 2));
					color = ((uint)value >> gShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 2] = (byte)((color << 3) | (color >> 2));
					color = ((uint)value >> bShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 3] = (byte)((color << 3) | (color >> 2));
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight * 2;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			int aShifts = 0;
			int rShifts = 0;
			int gShifts = 0;
			int bShifts = 0;
			while (((ARGBMask.A >> aShifts) & 1) == 0)
			{
				++aShifts;
			}
			while (((ARGBMask.R >> rShifts) & 1) == 0)
			{
				++rShifts;
			}
			while (((ARGBMask.G >> gShifts) & 1) == 0)
			{
				++gShifts;
			}
			while (((ARGBMask.B >> bShifts) & 1) == 0)
			{
				++bShifts;
			}
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				for (uint idy = 0; idy < mipHeight; ++idy)
				{
					for (uint idx = 0; idx < mipWidth; ++idx)
					{
						ushort value = (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4] == 0) ? 0 : 1 << aShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 1] >> 3) << rShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 2] >> 3) << gShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 3] >> 3) << bShifts);
						content[position++] = (byte)value;
						content[position++] = (byte)(value >> 0x08);
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorR5G6B5 : Color
	{
		public ColorR5G6B5(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorR5G6B5(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			uint color = 0;
			byte[] result = new byte[width * height * 4];
			int rShifts = 0;
			int gShifts = 0;
			int bShifts = 0;
			while (((ARGBMask.R >> rShifts) & 1) == 0)
			{
				++rShifts;
			}
			while (((ARGBMask.G >> gShifts) & 1) == 0)
			{
				++gShifts;
			}
			while (((ARGBMask.B >> bShifts) & 1) == 0)
			{
				++bShifts;
			}
			for (uint idy = 0; idy < height; ++idy)
			{
				for (uint idx = 0; idx < width; ++idx)
				{
					ushort value = (ushort)(content[position++] | (content[position++] << 0x08));
					result[idy * width * 4 + idx * 4] = 0xFF;
					color = ((uint)value >> rShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 1] = (byte)((color << 3) | (color >> 2));
					color = ((uint)value >> gShifts) & 0x3F;
					result[idy * width * 4 + idx * 4 + 2] = (byte)((color << 2) | (color >> 4));
					color = ((uint)value >> bShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 3] = (byte)((color << 3) | (color >> 2));
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight * 2;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			int rShifts = 0;
			int gShifts = 0;
			int bShifts = 0;
			while (((ARGBMask.R >> rShifts) & 1) == 0)
			{
				++rShifts;
			}
			while (((ARGBMask.G >> gShifts) & 1) == 0)
			{
				++gShifts;
			}
			while (((ARGBMask.B >> bShifts) & 1) == 0)
			{
				++bShifts;
			}
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				for (uint idy = 0; idy < mipHeight; ++idy)
				{
					for (uint idx = 0; idx < mipWidth; ++idx)
					{
						ushort value = (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 1] >> 3) << rShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 2] >> 2) << gShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 3] >> 3) << bShifts);
						content[position++] = (byte)value;
						content[position++] = (byte)(value >> 0x08);
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorR5G5B5 : Color
	{
		public ColorR5G5B5(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorR5G5B5(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			uint color = 0;
			byte[] result = new byte[width * height * 4];
			int rShifts = 0;
			int gShifts = 0;
			int bShifts = 0;
			while (((ARGBMask.R >> rShifts) & 1) == 0)
			{
				++rShifts;
			}
			while (((ARGBMask.G >> gShifts) & 1) == 0)
			{
				++gShifts;
			}
			while (((ARGBMask.B >> bShifts) & 1) == 0)
			{
				++bShifts;
			}
			for (uint idy = 0; idy < height; ++idy)
			{
				for (uint idx = 0; idx < width; ++idx)
				{
					ushort value = (ushort)(content[position++] | (content[position++] << 0x08));
					result[idy * width * 4 + idx * 4] = 0xFF;
					color = ((uint)value >> rShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 1] = (byte)((color << 3) | (color >> 2));
					color = ((uint)value >> gShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 2] = (byte)((color << 3) | (color >> 2));
					color = ((uint)value >> bShifts) & 0x1F;
					result[idy * width * 4 + idx * 4 + 3] = (byte)((color << 3) | (color >> 2));
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight * 2;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			int rShifts = 0;
			int gShifts = 0;
			int bShifts = 0;
			while (((ARGBMask.R >> rShifts) & 1) == 0)
			{
				++rShifts;
			}
			while (((ARGBMask.G >> gShifts) & 1) == 0)
			{
				++gShifts;
			}
			while (((ARGBMask.B >> bShifts) & 1) == 0)
			{
				++bShifts;
			}
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				for (uint idy = 0; idy < mipHeight; ++idy)
				{
					for (uint idx = 0; idx < mipWidth; ++idx)
					{
						ushort value = (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 1] >> 3) << rShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 2] >> 3) << gShifts);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 3] >> 3) << bShifts);
						content[position++] = (byte)value;
						content[position++] = (byte)(value >> 0x08);
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorA4R4G4B4 : Color
	{
		public ColorA4R4G4B4(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorA4R4G4B4(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			uint color = 0;
			byte[] result = new byte[width * height * 4];
			for (uint idy = 0; idy < height; ++idy)
			{
				for (uint idx = 0; idx < width; ++idx)
				{
					ushort value = (ushort)(content[position++] | (content[position++] << 0x08));
					color = (uint)value >> 0x0C;
					result[idy * width * 4 + idx * 4] = (byte)((color << 4) | color);
					color = ((uint)value >> 0x08) & 0xF;
					result[idy * width * 4 + idx * 4 + 1] = (byte)((color << 4) | color);
					color = ((uint)value >> 0x04) & 0xF;
					result[idy * width * 4 + idx * 4 + 2] = (byte)((color << 4) | color);
					color = (uint)value & 0xF;
					result[idy * width * 4 + idx * 4 + 3] = (byte)((color << 4) | color);
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight * 2;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				for (uint idy = 0; idy < mipHeight; ++idy)
				{
					for (uint idx = 0; idx < mipWidth; ++idx)
					{
						ushort value = (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4] >> 4) << 0x0C);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 1] >> 4) << 0x08);
						value |= (ushort)((sourceMipMap[idy * mipWidth * 4 + idx * 4 + 2] >> 4) << 0x04);
						value |= (ushort)(sourceMipMap[idy * mipWidth * 4 + idx * 4 + 3] >> 4);
						content[position++] = (byte)value;
						content[position++] = (byte)(value >> 0x08);
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorDXT1 : Color
	{
		public ColorDXT1(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorDXT1(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			byte[] texel = new byte[4];
			byte[] result = new byte[width * height * 4];
			uint quaterWidth = width / 4;
			uint quaterHeight = height / 4;
			for (uint idy = 0; idy < quaterWidth; ++idy)
			{
				for (uint idx = 0; idx < quaterHeight; ++idx)
				{
					ColorBlock block = new ColorBlock(content, (int)position);
					position += 8;
					for (int blockY = 0; blockY < 4; ++blockY)
					{
						for (int blockX = 0; blockX < 4; ++blockX)
						{
							block.GetColor(texel, blockX, blockY);
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4] = texel[0];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 1] = texel[1];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 2] = texel[2];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 3] = texel[3];
						}
					}
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight / 2;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				uint quaterWidth = mipWidth / 4;
				uint quaterHeight = mipHeight / 4;
				for (uint idy = 0; idy < quaterHeight; ++idy)
				{
					for (uint idx = 0; idx < quaterWidth; ++idx)
					{
						byte[] block = new byte[64];
						bool hasAlpha = false;
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								block[blockY * 16 + blockX * 4] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4];
								block[blockY * 16 + blockX * 4 + 1] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 1];
								block[blockY * 16 + blockX * 4 + 2] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 2];
								block[blockY * 16 + blockX * 4 + 3] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 3];
								if (block[blockY * 16 + blockX * 4] < 0xFF)
								{
									hasAlpha = true;
								}
							}
						}
						int min = 0;
						int max = 0;
						GetMinMaxColor(block, ref min, ref max);
						int value0 = (block[min] >> 3) << 0x0B;
						value0 |= (block[min + 1] >> 2) << 0x05;
						value0 |= block[min + 2] >> 3;
						int value1 = (block[max] >> 3) << 0x0B;
						value1 |= (block[max + 1] >> 2) << 0x05;
						value1 |= block[max + 2] >> 3;
						if (hasAlpha)
						{
							content[position++] = (byte)value0;
							content[position++] = (byte)(value0 >> 0x08);
							content[position++] = (byte)value1;
							content[position++] = (byte)(value1 >> 0x08);
						}
						else
						{
							content[position++] = (byte)value1;
							content[position++] = (byte)(value1 >> 0x08);
							content[position++] = (byte)value0;
							content[position++] = (byte)(value0 >> 0x08);
							int temp = min;
							min = max;
							max = temp;
						}
						uint lookup = 0;
						if (hasAlpha)
						{
							for (int blockY = 0; blockY < 4; ++blockY)
							{
								for (int blockX = 0; blockX < 4; ++blockX)
								{
									if (block[blockY * 16 + blockX * 4] < 0xFF)
									{
										lookup |= (uint)(3 << ((3 - blockY) * 8 + blockX * 2));
										continue;
									}
									byte[] med = new byte[]
									{
										(byte)((block[min] + block[max]) / 2),
										(byte)((block[min + 1] + block[max + 1]) / 2),
										(byte)((block[min + 2] + block[max + 2]) / 2)
									};
									int distanceToV0 = GetDistance(block, min, block, blockY * 16 + blockX * 4 + 1);
									int distanceToV1 = GetDistance(block, max, block, blockY * 16 + blockX * 4 + 1);
									int distanceToMed = GetDistance(med, 0, block, blockY * 16 + blockX * 4 + 1);
									int minDistance = Math.Min(Math.Min(distanceToV0, distanceToMed), distanceToV1);
									if (distanceToV1 == minDistance)
									{
										lookup |= (uint)(1 << ((3 - blockY) * 8 + blockX * 2));
									}
									else if (distanceToMed == minDistance)
									{
										lookup |= (uint)(2 << ((3 - blockY) * 8 + blockX * 2));
									}
								}
							}
						}
						else
						{
							for (int blockY = 0; blockY < 4; ++blockY)
							{
								for (int blockX = 0; blockX < 4; ++blockX)
								{
									byte[] medLow = new byte[]
										{
											(byte)((2 * block[min] + block[max] + 1) / 3),
											(byte)((2 * block[min + 1] + block[max + 1] + 1) / 3),
											(byte)((2 * block[min + 2] + block[max + 2] + 1) / 3)
										};
									byte[] medHigh = new byte[]
										{
											(byte)((block[min] + 2 * block[max] + 1) / 3),
											(byte)((block[min + 1] + 2 * block[max + 1] + 1) / 3),
											(byte)((block[min + 2] + 2 * block[max + 2] + 1) / 3)
										};
									int distanceToV0 = GetDistance(block, min, block, blockY * 16 + blockX * 4 + 1);
									int distanceToV1 = GetDistance(block, max, block, blockY * 16 + blockX * 4 + 1);
									int distanceToMedLow = GetDistance(medLow, 0, block, blockY * 16 + blockX * 4 + 1);
									int distanceToMedHigh = GetDistance(medHigh, 0, block, blockY * 16 + blockX * 4 + 1);
									int minDistance = Math.Min(Math.Min(Math.Min(distanceToV0, distanceToMedLow), distanceToMedHigh), distanceToV1);
									if (distanceToV0 == minDistance)
									{
										lookup |= (uint)(1 << ((3 - blockY) * 8 + blockX * 2));
									}
									else if (distanceToMedHigh == minDistance)
									{
										lookup |= (uint)(2 << ((3 - blockY) * 8 + blockX * 2));
									}
									else if (distanceToMedLow == minDistance)
									{
										lookup |= (uint)(3 << ((3 - blockY) * 8 + blockX * 2));
									}
								}
							}
						}
						content[position++] = (byte)(lookup >> 24);
						content[position++] = (byte)(lookup >> 16);
						content[position++] = (byte)(lookup >> 8);
						content[position++] = (byte)lookup;
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorDXT3 : Color
	{
		public ColorDXT3(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorDXT3(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			byte[] texel = new byte[4];
			byte[] result = new byte[width * height * 4];
			uint quaterWidth = width / 4;
			uint quaterHeight = height / 4;
			for (uint idy = 0; idy < quaterWidth; ++idy)
			{
				for (uint idx = 0; idx < quaterHeight; ++idx)
				{
					for (int blockY = 0; blockY < 4; ++blockY)
					{
						for (int blockX = 0; blockX < 4; ++blockX)
						{
							if ((blockX & 0x01) == 0x00)
							{
								result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4] = (byte)(((content[position] & 0xF0) >> 4) * 0xFF / 0x0F);
							}
							else
							{
								result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4] = (byte)((content[position] & 0x0F) * 0xFF / 0x0F);
								if (blockX != 0)
								{
									++position;
								}
							}
						}
					}
					ColorBlock block = new ColorBlock(content, (int)position, false);
					position += 8;
					for (int blockY = 0; blockY < 4; ++blockY)
					{
						for (int blockX = 0; blockX < 4; ++blockX)
						{
							block.GetColor(texel, blockX, blockY);
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 1] = texel[1];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 2] = texel[2];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 3] = texel[3];
						}
					}
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				uint quaterWidth = mipWidth / 4;
				uint quaterHeight = mipHeight / 4;
				for (uint idy = 0; idy < quaterHeight; ++idy)
				{
					for (uint idx = 0; idx < quaterWidth; ++idx)
					{
						byte[] block = new byte[64];
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								block[blockY * 16 + blockX * 4] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4];
								block[blockY * 16 + blockX * 4 + 1] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 1];
								block[blockY * 16 + blockX * 4 + 2] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 2];
								block[blockY * 16 + blockX * 4 + 3] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 3];
							}
						}
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 1; blockX >= 0; --blockX)
							{
								content[position++] = (byte)((block[blockY * 16 + blockX * 8 + 4] << 4) | (block[blockY * 16 + blockX * 8] & 0x0F));
							}
						}
						int min = 0;
						int max = 0;
						GetMinMaxColor(block, ref min, ref max);
						int value0 = (block[min] >> 3) << 0x0B;
						value0 |= (block[min + 1] >> 2) << 0x05;
						value0 |= block[min + 2] >> 3;
						int value1 = (block[max] >> 3) << 0x0B;
						value1 |= (block[max + 1] >> 2) << 0x05;
						value1 |= block[max + 2] >> 3;
						content[position++] = (byte)value1;
						content[position++] = (byte)(value1 >> 0x08);
						content[position++] = (byte)value0;
						content[position++] = (byte)(value0 >> 0x08);
						uint lookup = 0;
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								byte[] medLow = new byte[]
									{
										(byte)((2 * block[min] + block[max] + 1) / 3),
										(byte)((2 * block[min + 1] + block[max + 1] + 1) / 3),
										(byte)((2 * block[min + 2] + block[max + 2] + 1) / 3)
									};
								byte[] medHigh = new byte[]
									{
										(byte)((block[min] + 2 * block[max] + 1) / 3),
										(byte)((block[min + 1] + 2 * block[max + 1] + 1) / 3),
										(byte)((block[min + 2] + 2 * block[max + 2] + 1) / 3)
									};
								int distanceToV0 = GetDistance(block, min, block, blockY * 16 + blockX * 4 + 1);
								int distanceToV1 = GetDistance(block, max, block, blockY * 16 + blockX * 4 + 1);
								int distanceToMedLow = GetDistance(medLow, 0, block, blockY * 16 + blockX * 4 + 1);
								int distanceToMedHigh = GetDistance(medHigh, 0, block, blockY * 16 + blockX * 4 + 1);
								int minDistance = Math.Min(Math.Min(Math.Min(distanceToV0, distanceToMedLow), distanceToMedHigh), distanceToV1);
								if (distanceToV0 == minDistance)
								{
									lookup |= (uint)(1 << ((3 - blockY) * 8 + blockX * 2));
								}
								else if (distanceToMedHigh == minDistance)
								{
									lookup |= (uint)(2 << ((3 - blockY) * 8 + blockX * 2));
								}
								else if (distanceToMedLow == minDistance)
								{
									lookup |= (uint)(3 << ((3 - blockY) * 8 + blockX * 2));
								}
							}
						}
						content[position++] = (byte)(lookup >> 24);
						content[position++] = (byte)(lookup >> 16);
						content[position++] = (byte)(lookup >> 8);
						content[position++] = (byte)lookup;
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}

	public class ColorDXT5 : Color
	{
		public ColorDXT5(ARGBMask mask, bool isNormalMap = false)
			: base(mask, isNormalMap)
		{
		}

		public ColorDXT5(byte[] source, ARGBMask mask, bool isNormalMap = false)
			: base(source, mask, isNormalMap)
		{
		}

		public override byte[] GetColor(uint width, uint height, uint offset = 0)
		{
			uint position = offset;
			byte[] texel = new byte[4];
			byte[] result = new byte[width * height * 4];
			uint quaterWidth = width / 4;
			uint quaterHeight = height / 4;
			for (uint idy = 0; idy < quaterWidth; ++idy)
			{
				for (uint idx = 0; idx < quaterHeight; ++idx)
				{
					AlphaBlock aBlock = new AlphaBlock(content, (int)position);
					position += 8;
					for (int blockY = 0; blockY < 4; ++blockY)
					{
						for (int blockX = 0; blockX < 4; ++blockX)
						{
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4] = aBlock.GetAlpha(blockX, blockY);
						}
					}
					ColorBlock block = new ColorBlock(content, (int)position, false);
					position += 8;
					for (int blockY = 0; blockY < 4; ++blockY)
					{
						for (int blockX = 0; blockX < 4; ++blockX)
						{
							block.GetColor(texel, blockX, blockY);
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 1] = texel[1];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 2] = texel[2];
							result[idy * width * 16 + blockY * width * 4 + idx * 16 + blockX * 4 + 3] = texel[3];
						}
					}
				}
			}
			return result;
		}

		public override byte[] CompressColor(byte[] source, uint width, uint height, int mipMaps = 1)
		{
			uint position = 0;
			uint mipWidth = width;
			uint mipHeight = height;
			uint size = 0;
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				size += mipWidth * mipHeight;
				mipWidth /= 2;
				mipHeight /= 2;
			}
			content = new byte[size];
			mipWidth = width;
			mipHeight = height;
			byte[] sourceMipMap = new byte[width * height * 4];
			Array.Copy(source, sourceMipMap, sourceMipMap.Length);
			for (int mipMap = 0; mipMap < mipMaps; ++mipMap)
			{
				uint quaterWidth = mipWidth / 4;
				uint quaterHeight = mipHeight / 4;
				for (uint idy = 0; idy < quaterHeight; ++idy)
				{
					for (uint idx = 0; idx < quaterWidth; ++idx)
					{
						byte[] block = new byte[64];
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								block[blockY * 16 + blockX * 4] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4];
								block[blockY * 16 + blockX * 4 + 1] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 1];
								block[blockY * 16 + blockX * 4 + 2] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 2];
								block[blockY * 16 + blockX * 4 + 3] = sourceMipMap[idy * mipWidth * 16 + blockY * mipWidth * 4 + idx * 16 + blockX * 4 + 3];
							}
						}
						byte minAlpha = 0xFF;
						byte maxAlpha = 0x00;
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								byte newAlpha = block[blockY * 16 + blockX * 4];
								if (newAlpha < minAlpha)
								{
									minAlpha = newAlpha;
								}
								if (newAlpha > maxAlpha)
								{
									maxAlpha = newAlpha;
								}
							}
						}
						byte[] alphas = new byte[8];
						if (minAlpha + 0xFF - maxAlpha > 0x1F)
						{
							alphas[0] = maxAlpha;
							alphas[1] = minAlpha;
							alphas[2] = (byte)((6 * maxAlpha + minAlpha + 3) / 7);
							alphas[3] = (byte)((5 * maxAlpha + 2 * minAlpha + 3) / 7);
							alphas[4] = (byte)((4 * maxAlpha + 3 * minAlpha + 3) / 7);
							alphas[5] = (byte)((3 * maxAlpha + 4 * minAlpha + 3) / 7);
							alphas[6] = (byte)((2 * maxAlpha + 5 * minAlpha + 3) / 7);
							alphas[7] = (byte)((maxAlpha + 6 * minAlpha + 3) / 7);
						}
						else
						{
							alphas[0] = minAlpha;
							alphas[1] = maxAlpha;
							alphas[2] = (byte)((4 * minAlpha + maxAlpha + 2) / 5);
							alphas[3] = (byte)((3 * minAlpha + 2 * maxAlpha + 2) / 5);
							alphas[4] = (byte)((2 * minAlpha + 3 * maxAlpha + 2) / 5);
							alphas[5] = (byte)((minAlpha + 4 * maxAlpha + 2) / 5);
							alphas[6] = 0x00;
							alphas[7] = 0xFF;
						}
						content[position++] = alphas[0];
						content[position++] = alphas[1];
						long alphaLookup = 0;
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								byte newAlpha = block[blockY * 16 + blockX * 4];
								long closest = 0;
								for (int alphaIdx = 1; alphaIdx < 8; ++alphaIdx)
								{
									if (Math.Abs(newAlpha - alphas[closest]) > Math.Abs(newAlpha - alphas[alphaIdx]))
									{
										closest = alphaIdx;
									}
								}
								alphaLookup |= closest << (blockY * 12 + blockX * 3);
							}
						}
						content[position++] = (byte)alphaLookup;
						content[position++] = (byte)(alphaLookup >> 8);
						content[position++] = (byte)(alphaLookup >> 16);
						content[position++] = (byte)(alphaLookup >> 24);
						content[position++] = (byte)(alphaLookup >> 32);
						content[position++] = (byte)(alphaLookup >> 40);
						int min = 0;
						int max = 0;
						GetMinMaxColor(block, ref min, ref max);
						int value0 = (block[min] >> 3) << 0x0B;
						value0 |= (block[min + 1] >> 2) << 0x05;
						value0 |= block[min + 2] >> 3;
						int value1 = (block[max] >> 3) << 0x0B;
						value1 |= (block[max + 1] >> 2) << 0x05;
						value1 |= block[max + 2] >> 3;
						content[position++] = (byte)value1;
						content[position++] = (byte)(value1 >> 0x08);
						content[position++] = (byte)value0;
						content[position++] = (byte)(value0 >> 0x08);
						uint lookup = 0;
						for (int blockY = 0; blockY < 4; ++blockY)
						{
							for (int blockX = 0; blockX < 4; ++blockX)
							{
								byte[] medLow = new byte[]
									{
										(byte)((2 * block[min] + block[max] + 1) / 3),
										(byte)((2 * block[min + 1] + block[max + 1] + 1) / 3),
										(byte)((2 * block[min + 2] + block[max + 2] + 1) / 3)
									};
								byte[] medHigh = new byte[]
									{
										(byte)((block[min] + 2 * block[max] + 1) / 3),
										(byte)((block[min + 1] + 2 * block[max + 1] + 1) / 3),
										(byte)((block[min + 2] + 2 * block[max + 2] + 1) / 3)
									};
								int distanceToV0 = GetDistance(block, min, block, blockY * 16 + blockX * 4 + 1);
								int distanceToV1 = GetDistance(block, max, block, blockY * 16 + blockX * 4 + 1);
								int distanceToMedLow = GetDistance(medLow, 0, block, blockY * 16 + blockX * 4 + 1);
								int distanceToMedHigh = GetDistance(medHigh, 0, block, blockY * 16 + blockX * 4 + 1);
								int minDistance = Math.Min(Math.Min(Math.Min(distanceToV0, distanceToMedLow), distanceToMedHigh), distanceToV1);
								if (distanceToV0 == minDistance)
								{
									lookup |= (uint)(1 << ((3 - blockY) * 8 + blockX * 2));
								}
								else if (distanceToMedHigh == minDistance)
								{
									lookup |= (uint)(2 << ((3 - blockY) * 8 + blockX * 2));
								}
								else if (distanceToMedLow == minDistance)
								{
									lookup |= (uint)(3 << ((3 - blockY) * 8 + blockX * 2));
								}
							}
						}
						content[position++] = (byte)(lookup >> 24);
						content[position++] = (byte)(lookup >> 16);
						content[position++] = (byte)(lookup >> 8);
						content[position++] = (byte)lookup;
					}
				}
				if (mipMap != mipMaps - 1)
				{
					sourceMipMap = GetNextMipMap(sourceMipMap, mipWidth, mipHeight);
					mipWidth /= 2;
					mipHeight /= 2;
				}
			}
			return content;
		}
	}
}
