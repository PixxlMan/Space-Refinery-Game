using FixedPrecision;
using System.Xml;

namespace Space_Refinery_Engine
{
	public struct ConstructionInfo
	{
		public int IndexOfSelectedConnector;
		public FixedDecimalLong8 Rotation;

		public ConstructionInfo(int indexOfSelectedConnector, FixedDecimalLong8 rotation)
		{
			IndexOfSelectedConnector = indexOfSelectedConnector;
			Rotation = rotation;
		}

		public void Serialize(XmlWriter writer)
		{
			writer.WriteStartElement(nameof(ConstructionInfo));
			{
				writer.WriteElementString(nameof(IndexOfSelectedConnector), IndexOfSelectedConnector.ToString());

				writer.Serialize(Rotation);
			}
			writer.WriteEndElement();
		}

		public static ConstructionInfo Deserialize(XmlReader reader)
		{
			int indexOfSelectedConnector;
			FixedDecimalLong8 rotation;

			reader.ReadStartElement(nameof(ConstructionInfo));
			{
				reader.ReadStartElement(nameof(IndexOfSelectedConnector));
				{
					indexOfSelectedConnector = int.Parse(reader.ReadContentAsString());
				}
				reader.ReadEndElement();

				rotation = reader.DeserializeFixedDecimalLong8();
			}
			reader.ReadEndElement();

			return new(indexOfSelectedConnector, rotation);
		}
	}
}