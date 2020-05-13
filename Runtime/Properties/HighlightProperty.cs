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
        /// <summary>
        /// Returns the highlight color, if the object is currently highlighted.
        /// Returns null, otherwise.
        /// </summary>
        public Color? CurrentHighlightColor { get; protected set; }
        
        private InteractableHighlighter Highlighter;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Highlighter == null)
            {
                Highlighter = gameObject.GetComponent<InteractableHighlighter>();
            }
        }

        /// <inheritdoc/>
        public override void Highlight(Color highlightColor)
        {
            CurrentHighlightColor = highlightColor;
            Highlighter.StartHighlighting(SceneObject.UniqueName, highlightColor);
            EmitHighlightEvent();
        }

        /// <inheritdoc/>
        public override void Unhighlight()
        {
            CurrentHighlightColor = null;
            Highlighter.StopHighlighting(SceneObject.UniqueName);
            EmitUnhighlightEvent();
        }
    }
}
