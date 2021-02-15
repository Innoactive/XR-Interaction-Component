using UnityEngine;
using Innoactive.Creator.Core.Properties;
using Innoactive.Creator.Unity;

namespace Innoactive.Creator.XRInteraction.Properties
{
    /// <summary>
    /// Highlight property which enables an attached <see cref="InteractableObject"/>.
    /// </summary>
    public class HighlightProperty : BaseHighlightProperty
    {
        /// <summary>
        /// Returns the highlight color, if the object is currently highlighted.
        /// Returns null, otherwise.
        /// </summary>
        public Color? CurrentHighlightColor { get; protected set; }
            
        /// <summary>
        /// The <see cref="InteractableHighlighter"/> which is used to highlight the <see cref="Core.SceneObjects.TrainingSceneObject"/>.
        /// </summary>
        protected InteractableHighlighter Highlighter;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Highlighter == null)
            {
                Initialize();
            }
        }
        
        protected void Reset()
        {
            Initialize();
        }

        protected void Initialize()
        {
            InteractableObject ownInteractableObject = gameObject.GetComponent<InteractableObject>();

            // If gameObject was not interactable before, disable interactable functionality.
            if (ownInteractableObject == null)
            {
                Rigidbody ownRigidbody = gameObject.GetComponent<Rigidbody>();
                ownInteractableObject = gameObject.GetOrAddComponent<InteractableObject>();
                ownInteractableObject.IsGrabbable = false;
                ownInteractableObject.IsTouchable = false;
                ownInteractableObject.IsUsable = false;
                // If the gameObject had no rigidbody and thus was unaffected by physics, make it kinematic.
                if (ownRigidbody == null)
                {
                    gameObject.GetOrAddComponent<Rigidbody>().isKinematic = true;
                }
            }

            Highlighter = gameObject.GetOrAddComponent<InteractableHighlighter>();
        }

        /// <inheritdoc/>
        public override void Highlight(Color highlightColor)
        {
            CurrentHighlightColor = highlightColor;
            IsHighlighted = true;
            Highlighter.StartHighlighting(SceneObject.UniqueName, highlightColor);
            EmitHighlightEvent();
        }

        /// <inheritdoc/>
        public override void Unhighlight()
        {
            CurrentHighlightColor = null;
            IsHighlighted = false;
            Highlighter.StopHighlighting(SceneObject.UniqueName);
            EmitUnhighlightEvent();
        }
    }
}
