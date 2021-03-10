using System;
using System.Globalization;

namespace SAGE.Types
{
	public class Percentage : SageBase
	{
		public const string DefaultValue = "0%";

		public float Value
		{
			get
			{
				if (value.EndsWith("%", StringComparison.InvariantCulture))
				{
					return float.Parse(value.Substring(0, value.Length - 1), NumberFormatInfo.InvariantInfo) * 0.01f;
				}
				return float.Parse(value, NumberFormatInfo.InvariantInfo) * 0.01f;
			}
		}

		public Percentage(string source)
		{
			RegEx = Constants.RegExPercentage;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
