using System;
using System.Reflection;

namespace Stateless
{
    internal static class ReflectionExtensions
    {
        public static Assembly GetAssembly(this Type type)
        {
            return type.Assembly;
        }

        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.IsAssignableFrom(otherType);
        }

        /// <summary>
        ///     Convenience method to get <see cref="MethodInfo" /> for different PCL profiles.
        /// </summary>
        /// <param name="del">Delegate whose method info is desired</param>
        /// <returns>Null if <paramref name="del" /> is null, otherwise <see cref="MemberInfo.Name" />.</returns>
        public static MethodInfo TryGetMethodInfo(this Delegate del)
        {
            return del?.Method;
        }

        /// <summary>
        ///     Convenience method to get method name for different PCL profiles.
        /// </summary>
        /// <param name="del">Delegate whose method name is desired</param>
        /// <returns>Null if <paramref name="del" /> is null, otherwise <see cref="MemberInfo.Name" />.</returns>
        public static string TryGetMethodName(this Delegate del)
        {
            return TryGetMethodInfo(del)?.Name;
        }
    }
}