using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Logging;

namespace WKLocalizationLoader
{
    public class ModuleLoadResult
    {
        private ManualLogSource _logger;
        public List<ModuleInfo> ModuleInfos;

        public ModuleLoadResult(ManualLogSource logger)
        {
            _logger = logger;
            ModuleInfos = new List<ModuleInfo>();
        }

        public void AddOKModule(Type moduleClass)
        {
            var message = $"Loaded \"{moduleClass.Name}\" successfully.";
            AddModuleInfo(moduleClass, ModuleStatus.OK, message);
        }

        public void AddDisabledModule(Type moduleClass)
        {
            var message =
                $"\"{moduleClass.Name}\" "
                + "is loaded but manually disabled in config.";
            AddModuleInfo(moduleClass, ModuleStatus.Disabled, message);
        }

        public void AddFileMissingModule(Type moduleClass)
        {
            var message =
                $"\"{moduleClass.Name}\" is missing its "
                + "associated .json file and disabled by default.";
            AddModuleInfo(moduleClass, ModuleStatus.Disabled, message);
        }

        public void AddConflictedModule(
            Type moduleClass,
            List<string> conflictedModGUIDs
        )
        {
            string message = null;
            if (conflictedModGUIDs is null || conflictedModGUIDs.Count == 0)
            {
                message =
                    $"\"{moduleClass.Name}\" is disabled to avoid conflicts.";
            }
            else
            {
                message =
                    $"\"{moduleClass.Name}\" is disabled "
                    + "to avoid conflicts with the following mod(s):\n"
                    + string.Join("\n", conflictedModGUIDs);
            }
            AddModuleInfo(moduleClass, ModuleStatus.Conflicted, message);
        }

        public void AddDeserializationFailedModule(
            Type moduleClass,
            string filePath,
            Exception e
        )
        {
            var message = "An error occurred while deserializing "
                + $"\"{moduleClass.Name}\" from \"{filePath}\".\n"
                + e.Message;
            AddModuleInfo(moduleClass, ModuleStatus.Failed, message);
        }

        public void AddModuleInfo(
            Type moduleClass,
            ModuleStatus status,
            string message
        )
        {
            var moduleInfo = new ModuleInfo(moduleClass, status, message);
            ModuleInfos.Add(moduleInfo);
        }

        public List<Type> FilterModuleClassesByModuleStatus(
            ModuleStatus status
        )
        {
            if (ModuleInfos is null) return null;
            var filteredModuleClasses = ModuleInfos
                .Where(m => m.Status == status)
                .Select(m => m.ModuleClass)
                .ToList();
            return filteredModuleClasses;
        }

        public void PrintModuleInfoMessageBySeverity(
            ModuleStatus minSeverity
        )
        {
            if (ModuleInfos is null || _logger is null) return;
            var filteredModuleInfos = ModuleInfos
                .Where(m => m.Status >= minSeverity)
                .ToList();
            foreach (var moduleInfo in filteredModuleInfos)
            {
                PrintModuleInfoMessage(moduleInfo);
            }
        }

        public void PrintModuleInfoMessage(ModuleInfo moduleInfo)
        {
            if (_logger is null) return;
            switch (moduleInfo.Status)
            {
                case ModuleStatus.OK:
                    _logger.LogInfo(moduleInfo.Message);
                    break;
                case ModuleStatus.Disabled:
                    _logger.LogInfo(moduleInfo.Message);
                    break;
                case ModuleStatus.Conflicted:
                    _logger.LogInfo(moduleInfo.Message);
                    break;
                case ModuleStatus.Failed:
                    _logger.LogError(moduleInfo.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("How?");
            }
        }
    }
}

