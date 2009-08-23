using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    static class ParameterExtensions
    {
        public static object Unpack(this object[] args, Type argType, int index)
        {
            Enforce.ArgumentNotNull(args, "args");

            if (args.Length <= index)
                throw new ArgumentException(
                    string.Format(ParameterExtensionsResources.ArgOfTypeRequiredInPosition, argType, index));

            var arg = args[index];

            if (arg != null && !argType.IsAssignableFrom(arg.GetType()))
                throw new ArgumentException(
                    string.Format(ParameterExtensionsResources.WrongArgType, index, arg.GetType(), argType));

            return arg;
        }

        public static TArg Unpack<TArg>(this object[] args, int index)
        {
            return (TArg)args.Unpack(typeof(TArg), index);
        }
    }
}
