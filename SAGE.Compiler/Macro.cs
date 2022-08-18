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
					testString = Path.Combine(macro, subfolder, source.Substring(4));
					testString_nofolder = Path.Combine(macro, source.Substring(4));
					testStringPostFix = Path.Combine(Path.GetDirectoryName(testString), $"{Path.GetFileNameWithoutExtension(testString)}_{PostFix}{Path.GetExtension(testString)}");
					testStringPostFix_nofolder = Path.Combine(Path.GetDirectoryName(testString_nofolder), $"{Path.GetFileNameWithoutExtension(testString_nofolder)}_{PostFix}{Path.GetExtension(testString_nofolder)}");

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
					testString = Path.Combine(macro, source.Substring(6));
					testStringPostFix = Path.Combine(Path.GetDirectoryName(testString), $"{Path.GetFileNameWithoutExtension(testString)}_{PostFix}{Path.GetExtension(testString)}");

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
					testString = Path.Combine(macro, source.Substring(5));
					testStringPostFix = Path.Combine(Path.GetDirectoryName(testString), $"{Path.GetFileNameWithoutExtension(testString)}_{PostFix}{Path.GetExtension(testString)}");

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
