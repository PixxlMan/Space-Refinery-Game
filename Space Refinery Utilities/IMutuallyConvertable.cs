namespace Space_Refinery_Utilities;

/// <summary>
/// A type <typeparamref name="TSelf"/> implementing this interface will always be explicitly convertable to <typeparamref name="TMutual"/>.
/// <typeparamref name="TMutual"/> will also always be explicitly convertable to <typeparamref name="TSelf"/>.
/// </summary>
/// <remarks>
/// Both <typeparamref name="TSelf"/> and <typeparamref name="TMutual"/> need to implement the <c>IMutuallyConvertable</c> interface.
/// </remarks>
/// <typeparam name="TSelf">The type implementing this interface.</typeparam>
/// <typeparam name="TMutual">The type with which to have explicit mutual convertability.</typeparam>
public interface IMutuallyConvertable<TSelf, TMutual>
	where TSelf : IMutuallyConvertable<TSelf, TMutual>
	where TMutual : IMutuallyConvertable<TMutual, TSelf>
{
	public static abstract explicit operator TMutual(TSelf self);
}

/// <summary>
/// A type <typeparamref name="TSelf"/> implementing this interface will always be implicitly convertable to <typeparamref name="TMutual"/>.
/// <typeparamref name="TMutual"/> will also always be implcitly convertable to <typeparamref name="TSelf"/>.
/// </summary>
/// <remarks>
/// Both <typeparamref name="TSelf"/> and <typeparamref name="TMutual"/> need to implement the <c>IInterchangeable</c> interface.
/// </remarks>
/// <typeparam name="TSelf">The type implementing this interface.</typeparam>
/// <typeparam name="TMutual">The type with which to have implicit mutual convertability.</typeparam>
public interface IInterchangeable<TSelf, TMutual>
	where TSelf : IInterchangeable<TSelf, TMutual>
	where TMutual : IInterchangeable<TMutual, TSelf>
{
	public static abstract implicit operator TMutual(TSelf self);
}