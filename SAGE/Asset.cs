using System;
using System.Collections.Generic;

namespace SAGE
{
	public class Asset
	{
		public uint TypeId;
		public uint InstanceId;
		public uint TypeHash;
		public uint InstanceHash;

		public List<uint[]> AssetReferences;
		public List<uint[]> WeakAssetReferences;

		public string Type;
		public string Name;
		public string SourceFile;

		public BinaryAsset Binary;

		public bool IsNew;

		public Asset(string type, uint typeHash, string instance, uint instanceHash, string source)
		{
			TypeId = StringHasher.Hash(type);
			InstanceId = StringHasher.Hash(instance.ToLowerInvariant());
			TypeHash = typeHash;
			InstanceHash = instanceHash;

			AssetReferences = new List<uint[]>();
			WeakAssetReferences = new List<uint[]>();

			Type = type;
			Name = instance;
			SourceFile = source;

			IsNew = false;
		}
	}
}
