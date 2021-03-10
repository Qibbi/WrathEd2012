using System;
using System.Collections.Generic;

namespace SAGE.Compiler
{
	public static class Defines
	{
		public static Dictionary<string, string> Define;

		static Defines()
		{
			Define = new Dictionary<string, string>();
		}
	}
}
