using System;
using System.Globalization;

namespace SAGE.Types
{
	public class Angle : SageBase
	{
		public const string DefaultValue = "0r";

		public float Value
		{
			get
			{
				if ((value.EndsWith("r", StringComparison.InvariantCulture)))
				{
					return float.Parse(value.Substring(0, value.Length - 1), NumberFormatInfo.InvariantInfo);
				}
				return float.Parse(value.Substring(0, value.Length - 1), NumberFormatInfo.InvariantInfo) * Constants.RADS_PER_DEGREE;
			}
		}

		public Angle(string source)
		{
			RegEx = Constants.RegExAngle;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
