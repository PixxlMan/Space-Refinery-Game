namespace Space_Refinery_Engine
{
	public interface Entity : ISerializableReference
	{
		public abstract IInformationProvider InformationProvider { get; }

		public void Tick();

		public void Interacted();

		public void Destroy();
	}
}