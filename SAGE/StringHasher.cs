using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SAGE
{
	public static class StringHasher
	{
		private static byte[] _hashSign = { 0x30, 0x78 };
		
		public static void Hash(string value, out uint currentValue)
		{
			byte[] input = Encoding.UTF8.GetBytes(value);
			int numBlocks, extra;
			if (input.Length == 0)
			{
				currentValue = 0;
				return;
			}
			if (input.Length == 10 && input[0] == _hashSign[0] && input[1] == _hashSign[1])
			{
				currentValue = uint.Parse(value.Substring(2), NumberStyles.HexNumber);
				return;
			}
			numBlocks = input.Length / 4;
			extra = input.Length % 4;
			currentValue = (uint)input.Length;
			int idy = 0;
			for (int idx = numBlocks; idx != 0; --idx)
			{
				currentValue += (uint)(input[idy + 1] << 8 | input[idy]);
				currentValue ^= (((uint)(input[idy + 3] << 8 | input[idy + 2])
					^ (currentValue << 5)) << 11);
				currentValue += currentValue >> 11;
				idy += 4;
			}
			if (extra != 0)
			{
				switch (extra)
				{
					case 1:
						{
							currentValue += input[idy];
							currentValue = (currentValue << 10) ^ currentValue;
							currentValue += currentValue >> 1;
							break;
						}
					case 2:
						{
							currentValue += (uint)(input[idy + 1] << 8 | input[idy]);
							currentValue ^= currentValue << 11;
							currentValue += currentValue >> 17;
							break;
						}
					case 3:
						{
							currentValue += (uint)(input[idy + 1] << 8 | input[idy]);
							currentValue ^= (currentValue ^ (uint)(input[idy + 2] << 2)) << 16;
							currentValue += currentValue >> 11;
							break;
						}
				}
			}
			currentValue ^= currentValue << 3;
			currentValue += currentValue >> 5;
			currentValue ^= currentValue << 2;
			currentValue += currentValue >> 15;
			currentValue ^= currentValue << 10;
		}

		public static uint Hash(string value)
		{
			uint result;
			Hash(value, out result);
			return result;
		}
	}
}
