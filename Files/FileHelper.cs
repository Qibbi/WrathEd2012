using System;
using System.Text;

namespace Files
{
    public static class FileHelper
    {
        public enum Endian
        {
            LITTLE,
            BIG
        }

        public static Endian Endianness = Endian.LITTLE;

        public static short Invert(short invInt)
        {
            short result = (short)(invInt << 8);
            invInt = (short)(invInt >> 8);
            result |= (short)(invInt & 0xff);
            return result;
        }
        public static ushort Invert(ushort invInt)
        {
            ushort result = (ushort)(invInt << 8);
            invInt = (ushort)(invInt >> 8);
            result |= (ushort)(invInt & 0xff);
            return result;
        }
        public static int Invert(int invInt)
        {
            int result = (invInt & 0xff) << 24;
            invInt = invInt >> 8;
            result |= (invInt & 0xff) << 16;
            invInt = invInt >> 8;
            result |= (invInt & 0xff) << 8;
            invInt = (invInt >> 8);
            result |= invInt & 0xff;
            return result;
        }
        public static uint Invert(uint invInt)
        {
            uint result = (invInt & 0xff) << 24;
            invInt = invInt >> 8;
            result |= (invInt & 0xff) << 16;
            invInt = invInt >> 8;
            result |= (invInt & 0xff) << 8;
            invInt = invInt >> 8;
            result |= invInt & 0xff;
            return result;
        }

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

        public static long GetInt64(int address, byte[] target)
        {
            return BitConverter.ToInt64(target, address);
        }
        public static void SetInt64(uint value, int address, byte[] target)
        {
            target[address++] = (byte)value;
            target[address++] = (byte)(value >> 8);
            target[address++] = (byte)(value >> 16);
            target[address++] = (byte)(value >> 24);
            target[address++] = (byte)(value >> 32);
            target[address++] = (byte)(value >> 40);
            target[address++] = (byte)(value >> 48);
            target[address] = (byte)(value >> 56);
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

        public static unsafe string GetString(byte* str, int length)
        {
            return new string((sbyte*)str, 0, length, Encoding.UTF8);
        }

        public static string GetString(int address, byte[] target)
        {
            int idx = 0;
            if (target.Length != 0 && target[address] != 0)
            {
                while (address + idx < target.Length)
                {
                    if (target[address + idx] == 0)
                    {
                        break;
                    }
                    ++idx;
                }
                return Encoding.UTF8.GetString(target, address, idx);
            }
            else
            {
                return string.Empty;
            }
        }
        public static string GetString(int address, byte[] target, int length)
        {
            return Encoding.UTF8.GetString(target, address, length);
        }
        public static void SetString(string value, int address, byte[] target)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Array.Copy(bytes, 0, target, address, bytes.Length);
        }

        public static string GetWideString(int address, byte[] target)
        {
            int idx = 0;
            if (target.Length != 0 && (target[address] != 0 || target[address + 1] != 0))
            {
                while (address + idx < target.Length)
                {
                    if (target[address + idx] == 0 && target[address + idx + 1] == 0)
                    {
                        break;
                    }
                    idx += 2;
                }
                return Encoding.Unicode.GetString(target, address, idx);
            }
            else
            {
                return string.Empty;
            }
        }
        public static string GetWideString(int address, byte[] target, int length)
        {
            return Encoding.Unicode.GetString(target, address, length << 1);
        }
        public static void SetWideString(string value, int address, byte[] target)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            Array.Copy(bytes, 0, target, address, bytes.Length);
        }
    }
}
