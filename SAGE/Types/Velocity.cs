using System;
using System.Globalization;

namespace SAGE.Types
{
	public class Velocity : SageBase
	{
		public const string DefaultValue = "0";

		public float Value
		{
			get
			{
				return float.Parse(value, NumberFormatInfo.InvariantInfo) * Constants.SECONDS_PER_LOGICFRAME_REAL;
			}
		}

		public Velocity(string source)
		{
			RegEx = Constants.RegExReal;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
