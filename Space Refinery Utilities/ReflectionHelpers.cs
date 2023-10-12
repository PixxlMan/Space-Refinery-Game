using System.Reflection;

namespace Space_Refinery_Utilities;

public static class ReflectionHelpers
{
	public static MethodInfo GetImplementedMethod(this Type targetType, MethodInfo interfaceMethod) // https://stackoverflow.com/questions/1113635/how-to-get-methodinfo-of-interface-method-having-implementing-methodinfo-of-cla
	{
		if (targetType is null) throw new ArgumentNullException(nameof(targetType));
		if (interfaceMethod is null) throw new ArgumentNullException(nameof(interfaceMethod));

		var map = targetType.GetInterfaceMap(interfaceMethod.DeclaringType);
		var index = Array.IndexOf(map.InterfaceMethods, interfaceMethod);
		if (index < 0) return null;

		return map.TargetMethods[index];
	}
}
