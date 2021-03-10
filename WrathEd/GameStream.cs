using System;
using System.Collections.Generic;

namespace WrathEd
{
	public class GameStream
	{
		public SAGE.Stream.File StreamManifest { get; set; }
		public List<SAGE.Stream.File> StreamBases { get; set; }
		public List<SAGE.Stream.File> StreamReferences { get; set; }
		public SAGE.Big.PackedFile StreamManifestEntry { get; set; }
		public List<SAGE.Big.PackedFile> StreamBaseEntries { get; set; }
		public List<SAGE.Big.PackedFile> StreamReferenceEntries { get; set; }

		public GameStream(SAGE.Stream.File manifest, SAGE.Big.PackedFile manifestEntry,
			List<SAGE.Stream.File> manifestFiles, List<SAGE.Big.PackedFile> manifestFileEntries, Dictionary<string, string> versionFiles)
		{
			StreamBases = new List<SAGE.Stream.File>();
			StreamReferences = new List<SAGE.Stream.File>();
			StreamBaseEntries = new List<SAGE.Big.PackedFile>();
			StreamReferenceEntries = new List<SAGE.Big.PackedFile>();
			StreamManifest = manifest;
			StreamManifestEntry = manifestEntry;

			foreach (SAGE.Stream.StreamReference reference in StreamManifest.StreamReferences)
			{
				string streamName = reference.Path;
				if (reference.ReferenceType == SAGE.Stream.StreamReferenceType.REFERENCE)
				{
					string streamNameWithoutExtension = streamName.Substring(0, streamName.LastIndexOf('.'));
					foreach (KeyValuePair<string, string> version in versionFiles)
					{
						if (version.Key.EndsWith(streamNameWithoutExtension))
						{
							streamName = streamNameWithoutExtension + version.Value + ".manifest";
							break;
						}
					}
				}
				for (int idx = 0; idx < manifestFileEntries.Count; ++idx)
				{
					if (manifestFileEntries[idx].Name.ToLowerInvariant().EndsWith(streamName.ToLowerInvariant()))
					{
						switch (reference.ReferenceType)
						{
							case SAGE.Stream.StreamReferenceType.PATCH:
								StreamBaseEntries.Add(manifestFileEntries[idx]);
								StreamBases.Add(manifestFiles[idx]);
								GetBaseManifest(manifestFiles[idx], manifestFiles, manifestFileEntries);
								break;
							case SAGE.Stream.StreamReferenceType.REFERENCE:
								StreamReferenceEntries.Add(manifestFileEntries[idx]);
								StreamReferences.Add(manifestFiles[idx]);
								break;
						}
					}
				}
			}
		}

		private void GetBaseManifest(SAGE.Stream.File manifest, List<SAGE.Stream.File> manifestFiles, List<SAGE.Big.PackedFile> manifestFileEntries)
		{
			if (manifest.StreamReferences.Count > 0)
			{
				SAGE.Stream.StreamReference reference = manifest.StreamReferences[0];
				if (reference.ReferenceType == SAGE.Stream.StreamReferenceType.PATCH)
				{
					for (int idx = 0; idx < manifestFileEntries.Count; ++idx)
					{
						if (manifestFileEntries[idx].Name.ToLowerInvariant().EndsWith(reference.Path.ToLowerInvariant()))
						{
							StreamBaseEntries.Add(manifestFileEntries[idx]);
							StreamBases.Add(manifestFiles[idx]);
							GetBaseManifest(manifestFiles[idx], manifestFiles, manifestFileEntries);
						}
					}
				}
			}
		}
	}
}
