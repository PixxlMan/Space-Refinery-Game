namespace Space_Refinery_Game
{
	public interface IUIInspectable
	{
		public void DoUIInspectorReadonly();

		public IUIInspectable DoUIInspectorEditable();
	}
}