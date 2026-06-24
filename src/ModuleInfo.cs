using System;

namespace WKLocalizationLoader
{
    public class ModuleInfo
    {
        public Type ModuleClass;
        public ModuleStatus Status;
        public string Message;

        public ModuleInfo(
            Type moduleClass,
            ModuleStatus status,
            string message
        )
        {
            ModuleClass = moduleClass;
            Status = status;
            Message = message;
        }
    }
}

