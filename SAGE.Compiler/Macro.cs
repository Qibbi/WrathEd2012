using System;
using System.Collections.Generic;
using System.IO;

namespace SAGE.Compiler
{
	public static class Macro
	{
		public static List<string> Art = new List<string>();
		public static List<string> Audio = new List<string>();
		public static List<string> Data = new List<string>();
		public static string Root;
		public static string Terrain;
		public static string PostFix;

		static Macro()
		{
			Root = null;
			Terrain = null;
			Art.Add(@"..\Art");
			Art.Add(@".\Art");
			Audio.Add(@"..\Audio");
			Audio.Add(@".\Audio");
			Data.Add(@".");
			Data.Add(@".\Mods");
			Data.Add(@".\CnC3Xml");
			Data.Add(@".\SageXml");
			PostFix = null;
		}

		public static Uri Parse(string source)
		{
			Uri baseUri = new Uri(Root);
			string testString;
			string testStringPostFix;
			Uri result;
			if (source.StartsWith("ART:", StringComparison.InvariantCultureIgnoreCase))
			{
				string subfolder = source.Substring(4, 2);
				string testString_nofolder;
				string testStringPostFix_nofolder;
				foreach (string macro in Art)
				{
					testString = macro + Path.DirectorySeparatorChar + subfolder + Path.DirectorySeparatorChar + source.Substring(4);
					testString_nofolder = macro + Path.DirectorySeparatorChar + source.Substring(4);

					testStringPostFix = testString.Substring(0, testString.Length - 4) + "_" + PostFix + testString.Substring(testString.Length - 4);
					testStringPostFix_nofolder = testString_nofolder.Substring(0, testString_nofolder.Length - 4) + "_" + PostFix + testString_nofolder.Substring(testString_nofolder.Length - 4);

					if (File.Exists((result = new Uri(baseUri, testStringPostFix)).LocalPath))
					{
						return result;
					}
					else if (File.Exists((result = new Uri(baseUri, testStringPostFix_nofolder)).LocalPath))
					{
						return result;
					}
					else if (File.Exists((result = new Uri(baseUri, testString)).LocalPath))
					{
						return result;
					}
					else if (File.Exists((result = new Uri(baseUri, testString_nofolder)).LocalPath))
					{
						return result;
					}
				}
				source = Art[0] + Path.DirectorySeparatorChar + source.Substring(4);
				return new Uri(baseUri, source);
			}
			else if (source.StartsWith("AUDIO:", StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (string macro in Audio)
				{
					testString = macro + Path.DirectorySeparatorChar + source.Substring(6);
					testStringPostFix = testString.Substring(0, testString.Length - 4) + "_" + PostFix + testString.Substring(testString.Length - 4);
					if (File.Exists((result = new Uri(baseUri, testStringPostFix)).LocalPath))
					{
						return result;
					}
					else if (File.Exists((result = new Uri(baseUri, testString)).LocalPath))
					{
						return result;
					}
				}
				source = Audio[0] + Path.DirectorySeparatorChar + source.Substring(6);
				return new Uri(baseUri, source);
			}
			else if (source.StartsWith("DATA:", StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (string macro in Data)
				{
					testString = macro + Path.DirectorySeparatorChar + source.Substring(5);
					testStringPostFix = testString.Substring(0, testString.Length - 4) + "_" + PostFix + testString.Substring(testString.Length - 4);
					if (File.Exists((result = new Uri(baseUri, testStringPostFix)).LocalPath))
					{
						return result;
					}
					else if (File.Exists((result = new Uri(baseUri, testString)).LocalPath))
					{
						return result;
					}
				}
				source = Data[0] + Path.DirectorySeparatorChar + source.Substring(5);
				return new Uri(baseUri, source);
			}
			return new Uri(source, UriKind.RelativeOrAbsolute);
		}
	}
}
