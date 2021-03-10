using System;
using System.Globalization;

namespace SAGE.Types
{
	public class SageReal : SageBase
	{
		public const string DefaultValue = "0";

		public float Value
		{
			get
			{
				float result = 0;
				try
				{
					result = float.Parse(value, NumberFormatInfo.InvariantInfo);
				}
				catch
				{
					result = float.PositiveInfinity;
				}
				return result;
			}
		}

		public SageReal(string source)
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
