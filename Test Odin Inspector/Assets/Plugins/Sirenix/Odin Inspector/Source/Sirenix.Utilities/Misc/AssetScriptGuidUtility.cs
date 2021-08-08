//-----------------------------------------------------------------------
// <copyright file="AssetScriptGuidUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Utility class for asset Guid script
    /// </summary>
    public static class AssetScriptGuidUtility
    {
        /// <summary>
        /// Tries to update the Guid of a specified asset with the Guid from a specified script type.
        /// </summary>
        public static bool TryUpdateAssetScriptGuid(string fullAssetPath, Type scriptType)
        {
            Debug.Log("TryUpdateAssetScriptGuid");
            FileInfo assetFileInfo = new FileInfo(fullAssetPath);

            if (!assetFileInfo.Exists)
            {
                throw new ArgumentException("No asset file exists to be corrected at path '" + fullAssetPath + "'.");
            }

            if (!scriptType.ImplementsOrInherits(typeof(ScriptableObject)))
            {
                throw new NotSupportedException("UpdateAssetGuid only supports updating the asset script guids of types derived from ScriptableObject.");
            }

            ScriptableObject temp = ScriptableObject.CreateInstance(scriptType);

            if (temp == null)
            {
                Debug.Log("Could not create scriptable object of type " + scriptType.GetNiceName());
                return false;
            }

            MonoScript monoScript = MonoScript.FromScriptableObject(temp);
            UnityEngine.Object.DestroyImmediate(temp);

            if (monoScript == null)
            {
                Debug.Log("Could not find MonoScript for type " + scriptType.GetNiceName());
                return false;
            }

            string scriptPath = AssetDatabase.GetAssetPath(monoScript);
            string scriptGuid = AssetDatabase.AssetPathToGUID(scriptPath);

            try
            {
                new Guid(scriptGuid);
                Debug.Log("Found valid script guid to set: " + scriptGuid);
            }
            catch
            {
                Debug.Log("Invalid script guid found: " + scriptGuid);
                return false;
            }

            string[] assetFile = File.ReadAllLines(assetFileInfo.FullName);

            for (int i = 0; i < assetFile.Length; i++)
            {
                string line = assetFile[i];
                const string m_ScriptPattern = "m_Script: ";

                if (line.TrimStart(' ', '\t').StartsWith(m_ScriptPattern, StringComparison.InvariantCulture))
                {
                    Debug.Log("Found current file line '" + line + "'");

                    temp = ScriptableObject.CreateInstance(scriptType);

                    // We can use 'Assets/' here as it's a temporary file that will be deleted later.
                    string tempName = Guid.NewGuid().ToString() + ".asset";
                    string tempAssetPath = "Assets/" + tempName;
                    string tempFullPath = Application.dataPath + "/" + tempName;

					bool deleted = false;

					try
					{
						AssetDatabase.CreateAsset(temp, tempAssetPath);
						AssetDatabase.SaveAssets();

						if (File.Exists(tempFullPath))
						{
							try
							{
								string[] tempAssetFile = File.ReadAllLines(tempFullPath);

								for (int j = 0; j < tempAssetFile.Length; j++)
								{
									string tempLine = tempAssetFile[j];

									if (tempLine.TrimStart(' ', '\t').StartsWith(m_ScriptPattern, StringComparison.InvariantCulture))
									{
										//
										// We did it! We made a valid temp asset, and can now copy over the m_Script line verbatim
										//

										Debug.Log("Found new file line to set '" + tempLine + "'");

										assetFile[i] = tempLine;
										File.WriteAllLines(fullAssetPath, assetFile);
										return true;
									}
								}
							}
							finally
							{
								File.Delete(tempFullPath);
								File.Delete(tempFullPath + ".meta");
								AssetDatabase.Refresh();
								deleted = true;
							}
						}
						else
						{
							AssetDatabase.DeleteAsset(tempAssetPath);
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
							deleted = true;
						}
					}
					finally
					{
						if (!deleted)
						{
							File.Delete(tempFullPath);
							File.Delete(tempFullPath + ".meta");
							AssetDatabase.DeleteAsset(tempAssetPath);
							AssetDatabase.Refresh();
						}
					}

                    break;
                }
            }

            return false;
        }
    }
}

#endif