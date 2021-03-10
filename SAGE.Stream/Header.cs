using System;
using SAGE;
using Files;

namespace SAGE.Stream
{
	public abstract class Header
	{
		protected byte[] header;
		public abstract bool IsBigEndian { get; set; }
		public abstract bool IsLinked { get; set; }
		public abstract ushort Version { get; set; }
		public abstract uint Checksum { get; set; }
		public abstract uint AllTypesHash { get; set; }
		public abstract uint AssetCount { get; set; }
		public abstract uint BinaryDataSize { get; set; }
		public abstract uint MaxBinaryChunkSize { get; set; }
		public abstract uint MaxRelocationsChunkSize { get; set; }
		public abstract uint MaxImportsChunkSize { get; set; }
		public abstract uint AssetReferenceBufferSize { get; set; }
		public abstract uint ReferencedManifestNameBufferSize { get; set; }
		public abstract uint AssetNameBufferSize { get; set; }
		public abstract uint SourceFileNameBufferSize { get; set; }

		public Header()
		{
		}

		public Header(byte[] source)
		{
			header = source;
		}

		public void Write(System.IO.BinaryWriter writer)
		{
			writer.Write(header);
		}
	}

	public class Header5 : Header
	{
		public override bool IsBigEndian
		{
			get
			{
				return FileHelper.GetBool(0x00, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x00, header);
			}
		}

		public override bool IsLinked
		{
			get
			{
				return FileHelper.GetBool(0x01, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x01, header);
			}
		}

		public override ushort Version
		{
			get
			{
				return FileHelper.GetUShort(0x02, header);
			}
			set
			{
				FileHelper.SetUShort(value, 0x02, header);
			}
		}

		public override uint Checksum
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

		public override uint AllTypesHash
		{
			get
			{
				return FileHelper.GetUInt(0x08, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x08, header);
			}
		}

		public override uint AssetCount
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

		public override uint BinaryDataSize
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

		public override uint MaxBinaryChunkSize
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

		public override uint MaxRelocationsChunkSize
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

		public override uint MaxImportsChunkSize
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

		public override uint AssetReferenceBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x20, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x20, header);
			}
		}

		public override uint ReferencedManifestNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x24, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x24, header);
			}
		}

		public override uint AssetNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x28, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x28, header);
			}
		}

		public override uint SourceFileNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x2C, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x2C, header);
			}
		}

		public Header5()
		{
			header = new byte[0x30];
			Version = 5;
		}

		public Header5(byte[] source)
			: base(source)
		{
		}
	}

	public class Header6 : Header
	{
		public override bool IsBigEndian
		{
			get
			{
				return FileHelper.GetBool(0x00, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x00, header);
			}
		}

		public override bool IsLinked
		{
			get
			{
				return FileHelper.GetBool(0x01, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x01, header);
			}
		}

		public override ushort Version
		{
			get
			{
				return FileHelper.GetUShort(0x02, header);
			}
			set
			{
				FileHelper.SetUShort(value, 0x02, header);
			}
		}

		public override uint Checksum
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

		public override uint AllTypesHash
		{
			get
			{
				return FileHelper.GetUInt(0x08, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x08, header);
			}
		}

		public override uint AssetCount
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

		public override uint BinaryDataSize
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

		public override uint MaxBinaryChunkSize
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

		public override uint MaxRelocationsChunkSize
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

		public override uint MaxImportsChunkSize
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

		public override uint AssetReferenceBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x20, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x20, header);
			}
		}

		public override uint ReferencedManifestNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x24, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x24, header);
			}
		}

		public override uint AssetNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x28, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x28, header);
			}
		}

		public override uint SourceFileNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x2C, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x2C, header);
			}
		}

		public Header6()
		{
			header = new byte[0x30];
			Version = 6;
		}

		public Header6(byte[] source)
			: base(source)
		{
		}
	}

	public class Header7 : Header
	{
		public override bool IsBigEndian
		{
			get
			{
				return FileHelper.GetBool(0x06, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x06, header);
			}
		}

		public override bool IsLinked
		{
			get
			{
				return FileHelper.GetBool(0x07, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x07, header);
			}
		}

		public override ushort Version
		{
			get
			{
				return FileHelper.GetUShort(0x04, header);
			}
			set
			{
				FileHelper.SetUShort(value, 0x04, header);
			}
		}

		public override uint Checksum
		{
			get
			{
				return FileHelper.GetUInt(0x08, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x08, header);
			}
		}

		public override uint AllTypesHash
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

		public override uint AssetCount
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

		public override uint BinaryDataSize
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

		public override uint MaxBinaryChunkSize
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

		public override uint MaxRelocationsChunkSize
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

		public override uint MaxImportsChunkSize
		{
			get
			{
				return FileHelper.GetUInt(0x20, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x20, header);
			}
		}

		public override uint AssetReferenceBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x24, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x24, header);
			}
		}

		public override uint ReferencedManifestNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x28, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x28, header);
			}
		}

		public override uint AssetNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x2C, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x2C, header);
			}
		}

		public override uint SourceFileNameBufferSize
		{
			get
			{
				return FileHelper.GetUInt(0x30, header);
			}
			set
			{
				FileHelper.SetUInt(value, 0x30, header);
			}
		}

		public Header7()
		{
			header = new byte[0x34];
			Version = 7;
		}

		public Header7(byte[] source)
			: base(source)
		{
		}
	}
}
