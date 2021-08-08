#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CheckForUpdatesWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using UnityEngine;
    using UnityEditor;
    using Sirenix.Utilities.Editor;

    public class CheckForUpdatesWindow : EditorWindow
    {
        private const string CheckForUpdatesDailyPref = "CheckForUpdates.CheckDaily";
        private const string CheckBetaVersionsEnablePref = "CheckForUpdates.EnableBeta";
        private const string LastCheckDatePref = "CheckForUpdates.LastCheckDate";

        private static bool? checkDaily;
        private static bool? checkForBeta;
        private static FetchVersionResult state;
        private static SirenixVersion latestVersion;

        internal static bool CheckDailyEnabled
        {
            get
            {
                if (checkDaily == null)
                {
                    checkDaily = EditorPrefs.GetBool(CheckForUpdatesDailyPref, false);
                }

                return checkDaily.Value;
            }
            set
            {
                if (checkDaily == null || checkDaily.Value != value)
                {
                    checkDaily = value;
                    EditorPrefs.SetBool(CheckForUpdatesDailyPref, value);
                }
            }
        }

        internal static bool IncludeBetasEnabled
        {
            get
            {
                if (checkForBeta == null)
                {
                    checkForBeta = EditorPrefs.GetBool(CheckBetaVersionsEnablePref, false);
                }

                return checkForBeta.Value;
            }
            set
            {
                if (checkForBeta == null || checkForBeta.Value != value)
                {
                    checkForBeta = value;
                    EditorPrefs.SetBool(CheckBetaVersionsEnablePref, value);
                }
            }
        }

        [InitializeOnLoadMethod]
        private static void DailyCheckForUpdates()
        {
            if (CheckDailyEnabled)
            {
                EditorApplication.update -= WaitForVersionFetch;
                EditorApplication.update += WaitForVersionFetch;

                // DateTime.ticks is waay to big to fit into an EditorPref, but I only need to know the date,
                // so I've scaled the prefs' tick value down to a per day scale.
                var lastDays = EditorPrefs.GetInt(LastCheckDatePref, 0);
                var nowDays = (int)(DateTime.Now.Date.Ticks / TimeSpan.TicksPerDay);

                if (lastDays < nowDays)
                {
                    //#if SIRENIX_INTERNAL
                    //Debug.Log(">>> DAILY CHECK <<< Remove this before next patch.");
                    //#endif
                    EditorPrefs.SetInt(LastCheckDatePref, (int)nowDays);
                    StartTryRefreshLatestOdinVersion();
                }
            }
        }

        private static void WaitForVersionFetch()
        {
            if (state == FetchVersionResult.Success || state == FetchVersionResult.Failed)
            {
                EditorApplication.update -= WaitForVersionFetch;
                if (state == FetchVersionResult.Success
                    && (IsVersionHigher(latestVersion.version, OdinInspectorVersion.Version)
                    || IncludeBetasEnabled && IsVersionHigher(latestVersion.beta.version, OdinInspectorVersion.Version)))
                {
                    OpenWindow();
                }
            }
        }

        public static void OpenWindow()
        {
            var w = GetWindow<CheckForUpdatesWindow>("Odin Updates");
            w.position = GUIHelper.GetEditorWindowRect().AlignCenter(350, 150);
            w.minSize = new Vector2(350, 150);
            w.maxSize = new Vector2(350, 150);
        }

        private void OnEnable()
        {
            if (state == FetchVersionResult.None && state != FetchVersionResult.Fetching)
            {
                StartTryRefreshLatestOdinVersion();
            }
        }

        private void OnGUI()
        {
            Rect rect = new Rect(0, 0, this.position.width, this.position.height).Padding(4);

            GUI.enabled = state != FetchVersionResult.Fetching;
            if (SirenixEditorGUI.IconButton(rect.AlignRight(20).AlignTop(20), EditorIcons.Refresh, "Refresh"))
            {
                StartTryRefreshLatestOdinVersion();
            }
            GUI.enabled = true;

            var content = rect.AlignTop(rect.height - 48);

            GUI.DrawTexture(content.AlignLeft(64), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleToFit);

            content = content.AddXMin(64);

            if (state == FetchVersionResult.Fetching)
            {
                var r = content.AlignCenterY(20);
                GUI.DrawTexture(r.AlignLeft(20).SubY(2), EditorIcons.Refresh.Raw);
                GUI.Label(r.AddXMin(24), GUIHelper.TempContent("Getting latest version..."));
                GUIHelper.RequestRepaint();
            }
            else if (state == FetchVersionResult.Failed)
            {
                GUI.Label(
                    content,
                    GUIHelper.TempContent("Failed to fetch latest Odin Inspector version.\nPlease try again later."),
                    SirenixGUIStyles.MultiLineCenteredLabel);
            }
            else if (state == FetchVersionResult.Success)
            {
                Rect r = content.AlignCenterY(20);

                if (latestVersion.beta.available && IsVersionHigher(latestVersion.beta.version, OdinInspectorVersion.Version))
                {
                    r.y += 15;
                    GUI.DrawTexture(r.AlignLeft(20).SubY(2), EditorIcons.Bell.Raw);
                    GUI.Label(r.AddXMin(24), "Beta " + latestVersion.beta.version + " is available for download!");
                    r.y -= 25;
                }

                if (IsVersionHigher(latestVersion.version, OdinInspectorVersion.Version))
                {
                    GUI.DrawTexture(r.AlignLeft(20).SubY(2), EditorIcons.Bell.Raw);
                    GUI.Label(r.AddXMin(24), "Patch " + latestVersion.version + " is available for download!");
                }
                else
                {
                    GUI.color = Color.green;
                    GUI.DrawTexture(r.AlignLeft(20).SubY(2), EditorIcons.Checkmark.Raw);
                    GUI.color = Color.white;
                    GUI.Label(r.AddXMin(24), "Latest stable Odin version " + OdinInspectorVersion.Version + " installed");
                }
            }

            GUI.enabled = state == FetchVersionResult.Success;
            var buttons = rect.AlignBottom(44);

            if (GUI.Button(buttons.AlignTop(20).Split(0, 2), GUIHelper.TempContent("See patch notes")))
            {
                Application.OpenURL(latestVersion.patchNotesUrl);
            }
            if (GUI.Button(buttons.AlignTop(20).Split(1, 2), GUIHelper.TempContent("Download here")))
            {
                Application.OpenURL(latestVersion.downloadUrl);
            }

            GUI.enabled = true;
            CheckDailyEnabled = EditorGUI.ToggleLeft(buttons.AlignBottom(20).Split(0, 2), "Daily check for updates", CheckDailyEnabled);
            IncludeBetasEnabled = EditorGUI.ToggleLeft(buttons.AlignBottom(20).Split(1, 2), "Include betas", IncludeBetasEnabled);

            this.RepaintIfRequested();
        }
        private static string GetUpdateAvailableMessage(SirenixVersion version)
        {
            if (version.beta.available)
            {
                if (IsVersionHigher(version.beta.version, OdinInspectorVersion.Version))
                {
                    return "Beta version " + version.version + " is available for download at " + version.downloadUrl;
                }

            }
            else if (IsVersionHigher(version.version, OdinInspectorVersion.Version))
            {
                return "Update " + version.version + " is available for download at " + version.downloadUrl;
            }

            return "Looks like you're up to date with the latest version. You get a cookie for that.";
        }

        private static bool IsVersionHigher(string a, string b)
        {
            try
            {
                Version aVersion = new Version(a);
                Version bVersion = new Version(b);

                return Math.Max(aVersion.Major, 0) > Math.Max(bVersion.Major, 0)
                    || Math.Max(aVersion.Minor, 0) > Math.Max(bVersion.Minor, 0)
                    || Math.Max(aVersion.Build, 0) > Math.Max(bVersion.Build, 0)
                    || Math.Max(aVersion.Revision, 0) > Math.Max(bVersion.Revision, 0);
            }
            catch // No. I'm not dealing with Version throwing exceptions.
            {
                return false;
            }
        }

        private static void StartTryRefreshLatestOdinVersion()
        {
            if (state == FetchVersionResult.Fetching) return; // Should already be updating.

            state = FetchVersionResult.Fetching;
            new Thread(() => TryRefreshLatestOdinVersion())
            {
                IsBackground = true
            }
            .Start();
        }

        private static void TryRefreshLatestOdinVersion()
        {
            const string url = "http://odininspector.com/latest-version/odin-inspector";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    if (response.ContentType == "application/json; charset=utf-8")
                    {
                        latestVersion = JsonUtility.FromJson<SirenixVersion>(reader.ReadToEnd());
                        state = FetchVersionResult.Success;
                    }
                    else
                    {
                        latestVersion = default(SirenixVersion);
                        state = FetchVersionResult.Failed;
                    }
                }
            }
            catch
#if SIRENIX_INTERNAL
            (Exception ex)
#endif
            {
                latestVersion = default(SirenixVersion);
                state = FetchVersionResult.Failed;

#if SIRENIX_INTERNAL
                Debug.LogException(ex);
#endif
            }
        }

        [Serializable]
        private struct SirenixVersion
        {
            public string name;
            public string version;
            public string patchNotesUrl;
            public int releaseDateTicks;
            public BetaVersion beta;
            public string downloadUrl;

            [Serializable]
            public struct BetaVersion
            {
                public bool available;
                public string version;
                public int releaseDateTicks;
                public string patchNotesUrl;
            }
        }

        private enum FetchVersionResult : byte
        {
            None,
            Fetching,
            Failed,
            Success,
        };
    }
}
#endif