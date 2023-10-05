namespace Space_Refinery_Engine
{
	public interface Entity
	{
		public abstract IInformationProvider InformationProvider { get; }

		public void Tick();

		public void Interacted();

		public void Destroy();
	}
}