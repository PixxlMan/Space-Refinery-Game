using System.Runtime.Serialization;

namespace Space_Refinery_Utilities;

[Serializable]
public class GlitchInTheMatrixException : Exception
{
	public GlitchInTheMatrixException() : base("Something went wrong that shouldn't possibly go wrong.")
	{
	}

	public GlitchInTheMatrixException(string message) : base(message)
	{
	}

	public GlitchInTheMatrixException(string message, Exception innerException) : base(message, innerException)
	{
	}

	protected GlitchInTheMatrixException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}