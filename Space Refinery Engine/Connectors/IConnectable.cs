namespace Space_Refinery_Engine
{
	public interface IConnectable : ISerializableReference
	{
		public Connector[] Connectors { get; }
	}
}