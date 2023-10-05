namespace Space_Refinery_Engine
{
	public interface IUIInspectable
	{
		public void DoUIInspectorReadonly();

		public IUIInspectable DoUIInspectorEditable();
	}
}