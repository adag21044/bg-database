using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BansheeGz.BGDatabase.Editor
{
    /// <summary>
    /// If database asset was saved with JSON format before building and it was switched to binary format
    /// during build process, switch it back to JSON format
    /// </summary>
    public class BGDatabaseSwitchFormatOnPostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPostprocessBuild(BuildReport report)
        {
            if (BGRepo.Ok && BGDatabaseSwitchFormatOnPreBuild.Switched)
            {
                BGDatabaseSwitchFormatOnPreBuild.Switched = false;
                BGDatabaseSwitchFormatOnPreBuild.Switch(BGAddonSettings.FormatEnum.Binary, BGAddonSettings.FormatEnum.Json);
            }
        }
    }
}