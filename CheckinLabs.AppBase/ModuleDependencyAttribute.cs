using System;
using System.Collections.Generic;
using System.Text;

namespace CheckinLabs.AppBase
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ModuleDependencyAttribute : Attribute
    {
        public Type DependencyType { get; private set; }
        public ModuleDependencyAttribute(Type depType) : base()
        {
            if (typeof(StartModuleBase).IsAssignableFrom(depType))
            {
                DependencyType = depType;
            }
        }
    }
}
