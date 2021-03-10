using System;
using System.Globalization;

namespace SAGE.Types
{
	public class SageInt : SageBase
	{
		public const string DefaultValue = "0";

		public int Value
		{
			get
			{
				if (value.StartsWith("0x", StringComparison.InvariantCulture))
				{
					return int.Parse(value.Substring(2), NumberStyles.HexNumber);
				}
				return int.Parse(value);
			}
		}

		public SageInt(string source)
		{
			RegEx = Constants.RegExInt;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
