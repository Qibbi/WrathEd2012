using System;
using System.Text.RegularExpressions;

namespace SAGE.Types
{
	public static class Constants
	{
		public const float PI = 3.14159265359f;
		public const float RADS_PER_DEGREE = PI / 180.0f;
		public const float DEGREES_PER_RAD = 1.0f / RADS_PER_DEGREE;
		public const float LOGICFRAMES_PER_SECOND = 5;
		public const float MSEC_PER_SECOND = 1000;
		public const float LOGICFRAMES_PER_MSEC_REAL = (LOGICFRAMES_PER_SECOND / MSEC_PER_SECOND);
		public const float LOGICFRAMES_PER_SECONDS_REAL = LOGICFRAMES_PER_SECOND;
		public const float SECONDS_PER_LOGICFRAME_REAL = 1.0f / LOGICFRAMES_PER_SECONDS_REAL;

		public static Regex RegExDefine = new Regex(@"=\$[_a-zA-Z]+$");
		public static Regex RegExAngle = new Regex(@"^(\+|\-)?((\.\d{1,16})|(\d{1,16}(\.\d{0,16})?))([eE](\+|\-)?\d{1,3})?(r|d)$");
		public static Regex RegExPercentage = new Regex(@"^(\+|\-)?((\.\d{1,16})|(\d{1,16}(\.\d{0,16})?))([%])?$");
		public static Regex RegExBool = new Regex(@"^([fF]alse)|[tT]rue|0|1$");
		public static Regex RegExInt = new Regex(@"^((\+|\-)?\d{1,10})|((0x)[a-fA-F0-9]{8,8})$");
		public static Regex RegExReal = new Regex(@"^(\+|\-)?((\.\d{1,16})|(\d{1,16}(\.\d{0,16})?))([eE](\+|\-)?\d{1,3})?$");
		public static Regex RegExUnsignedInt = new Regex(@"^(\d{1,10})|(0x[a-fA-F0-9]{1,8})$");
		public static Regex RegExUnsignedShort = new Regex(@"^\d{1,5}$");
		public static Regex RegExTime = new Regex(@"^(\+|\-)?((\.\d{1,16})|(\d{1,16}(\.\d{0,16})?))([eE](\+|\-)?\d{1,3})?(s|ms)$");
	}
}
