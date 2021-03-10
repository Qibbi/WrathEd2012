using System;
using System.Globalization;

namespace SAGE.Types
{
	public class SageUnsignedShort : SageBase
	{
		public const string DefaultValue = "0";

		public ushort Value
		{
			get
			{
				return ushort.Parse(value);
			}
		}

		public SageUnsignedShort(string source)
		{
			RegEx = Constants.RegExUnsignedShort;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
