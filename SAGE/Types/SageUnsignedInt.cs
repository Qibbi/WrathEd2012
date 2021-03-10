using System;
using System.Globalization;

namespace SAGE.Types
{
	public class SageUnsignedInt : SageBase
	{
		public const string DefaultValue = "0";

		public uint Value
		{
			get
			{
				if (value.StartsWith("0x", StringComparison.InvariantCulture))
				{
					return uint.Parse(value.Substring(2), NumberStyles.HexNumber);
				}
				return uint.Parse(value);
			}
		}

		public SageUnsignedInt(string source)
		{
			RegEx = Constants.RegExUnsignedInt;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
