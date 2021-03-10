using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Files;

namespace SAGE.CSF
{
	public enum GameVersion
	{
		CNC3_CNC3KW,
		UNUSED_00,
		UNUSED_01,
		CNC4
	}

	public enum Language
	{
		ENGLISH_US,
		ENGLISH_UK,
		GERMAN,
		FRENCH,
		SPANISH,
		ITALIAN,
		JAPANESE,
		UNUSED_00,
		KOREAN,
		CHINESE
	}

	public class CSF
	{
		private const uint iCSF = 0x43534620u; // "CSF "
		private const uint iLBL = 0x4C424C20u; // "LBL "
		private const uint iSTR = 0x53545220u; // "STR "

		private byte[] header;
		private byte[] data;

		private Dictionary<string, string> loadedStrings;

		public uint FileType
		{
			get { return FileHelper.GetUInt(0x00, header); }
			set { FileHelper.SetUInt(value, 0x00, header); }
		}

		public int Version
		{
			get { return FileHelper.GetInt(0x04, header); }
			set { FileHelper.SetInt(value, 0x04, header); }
		}

		public uint LabelsCount
		{
			get { return FileHelper.GetUInt(0x08, header); }
			set { FileHelper.SetUInt(value, 0x08, header); }
		}

		public uint LabelsCount2
		{
			get { return FileHelper.GetUInt(0x0C, header); }
			set { FileHelper.SetUInt(value, 0x0C, header); }
		}

		public uint Null
		{
			get { return FileHelper.GetUInt(0x10, header); }
			set { FileHelper.SetUInt(value, 0x10, header); }
		}

		public Language Language
		{
			get { return (Language)FileHelper.GetUInt(0x14, header); }
			set { FileHelper.SetUInt((uint)value, 0x14, header); }
		}

		public CSF(string path)
		{
			loadedStrings = new Dictionary<string, string>();
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("[String File] The file could not be found!");
			}
			using (FileStream stream = File.OpenRead(path))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					header = reader.ReadBytes(0x18);
					if (FileType != iCSF)
					{
						throw new NotSupportedException("[String File] The file is not supported!");
					}
					data = reader.ReadBytes((int)stream.Length - 0x18);
				}
			}
			int position = 0;
			for (int idx = 0; idx < LabelsCount; ++idx)
			{
				uint check = FileHelper.GetUInt(position, data);
				position += 4;
				if (check != iLBL)
				{
					throw new NotSupportedException("[String File] There was an error parsing the label entry @ position " + position + "!");
				}
				int substrings = FileHelper.GetInt(position, data);
				position += 4;
				if (substrings != 1)
				{
					throw new NotSupportedException("[String File] There was != 1 substrings @ position " + position + "!");
				}
				int labelLength = FileHelper.GetInt(position, data);
				position += 4;
				string label = FileHelper.GetString(position, data, labelLength);
				position += labelLength;
				check = FileHelper.GetUInt(position, data);
				position += 4;
				if (check != iSTR)
				{
					throw new NotSupportedException("[String File] There was an error parsing the string entry @ position " + position + "!");
				}
				int valueLength = FileHelper.GetInt(position, data) << 1;
				position += 4;
				byte[] valueArray = new byte[valueLength];
				for (int idy = 0; idy < valueLength; ++idy)
				{
					valueArray[idy] = (byte)~data[position++];
				}
				string value = FileHelper.GetWideString(0x00, valueArray);
				if (label.Contains(':'))
				{
					string[] labelCategory = label.Split(':');
					labelCategory[0] = labelCategory[0].ToUpperInvariant();
					label = labelCategory[0] + ':' + labelCategory[1];
				}
				if (!loadedStrings.ContainsKey(label))
				{
					loadedStrings.Add(label, value);
				}
			}
		}

		public string GetLocalizedString(string identifier)
		{
			if (loadedStrings.ContainsKey(identifier))
			{
				return loadedStrings[identifier];
			}
			return string.Format("MISSING: \"{0}\"", identifier);
		}

		public string GetLocalizedString(string identifier, out bool foundString)
		{
			if (loadedStrings.ContainsKey(identifier))
			{
				foundString = true;
				return loadedStrings[identifier];
			}
			foundString = false;
			return string.Format("MISSING: \"{0}\"", identifier);
		}
	}
}
