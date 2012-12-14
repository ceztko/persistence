using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

namespace Utilities
{
    public static class MethodUtils
    {
        public static MethodInfo BindToMethod(Type type, MethodInfo[] methods,
            BindingFlags bindingFlags, object[] args)
        {
            if (methods.Length == 0)
                return null;

            if (args == null)
                args = new object[0];

            object state;
            MethodBase foundMethod = Type.DefaultBinder.BindToMethod(bindingFlags,
                methods, ref args, null, CultureInfo.CurrentCulture, null,
                out state);

            if (state != null)
                Type.DefaultBinder.ReorderArgumentArray(ref args, state);

            return (MethodInfo)foundMethod;
        }

        /// <summary>Invoke all overrides of a method hierarchically. If the method is not
        /// virtual, it will be executed normally</summary>
        public static void HierarchicalInvoke(object thisInstance, string methodName,
            object[] args, bool reversed)
        {
            Type currentType = thisInstance.GetType();

            LinkedList<MethodBase> executionList = new LinkedList<MethodBase>();
            while (currentType != null)
            {
                // Find all the overloads of name "methodName" declared in this type
                // at this level
                MethodInfo[] overloads = GetMethods(currentType, methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                // Bind to the correct method among the overloads with the provided
                // arguments
                MethodInfo matchedMethod = BindToMethod(currentType, overloads,
                    BindingFlags.Instance | BindingFlags.Public, args);

                if (matchedMethod == null)
                {
                    currentType = currentType.BaseType;
                    continue;
                }

                if (matchedMethod.IsAbstract)
                    break;
                else
                {
                    if (reversed)
                        executionList.AddLast(matchedMethod);
                    else
                        executionList.AddFirst(matchedMethod);
                }

                // If the method is final and was declared here
                if (matchedMethod.IsVirtual
                        && matchedMethod == matchedMethod.GetBaseDefinition())
                    break;

                currentType = currentType.BaseType;
            }

            foreach (MethodInfo method in executionList)
            {
                if (!method.IsAbstract)
                    NonVirtualInvoke(thisInstance, method, args);
            }
        }

        /// <summary>
        /// Rick Byers
        /// http://blogs.msdn.com/b/rmbyers/archive/2008/08/16/invoking-a-virtual-method-non-virtually.aspx
        /// </summary>
        public static object NonVirtualInvoke(object thisInstance, MethodInfo method,
            object[] args)
        {
            // Use LCG to generate a temporary method that uses a 'call' instruction to
            // invoke the supplied method non-virtually.
            // Doing a non-virtual call on a virtual method outside the class that 
            // defines it will normally generate a VerificationException (PEVerify 
            // says "The 'this' parameter to the call must be the callng method's 
            // 'this' parameter.").  By associating the method with a type ("Program") 
            // in a full-trust assembly, we tell the JIT to skip this verification step.
            // Alternately we might want to associate it with method.DeclaringType - the
            // verification might then pass even if it's not skipped (eg. partial trust).
            var paramTypes = new List<Type>();

            if (!method.IsStatic)
                paramTypes.Add(method.DeclaringType);
            paramTypes.AddRange(method.GetParameters().Select(p => p.ParameterType));
            DynamicMethod dm = new DynamicMethod(
                "NonVirtualInvoker",    // name
                method.ReturnType,      // same return type as method we're calling 
                paramTypes.ToArray(),   // same parameter types as method we're calling
                method.DeclaringType); // associates with this full-trust code
            ILGenerator il = dm.GetILGenerator();
            for (int i = 0; i < paramTypes.Count; i++)
                il.Emit(OpCodes.Ldarg, i);             // load all args
            il.EmitCall(OpCodes.Call, method, null);   // call the method non-virtually
            il.Emit(OpCodes.Ret);                      // return what the call returned

            List<object> fullArgsList;
            if (args == null)
            {
                fullArgsList = new List<object>(1);
                fullArgsList.Add(thisInstance);
            }
            else
            {
                fullArgsList = new List<object>(args.Length + 1);
                fullArgsList.Add(thisInstance);
                fullArgsList.AddRange(args);
            }

            // Call the emitted method, which in turn will call the method requested
            return dm.Invoke(null, fullArgsList.ToArray());
        }

        public static MethodInfo[] GetMethods(Type type, string name,
            BindingFlags bindingFlgs)
        {
            List<MethodInfo> ret = new List<MethodInfo>();
            foreach (MethodInfo method in type.GetMethods(bindingFlgs))
            {
                if (method.Name == name)
                    ret.Add(method);
            }
            return ret.ToArray();
        }
    }
}
