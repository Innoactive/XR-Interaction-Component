using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Innoactive.Creator.XR.Editors.Utils
{
    /// <summary>
    /// Makes sure the XR-Interaction package is enabled.
    /// </summary>
    [InitializeOnLoad]
    public class XRInteractionPackageValidator
    {
        private const string CreatorXRInteractionSymbol = "CREATOR_XR_INTERACTION";
        private const string XRInteractionPackage = "com.unity.xr.interaction.toolkit";
        private static ListRequest listRequest;
        private static AddRequest addRequest;
        
        static XRInteractionPackageValidator()
        {
            RequestPackageList();
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            PackageCollection packageCollection = RetrievePackageListResult();

            if (packageCollection != null)
            {
                if (IsPackageEnabledInCollection(XRInteractionPackage, packageCollection))
                {
                    ValidateScriptingSymbols();
                    EditorApplication.update -= EditorUpdate;
                }
                else
                {
                    addRequest = EnablePackage(XRInteractionPackage);
                }
            }
            
            LogFinalPackageStatus();
        }
        
        private static void RequestPackageList()
        {
            listRequest = Client.List();
        }
        
        private static PackageCollection RetrievePackageListResult()
        {
            if (listRequest == null || listRequest.IsCompleted == false)
            {
                return null;
            }

            PackageCollection packageCollection = null;
            
            if (listRequest.Status == StatusCode.Failure)
            {
                // UB can't occur in this case.
                // ReSharper disable once DelegateSubtraction
                EditorApplication.update -= EditorUpdate;
                Debug.LogErrorFormat("There was an error trying to enable '{0}'.\n{1}", XRInteractionPackage, listRequest.Error.message);
            }
            else
            {
                packageCollection = listRequest.Result;
            }

            listRequest = null;

            return packageCollection;
        }

        private static bool IsPackageEnabledInCollection(string packageName, PackageCollection packageCollection)
        {
            return packageCollection.Any(packageInfo => packageInfo.name == packageName);
        }

        private static AddRequest EnablePackage(string packageName)
        {
            return Client.Add(packageName);
        }

        private static void LogFinalPackageStatus()
        {
            if (addRequest == null || addRequest.IsCompleted == false)
            {
                return;
            }

            if (addRequest.Status == StatusCode.Success)
            {
                ValidateScriptingSymbols();
                Debug.LogFormat("The package '{0}' has been automatically added", addRequest.Result.displayName);
            }
            else if (addRequest.Status >= StatusCode.Failure)
            {
                Debug.LogErrorFormat("There was an error trying to enable '{0}'.\n{1}", XRInteractionPackage, addRequest.Error.message);
            }

            addRequest = null;

            EditorApplication.update -= EditorUpdate;
        }
        
        private static void ValidateScriptingSymbols()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            List<string> symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').ToList();
        
            if (symbols.Contains(CreatorXRInteractionSymbol) == false)
            {
                symbols.Add(CreatorXRInteractionSymbol);
        
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", symbols.ToArray()));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            }
        }
    }
}
