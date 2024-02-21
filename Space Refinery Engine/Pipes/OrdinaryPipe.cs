using Space_Refinery_Engine;
using Space_Refinery_Utilities;
using System.Xml;

namespace Space_Refinery_Engine;

public sealed class OrdinaryPipe : Pipe
{
	private OrdinaryPipe()
	{
		informationProvider = new OrdinaryPipeInformationProvider(this);
	}

	public ResourceContainer ResourceContainer;

	public override void Tick()
	{
#if DEBUG
		DebugStopPoints.TickStopPoint(SerializableReference);
#endif

		ResourceContainer.Tick(Time.TickInterval);
	}

	public override void TransferResourceFromConnector(ResourceContainer source, VolumeUnit volume, PipeConnector _)
	{
		lock (SyncRoot)
		{
			ResourceContainer.TransferResourceByVolume(source, volume);
		}
	}

	protected override void DisplaceContents()
	{
		lock (SyncRoot)
		{
			List<PipeConnector> connectedConnectors = new();
			foreach (var connector in Connectors)
			{
				if (!connector.Vacant)
					connectedConnectors.Add(connector);
			}

			if (connectedConnectors.Count == 0)
			{
				return;
			}

			var volumePerConnector = (VolumeUnit)((DecimalNumber)ResourceContainer.Volume / connectedConnectors.Count);

			foreach (var connectedConnector in connectedConnectors)
			{
				connectedConnector.TransferResource(this, ResourceContainer, volumePerConnector);
			}
		}
	}

	protected override void SetUp()
	{
		lock (SyncRoot)
		{
			ResourceContainer ??= new(PipeType.PipeProperties.FlowableVolume);
		}
	}

	public override ResourceContainer GetResourceContainerForConnector(PipeConnector pipeConnector)
	{
		return ResourceContainer;
	}

	public override void SerializeState(XmlWriter writer)
	{
		base.SerializeState(writer);

		ResourceContainer.Serialize(writer);
	}

	public override void DeserializeState(XmlReader reader, SerializationData serializationData, SerializationReferenceHandler referenceHandler)
	{
		base.DeserializeState(reader, serializationData, referenceHandler);

		ResourceContainer = ResourceContainer.Deserialize(reader);
	}
}
