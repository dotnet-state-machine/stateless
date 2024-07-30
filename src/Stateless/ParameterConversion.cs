using System;

namespace Stateless
{
    static class ParameterConversion
    {
        public static object Unpack(object[] args, Type argType, int index)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            
            if (args.Length == 0)
                return null;
            
            if (args.Length <= index)
                throw new ArgumentException(
                    string.Format(ParameterConversionResources.ArgOfTypeRequiredInPosition, argType, index));

            var arg = args[index];

            if (arg != null && !argType.IsAssignableFrom(arg.GetType()))
                throw new ArgumentException(
                    string.Format(ParameterConversionResources.WrongArgType, index, arg.GetType(), argType));

            return arg;
        }
        
        public static bool TryUnpack(object[] args, Type argType, int index, out object result)
        {
            if (args == null)
            {
                result = null;
                return false;
            }

            if (args.Length == 0)
            {
                result = null;
                return false;
            }

            if (args.Length <= index)
            {
                result = null;
                return false;
            }

            var arg = args[index];

            if (arg != null && !argType.IsAssignableFrom(arg.GetType()))
            {
                result = null;
                return false;
            }

            result = arg;
            return true;
        }

        public static TArg Unpack<TArg>(object[] args, int index)
        {
            if (args.Length == 0) return default;

            return (TArg)Unpack(args, typeof(TArg), index);
        }
        
        public static bool TryUnpack<TArg>(object[] args, int index, out TArg result)
        {
            if (args.Length == 0)
            {
                result = default;
                return true;
            }

            object rawResult;
            if (!TryUnpack(args, typeof(TArg), index, out rawResult))
            {
                result = default;
                return false;
            }

            result = (TArg) rawResult;
            return true;
        }

        public static void Validate(object[] args, Type[] expected)
        {
            if (args.Length > expected.Length)
                throw new ArgumentException(
                    string.Format(ParameterConversionResources.TooManyParameters, expected.Length, args.Length));

            for (int i = 0; i < expected.Length; ++i)
                Unpack(args, expected[i], i);
        }
    }
}
