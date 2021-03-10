using System;
using Files;

namespace SAGE.Stream
{
	public enum StreamReferenceType
	{
		UNDEFINED,
		REFERENCE,
		PATCH
	}
	public class StreamReference
	{
		protected byte[] header;

		public StreamReferenceType ReferenceType
		{
			get
			{
				return (StreamReferenceType)FileHelper.GetByte(0x00, header);
			}
			set
			{
				FileHelper.SetByte((byte)value, 0x00, header);
			}
		}

		public string Path
		{
			get
			{
				return FileHelper.GetString(0x01, header, header.Length - 1);
			}
			set
			{
				FileHelper.SetString(value, 0x01, header);
			}
		}

		public StreamReference(StreamReferenceType type, string path)
		{
			header = new byte[path.Length + 2];
			ReferenceType = type;
			Path = path;
		}

		public StreamReference(byte[] source)
		{
			header = source;
		}

		public void Write(System.IO.BinaryWriter writer)
		{
			writer.Write(header);
		}
	}
}