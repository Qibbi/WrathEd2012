using System;
using System.Collections.Generic;
using System.Xml;

namespace SAGE
{
	public abstract class BaseEntryType
	{
		protected const string String = "String";
		protected const string Byte = "Byte";
		protected const string Angle = "Angle";
		protected const string Percentage = "Percentage";
		protected const string SageBool = "SageBool";
		protected const string SageInt = "SageInt";
		protected const string SageReal = "SageReal";
		protected const string SageUnsignedInt = "SageUnsignedInt";
		protected const string SageUnsignedShort = "SageUnsignedShort";
		protected const string Time = "Time";
		protected const string Velocity = "Velocity";
		protected const string DurationUnsignedInt = "DurationUnsignedInt";

		protected const string RequiredNotFoundAttribute = "Critical Error: The required attribute {0} was not found in:\n{1}";
		protected const string RequiredNotFoundElement = "Critical Error: The required element {0} was not found in:\n{1}";
		protected const string NotRequiredNotFoundElement = "Critical Error: The element {0} was not found and handling for that isn't implemented yet in:\n{1}";

		public string id { get; protected set; }
		public bool IsAttribute { get; protected set; }
		public bool IsRequired { get; protected set; }
		public bool IsVoid { get; protected set; }

		public BaseEntryType(WrathEdXML.AssetDefinition.BaseEntryType entry)
		{
			id = entry.id;
			IsAttribute = entry.IsAttribute;
			IsRequired = entry.IsRequired;
			IsVoid = entry.IsVoid;
		}

		public abstract bool Compile(Uri baseUri, BinaryAsset asset, XmlNode node, GameDefinition game, string trace, ref int position, out string ErrorDescription);
	}
}
