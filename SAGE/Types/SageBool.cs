using System;
using System.Globalization;

namespace SAGE.Types
{
	public class SageBool : SageBase
	{
		public const string DefaultValue = "False";
		public bool Value
		{
			get
			{
				if (value.Contains("0"))
				{
					return false;
				}
				else if (value.Contains("1"))
				{
					return true;
				}
				return bool.Parse(value);
			}
		}

		public SageBool(string source)
		{
			RegEx = Constants.RegExBool;
			setValue(source);
		}

		protected override string parseDefine(string source)
		{
			throw new NotImplementedException();
		}
	}
}
