using System;

namespace WKLocalizationLoader.Modules
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = false
    )]
    public class CrossGameVersionCompatibleAttribute : Attribute
    {
    }
}

