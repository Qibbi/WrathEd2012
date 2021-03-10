using SAGE.Stream;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SAGE.Compiler
{
    public abstract class CompileHandler
    {
        public abstract bool Compile(GameAssetType gameAsset, Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game,
            string trace, ref int position, out string ErrorDescription);

        public virtual bool HasSpecialDecompileHandling()
        {
            return false;
        }

        public virtual void Decompile(Stream.File stream, List<Stream.File> referencedStreams,
            XmlDocument document, XmlNode entryNode, List<AssetReference> assetReferences,
            byte[] bin, byte[] imp, byte[] relo)
        {
        }
    }
}
