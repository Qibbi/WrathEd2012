using System;
using System.Text.RegularExpressions;

namespace SAGE.Types
{
	public abstract class SageBase
	{
		protected const string ExceptionText = "The value \"{0}\" isn't valid for it's type \"{1}\".";

		protected string value;
		protected Regex RegEx;

		protected void setValue(string source)
		{
			if (isValid(source))
			{
				if (!isDefine(source))
				{
					value = source;
				}
				else
				{
					value = parseDefine(source);
				}
			}
			else
			{
				throw new ArgumentException(string.Format(ExceptionText, source, GetType().Name));
			}
		}

		protected bool isValid(string source)
		{
			if (source[0] == 0x3d || (RegEx == null || RegEx.IsMatch(source)))
			{
				return true;
			}
			return false;
		}

		protected bool isDefine(string source)
		{
			if (source[0] == 0x3d)
			{
				return true;
			}
			return false;
		}

		protected abstract string parseDefine(string source);

		public override string ToString()
		{
			return value;
		}
	}
}
