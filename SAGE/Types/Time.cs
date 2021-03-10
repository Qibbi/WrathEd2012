using System;
using System.Globalization;

namespace SAGE.Types
{
	public class Time : SageBase
	{
		public const string DefaultValue = "0s";

		public float Value
		{
			get
			{
				if ((value.EndsWith("ms", StringComparison.InvariantCulture)))
				{
					return float.Parse(value.Substring(0, value.Length - 2), NumberFormatInfo.InvariantInfo) * 0.001f;
				}
				return float.Parse(value.Substring(0, value.Length - 1), NumberFormatInfo.InvariantInfo);
			}
		}

		public Time(string source)
		{
			RegEx = Constants.RegExTime;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
