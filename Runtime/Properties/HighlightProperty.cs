using UnityEngine;
using Innoactive.Creator.Core.Properties;

namespace Innoactive.Creator.XRInteraction.Properties
{
    /// <summary>
    /// Highlight property which enables an attached <see cref="InteractableObject"/>.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class HighlightProperty : BaseHighlightProperty
    {
        private InteractableHighlighter Highlighter;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            Highlighter = gameObject.GetComponent<InteractableHighlighter>();
        }

        /// <inheritdoc/>
        public override void Highlight(Color highlightColor)
        {
            Highlighter.StartHighlighting(SceneObject.UniqueName, highlightColor);
            EmitHighlightEvent();
        }

        /// <inheritdoc/>
        public override void Unhighlight()
        {
            Highlighter.StopHighlighting(SceneObject.UniqueName);
            EmitUnhighlightEvent();
        }
    }
}
