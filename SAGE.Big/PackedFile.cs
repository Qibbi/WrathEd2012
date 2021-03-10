using System;

namespace SAGE.Big
{
	public class PackedFile
	{
		public uint Offset;
		public uint Size;
		public string Name;

		public PackedFile()
		{
			Offset = 0;
			Size = 0;
			Name = string.Empty;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
