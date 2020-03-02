using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Innoactive.Creator.Editors.Utils
{
    [InitializeOnLoad]
    public class XRInteractionPackageValidator
    {
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

            if (packageCollection != null && IsPackageEnabledInCollection(XRInteractionPackage, packageCollection) == false)
            {
                addRequest = EnablePackage(XRInteractionPackage);
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
                Debug.LogFormat("The package '{0}' has been automatically added", addRequest.Result.displayName);
            }
            else if (addRequest.Status >= StatusCode.Failure)
            {
                Debug.LogErrorFormat("There was an error trying to enable {0}.\n{1}", XRInteractionPackage, addRequest.Error.message);
            }

            addRequest = null;

            EditorApplication.update -= EditorUpdate;
        }
    }
}
