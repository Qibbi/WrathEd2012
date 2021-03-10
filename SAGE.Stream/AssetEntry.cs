using System;
using Files;

namespace SAGE.Stream
{
	public abstract class AssetEntry
	{
		protected byte[] header;
		public abstract uint TypeId { get; set; }
		public abstract uint InstanceId { get; set; }
		public abstract uint TypeHash { get; set; }
		public abstract uint InstanceHash { get; set; }
		public abstract uint AssetReferenceOffset { get; set; }
		public abstract uint AssetReferenceCount { get; set; }
		public abstract uint NameOffset { get; set; }
		public abstract uint SourceFileNameOffset { get; set; }
		public abstract uint BinaryDataSize { get; set; }
		public abstract uint RelocationsDataSize { get; set; }
		public abstract uint ImportsDataSize { get; set; }
		public abstract bool IsTokenized { get; set; }

		public AssetEntry()
		{
		}

		public AssetEntry(byte[] source)
		{
			header = source;
		}

		public AssetEntry(AssetEntry source)
		{
		}

		public void Write(System.IO.BinaryWriter writer)
		{
			writer.Write(header);
		}
	}

	public class AssetEntry5 : AssetEntry
	{
		public override uint TypeId
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

		public override uint InstanceId
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

		public override uint TypeHash
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

		public override uint InstanceHash
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

		public override uint AssetReferenceOffset
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

		public override uint AssetReferenceCount
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

		public override uint NameOffset
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

		public override uint SourceFileNameOffset
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

		public override uint BinaryDataSize
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

		public override uint RelocationsDataSize
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

		public override uint ImportsDataSize
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

		public override bool IsTokenized
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public AssetEntry5()
		{
			header = new byte[0x2C];
		}

		public AssetEntry5(byte[] source)
			: base(source)
		{
		}

		public AssetEntry5(AssetEntry source)
			: base(source)
		{
			header = new byte[0x2C];
			TypeId = source.TypeId;
			InstanceId = source.InstanceId;
			TypeHash = source.TypeHash;
			InstanceHash = source.InstanceHash;
			AssetReferenceOffset = source.AssetReferenceCount;
			AssetReferenceCount = source.AssetReferenceCount;
			NameOffset = source.NameOffset;
			SourceFileNameOffset = source.SourceFileNameOffset;
			BinaryDataSize = source.BinaryDataSize;
			RelocationsDataSize = source.RelocationsDataSize;
			ImportsDataSize = source.ImportsDataSize;
		}
	}

	public class AssetEntry6 : AssetEntry
	{
		public override uint TypeId
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

		public override uint InstanceId
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

		public override uint TypeHash
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

		public override uint InstanceHash
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

		public override uint AssetReferenceOffset
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

		public override uint AssetReferenceCount
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

		public override uint NameOffset
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

		public override uint SourceFileNameOffset
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

		public override uint BinaryDataSize
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

		public override uint RelocationsDataSize
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

		public override uint ImportsDataSize
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

		public override bool IsTokenized
		{
			get
			{
				return FileHelper.GetBool(0x2C, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x2C, header);
			}
		}

		public AssetEntry6()
		{
			header = new byte[0x30];
		}

		public AssetEntry6(byte[] source)
			: base(source)
		{
		}

		public AssetEntry6(AssetEntry source)
			: base(source)
		{
			header = new byte[0x30];
			TypeId = source.TypeId;
			InstanceId = source.InstanceId;
			TypeHash = source.TypeHash;
			InstanceHash = source.InstanceHash;
			AssetReferenceOffset = source.AssetReferenceCount;
			AssetReferenceCount = source.AssetReferenceCount;
			NameOffset = source.NameOffset;
			SourceFileNameOffset = source.SourceFileNameOffset;
			BinaryDataSize = source.BinaryDataSize;
			RelocationsDataSize = source.RelocationsDataSize;
			ImportsDataSize = source.ImportsDataSize;
			IsTokenized = source.IsTokenized;
		}
	}

	public class AssetEntry7 : AssetEntry
	{
		public override uint TypeId
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

		public override uint InstanceId
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

		public override uint TypeHash
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

		public override uint InstanceHash
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

		public override uint AssetReferenceOffset
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

		public override uint AssetReferenceCount
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

		public override uint NameOffset
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

		public override uint SourceFileNameOffset
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

		public override uint BinaryDataSize
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

		public override uint RelocationsDataSize
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

		public override uint ImportsDataSize
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

		public override bool IsTokenized
		{
			get
			{
				return FileHelper.GetBool(0x2C, header);
			}
			set
			{
				FileHelper.SetBool(value, 0x2C, header);
			}
		}

		public AssetEntry7()
		{
			header = new byte[0x30];
		}

		public AssetEntry7(byte[] source)
			: base(source)
		{
		}

		public AssetEntry7(AssetEntry source)
			: base(source)
		{
			header = new byte[0x30];
			TypeId = source.TypeId;
			InstanceId = source.InstanceId;
			TypeHash = source.TypeHash;
			InstanceHash = source.InstanceHash;
			AssetReferenceOffset = source.AssetReferenceCount;
			AssetReferenceCount = source.AssetReferenceCount;
			NameOffset = source.NameOffset;
			SourceFileNameOffset = source.SourceFileNameOffset;
			BinaryDataSize = source.BinaryDataSize;
			RelocationsDataSize = source.RelocationsDataSize;
			ImportsDataSize = source.ImportsDataSize;
			IsTokenized = source.IsTokenized;
		}
	}
}
