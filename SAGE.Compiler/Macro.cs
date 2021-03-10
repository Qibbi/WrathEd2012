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
		}

		public static Uri Parse(string source)
		{
			Uri baseUri = new Uri(Root);
			string testString;
			Uri result;
			if (source.StartsWith("ART:", StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (string macro in Art)
				{
					testString = macro + Path.DirectorySeparatorChar + source.Substring(4);
					if (File.Exists((result = new Uri(baseUri, testString)).LocalPath))
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
					if (File.Exists((result = new Uri(baseUri, testString)).LocalPath))
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
					if (File.Exists((result = new Uri(baseUri, testString)).LocalPath))
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
