using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Files;
using SAGE;

namespace SAGE.Compiler
{
	public class AptConstData : CompileHandler
	{
		public override bool Compile(GameAssetType gameAsset, Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game,
			string trace, ref int position, out string ErrorDescription)
		{
			BinaryAsset blob = new BinaryAsset(0);
			string file = null;
			foreach (XmlAttribute attribute in node.Attributes)
			{
				switch (attribute.Name)
				{
					case "File":
						file = attribute.Value;
						break;
				}
			}
			if (file == null)
			{
				ErrorDescription = string.Format("No file set for {0}", trace);
				return false;
			}
			Uri fileUri = Macro.Parse(file);
			if (!fileUri.IsAbsoluteUri)
			{
				fileUri = new Uri(baseUri, fileUri);
			}
			file = fileUri.LocalPath;
			if (!File.Exists(file))
			{
				ErrorDescription = string.Format("File not found for {0}", trace);
				return false;
			}
			using (FileStream blobStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader blobReader = new BinaryReader(blobStream))
				{
					blob.Content = blobReader.ReadBytes((int)(blobStream.Length));
				}
			}
			asset.SubAssets.Add(0x04, blob);
			FileHelper.SetInt(blob.Content.Length, 0x08, asset.Content);
			ErrorDescription = string.Empty;
			return true;
		}
	}
}
