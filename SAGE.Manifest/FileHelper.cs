using System;
using System.Text;

namespace SAGE.Manifest
{
	public static class FileHelper
	{
		public static bool GetBool(int address, byte[] target)
		{
			return BitConverter.ToBoolean(target, address);
		}
		public static void SetBool(bool value, int address, byte[] target)
		{
			target[address] = value ? (byte)1 : (byte)0;
		}

		public static byte GetByte(int address, byte[] target)
		{
			return target[address];
		}
		public static void SetByte(byte value, int address, byte[] target)
		{
			target[address] = value;
		}

		public static short GetShort(int address, byte[] target)
		{
			return BitConverter.ToInt16(target, address);
		}
		public static void SetShort(short value, int address, byte[] target)
		{
			target[address++] = (byte)value;
			target[address] = (byte)(value >> 8);
		}

		public static ushort GetUShort(int address, byte[] target)
		{
			return BitConverter.ToUInt16(target, address);
		}
		public static void SetUShort(ushort value, int address, byte[] target)
		{
			target[address++] = (byte)value;
			target[address] = (byte)(value >> 8);
		}

		public static int GetInt(int address, byte[] target)
		{
			return BitConverter.ToInt32(target, address);
		}
		public static void SetInt(int value, int address, byte[] target)
		{
			target[address++] = (byte)value;
			target[address++] = (byte)(value >> 8);
			target[address++] = (byte)(value >> 16);
			target[address] = (byte)(value >> 24);
		}

		public static uint GetUInt(int address, byte[] target)
		{
			return BitConverter.ToUInt32(target, address);
		}
		public static void SetUInt(uint value, int address, byte[] target)
		{
			target[address++] = (byte)value;
			target[address++] = (byte)(value >> 8);
			target[address++] = (byte)(value >> 16);
			target[address] = (byte)(value >> 24);
		}

		public static float GetFloat(int address, byte[] target)
		{
			return BitConverter.ToSingle(target, address);
		}
		public static void SetFloat(float value, int address, byte[] target)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			target[address++] = bytes[0];
			target[address++] = bytes[1];
			target[address++] = bytes[2];
			target[address] = bytes[3];
		}

		public static string GetString(int address, byte[] target)
		{
			int idx = 0;
			if (target[address] != 0)
			{
				while (address + idx < target.Length)
				{
					if (target[address + ++idx] == 0)
					{
						break;
					}
				}
				return Encoding.UTF8.GetString(target, address, idx);
			}
			else
			{
				return string.Empty;
			}
		}
		public static void SetString(string value, int address, byte[] target)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			for (int idx = 0; idx < bytes.Length; ++idx)
			{
				target[address++] = bytes[idx];
			}
		}
	}
}
