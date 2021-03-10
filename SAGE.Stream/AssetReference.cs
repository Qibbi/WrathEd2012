using System;
using Files;

namespace SAGE.Stream
{
	public class AssetReference
	{
		protected byte[] header;

		public uint TypeId
		{
			get
			{
				return FileHelper.GetUInt(0x00, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x00, header);
			}
		}

		public uint InstanceId
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

		public AssetReference(byte[] source)
		{
			header = source;
		}

		public void Write(System.IO.BinaryWriter writer)
		{
			writer.Write(header);
		}
	}
}