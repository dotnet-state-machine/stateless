﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Reflection;

namespace Stateless {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class StateMachineResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StateMachineResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
#if PORTABLE259
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Stateless.StateMachineResources", typeof(StateMachineResources).GetTypeInfo().Assembly);
#else
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Stateless.StateMachineResources", typeof(StateMachineResources).Assembly);
#endif
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameters for the trigger &apos;{0}&apos; have already been configured..
        /// </summary>
        internal static string CannotReconfigureParameters {
            get {
                return ResourceManager.GetString("CannotReconfigureParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No valid leaving transitions are permitted from state &apos;{1}&apos; for trigger &apos;{0}&apos;. Consider ignoring the trigger..
        /// </summary>
        internal static string NoTransitionsPermitted {
            get {
                return ResourceManager.GetString("NoTransitionsPermitted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Trigger &apos;{0}&apos; is valid for transition from state &apos;{1}&apos; but a guard condition is not met. Guard description: &apos;{2}&apos;..
        /// </summary>
        internal static string NoTransitionsUnmetGuardCondition {
            get {
                return ResourceManager.GetString("NoTransitionsUnmetGuardCondition", resourceCulture);
            }
        }
    }
}
