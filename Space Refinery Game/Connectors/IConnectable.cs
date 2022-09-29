namespace Space_Refinery_Game
{
	public interface IConnectable : ISerializableReference
	{
		public Connector[] Connectors { get; }
	}
}