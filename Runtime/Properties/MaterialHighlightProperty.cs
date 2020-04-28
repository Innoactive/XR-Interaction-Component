using UnityEngine;
using Innoactive.Creator.Core.Properties;

namespace Innoactive.Creator.XRInteraction.Properties
{
    /// <summary>
    /// Highlight property which highlights this object by using a material.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class MaterialHighlightProperty : HighlightProperty
    {
        [Tooltip("Highlight Material")]
        [SerializeField] 
        protected Material highlightMaterial;
        
        /// <inheritdoc/>
        public override void Highlight(Color highlightColor)
        {
            Material materialCopy = new Material(highlightMaterial);
            materialCopy.color = highlightColor;
            
            Highlighter.StartHighlighting(SceneObject.UniqueName, materialCopy, true);
            EmitHighlightEvent();
        }
    }
}
