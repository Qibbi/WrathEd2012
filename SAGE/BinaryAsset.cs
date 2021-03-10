using System;
using System.Collections.Generic;
using Files;

namespace SAGE
{
	public class BinaryAsset
	{
		public byte[] Content;
		public Dictionary<int, BinaryAsset> SubAssets;
		public Dictionary<int, uint[]> AssetImports;
		public BinaryAsset CData;
		public bool IsWritingCDataHead;

		public BinaryAsset(int size)
		{
			Content = new byte[size];
			SubAssets = new Dictionary<int, BinaryAsset>();
			AssetImports = new Dictionary<int, uint[]>();
			IsWritingCDataHead = true;
		}

		public void GetBinary(short version, List<byte[]> binary, List<int> imports, List<uint[]> importAssets, List<int> relocations, ref int position, ref int length)
		{
			binary.Add(Content);
			foreach (KeyValuePair<int, uint[]> assetImport in AssetImports)
			{
				FileHelper.SetInt(((version == 5) ? imports.Count : imports.Count + 1), assetImport.Key, Content);
				imports.Add(position + assetImport.Key);
				importAssets.Add(assetImport.Value);
			}
			foreach (int subAsset in SubAssets.Keys)
			{
				if (subAsset != -1)
				{
					relocations.Add(position + subAsset);
				}
			}
			position += Content.Length;
			foreach (KeyValuePair<int, BinaryAsset> subAsset in SubAssets)
			{
				if (subAsset.Key != -1)
				{
					FileHelper.SetInt(length, subAsset.Key, Content);
				}
				length += subAsset.Value.Content.Length;
				subAsset.Value.GetBinary(version, binary, imports, importAssets, relocations, ref position, ref length);
			}
		}

		public void GatherReferences(List<uint[]> references)
		{
			foreach (uint[] reference in AssetImports.Values)
			{
				references.Add(reference);
			}
			foreach (BinaryAsset subAsset in SubAssets.Values)
			{
				subAsset.GatherReferences(references);
			}
		}
	}
}
