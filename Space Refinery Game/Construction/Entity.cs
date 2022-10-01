namespace Space_Refinery_Game
{
	public interface Entity
	{
		public abstract IInformationProvider InformationProvider { get; }

		public void Tick();

		public void Interacted();

		public void Destroyed();
	}
}