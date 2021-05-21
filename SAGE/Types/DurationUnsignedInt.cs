using System;
using System.Globalization;

namespace SAGE.Types
{
	public class DurationUnsignedInt : SageBase
	{
		public const string DefaultValue = "0";

		public uint Value
		{
			get
			{
				if (value.StartsWith("0x", StringComparison.InvariantCulture))
				{
					return (uint)Math.Ceiling(uint.Parse(value.Substring(2), NumberStyles.HexNumber) * Constants.LOGICFRAMES_PER_MSEC_REAL);
				}
				return (uint)Math.Ceiling(uint.Parse(value) * Constants.LOGICFRAMES_PER_MSEC_REAL);
			}
		}

		public DurationUnsignedInt(string source)
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
