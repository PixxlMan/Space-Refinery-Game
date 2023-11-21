namespace Space_Refinery_Engine;

public interface IConstruction : ISerializableReference, Entity
{
	public void Deconstruct();
}