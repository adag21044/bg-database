using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BansheeGz.BGDatabase.Editor
{
    /// <summary>
    /// If database asset is saved with JSON format, switches it to binary format
    /// </summary>
    public class BGDatabaseSwitchFormatOnPreBuild : IPreprocessBuildWithReport
    {
        public static bool Switched;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!BGRepo.DefaultRepoLoaded) BGRepo.Load();
            if (!BGRepo.Ok) Debug.Log("Warning! Can not switch database format from JSON to binary, cause database can not be loaded!");
            else Switch(BGAddonSettings.FormatEnum.Json, BGAddonSettings.FormatEnum.Binary, () => Switched = true);
        }

        public static void Switch(BGAddonSettings.FormatEnum from, BGAddonSettings.FormatEnum to, Action onSuccess = null)
        {
            var addon = BGRepo.I.Addons.Get<BGAddonSettings>();
            if (addon == null || addon.Format != from) return;
            addon.Format = to;
            BGRepoSaver.SaveAndMarkAsSaved();
            onSuccess?.Invoke();
        }
    }
}