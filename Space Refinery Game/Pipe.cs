using FXRenderer;

namespace Space_Refinery_Game
{
	public interface Pipe : Entity, IConstruction
	{
		public Transform Transform { get; protected set; }
	}
}