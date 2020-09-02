using System.IO;
using UnityEditor;
using UnityEngine;
using Innoactive.Creator.XRInteraction;

namespace Innoactive.CreatorEditor.XRInteraction
{
    /// <summary>
    /// Settings for <see cref="SnapZone"/>s for e.g. automatic creation of such snap zones.
    /// </summary>
    [CreateAssetMenu(fileName = "SnapZoneSettings", menuName = "Innoactive/SnapZoneSettings", order = 1)]
    public class SnapZoneSettings : ScriptableObject
    {
        private const string MaterialsPath = "Assets/Resources/SnapZones";
        private static SnapZoneSettings settings;
        
        /// <summary>
        /// Only Interactables with this LayerMask will interact with this <see cref="Innoactive.Creator.XRInteraction.SnapZone"/>.
        /// </summary>
        [Tooltip("Only Interactables with this LayerMask will interact with this SnapZone.")]
        public LayerMask InteractionLayerMask = 1;

        /// <summary>
        /// This color is used as the snap zone highlight color when no object is hovering a <see cref="SnapZone"/>.
        /// </summary>
        [Tooltip("This color is used as the snap zone highlight color when no object is hovering but `Snap Zone Active` is true.")]
        public Color HighlightColor = new Color32(64, 200, 255, 50);
        
        [SerializeField]
        [Tooltip("The material used for the highlight object. Should be transparent.\n\n[This field overrides 'HighlightColor']")]
        private Material highlightMaterial;

        /// <summary>
        /// This color is used when a valid <see cref="InteractableObject"/> is hovering a <see cref="SnapZone"/>.
        /// </summary>
        [Tooltip("This color is used when a valid object is hovering the snap zone.")]
        public Color ValidationColor = new Color32(0, 255, 0, 50);
        
        /// <summary>
        /// This color is used when an invalid <see cref="InteractableObject"/> is hovering a <see cref="SnapZone"/>.
        /// </summary>
        [Tooltip("This color is used when an invalid object is hovering the snap zone.")]
        public Color InvalidColor = new Color32(255, 0, 0, 50);

        [SerializeField]
        [Tooltip("The material shown when a valid object is hovering the snap zone. Should be transparent.\n\n[This field overrides 'ValidHighlightColor']")]
        private Material validationMaterial;

        [SerializeField]
        [Tooltip("The material shown when an invalid object is hovering the snap zone. Should be transparent.\n\n[This field overrides 'InvalidHighlightColor']")]
        private Material invalidMaterial;

        /// <summary>
        /// The material used for drawing when an <see cref="InteractableObject"/> is hovering a <see cref="SnapZone"/>. Should be transparent.
        /// </summary>
        public Material HighlightMaterial => SetupHighlightMaterial();

        /// <summary>
        /// The material used for the highlight object, when a valid object is hovering. Should be transparent.
        /// </summary>
        public Material ValidationMaterial => SetupValidationMaterial();
        
        /// <summary>
        /// The material used for the highlight object, when an invalid object is hovering. Should be transparent.
        /// </summary>
        public Material InvalidMaterial => SetupInvalidMaterial();

        /// <summary>
        /// Loads the first existing <see cref="SnapZoneSettings"/> found in the project.
        /// If non are found, it creates and saves a new instance with default values.
        /// </summary>
        public static SnapZoneSettings Settings => RetrieveSnapZoneSettings();

        /// <summary>
        /// Applies current settings to provided <see cref="SnapZone"/>.
        /// </summary>
        public void ApplySettingsToSnapZone(SnapZone snapZone)
        {
            snapZone.InteractionLayerMask = InteractionLayerMask;
            snapZone.ShownHighlightObjectColor = HighlightColor;
            snapZone.ValidationMaterial = ValidationMaterial;
            snapZone.InvalidMaterial = InvalidMaterial;
        }
        
        private static SnapZoneSettings RetrieveSnapZoneSettings()
        {
            if (settings == null)
            {
                string filter = "t:ScriptableObject SnapZoneSettings";
                    
                foreach (string guid in AssetDatabase.FindAssets(filter))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    return settings = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SnapZoneSettings)) as SnapZoneSettings;
                }

                settings = CreateNewConfiguration();
            }

            return settings;
        }
        
        private static SnapZoneSettings CreateNewConfiguration()
        {
            SnapZoneSettings snapZoneSettings = CreateInstance<SnapZoneSettings>();
            
            string filePath = "Assets/Resources";
            
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }
            
            AssetDatabase.CreateAsset(snapZoneSettings, $"{filePath}/{nameof(SnapZoneSettings)}.asset");
            AssetDatabase.Refresh();
            
            return snapZoneSettings;
        }
        
        private Material SetupHighlightMaterial()
        {
            if (highlightMaterial == null)
            {
                highlightMaterial = CreateMaterial();
                highlightMaterial.name = "SnapZoneHighlightMaterial";

                if (Directory.Exists(MaterialsPath) == false)
                {
                    Directory.CreateDirectory(MaterialsPath);
                }

                AssetDatabase.CreateAsset(highlightMaterial, $"{MaterialsPath}/{highlightMaterial.name}.mat");
                AssetDatabase.Refresh();
            }
                
            highlightMaterial.color = HighlightColor;
            return highlightMaterial;
        }
        
        private Material SetupInvalidMaterial()
        {
            if (invalidMaterial == null)
            {
                invalidMaterial = CreateMaterial();
                invalidMaterial.name = "SnapZoneInvalidMaterial";

                if (Directory.Exists(MaterialsPath) == false)
                {
                    Directory.CreateDirectory(MaterialsPath);
                }

                AssetDatabase.CreateAsset(invalidMaterial, $"{MaterialsPath}/{invalidMaterial.name}.mat");
                AssetDatabase.Refresh();
            }
                
            invalidMaterial.color = InvalidColor;
            return invalidMaterial;
        }
        
        private Material SetupValidationMaterial()
        {
            if (validationMaterial == null)
            {
                validationMaterial = CreateMaterial();
                validationMaterial.name = "SnapZoneValidationMaterial";

                if (Directory.Exists(MaterialsPath) == false)
                {
                    Directory.CreateDirectory(MaterialsPath);
                }

                AssetDatabase.CreateAsset(validationMaterial, $"{MaterialsPath}/{validationMaterial.name}.mat");
                AssetDatabase.Refresh();
            }
                
            validationMaterial.color = ValidationColor;
            return validationMaterial;
        }

        private Material CreateMaterial()
        {
            Shader standardShader = Shader.Find("Standard");

            Material material = new Material(standardShader);
            material.SetFloat("_Mode", 3);

            return material;
        }
    }
}
