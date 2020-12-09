using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Innoactive.Creator.XRInteraction
{
    /// <summary>
    /// Use this class to map input actions to each controller state (mode)
    /// and set up the transitions between controller states (modes).
    /// </summary>
    [AddComponentMenu("XR/Action Based Controller Manager")]
    [DefaultExecutionOrder(KControllerManagerUpdateOrder)]
    public class ActionBasedControllerManager : MonoBehaviour
    {
        // Slightly after the default, so that any actions such as release or grab can be processed *before* we switch controllers.
        public const int KControllerManagerUpdateOrder = 10;

        public enum StateId
        {
            None,
            Select,
            Teleport,
            Interact,
            UI
        }

        [Serializable]
        public class StateEnterEvent : UnityEvent<StateId>
        {
        }

        [Serializable]
        public class StateUpdateEvent : UnityEvent
        {
        }

        [Serializable]
        public class StateExitEvent : UnityEvent<StateId>
        {
        }

        /// <summary>
        /// Use this class to create a controller state and set up its enter, update, and exit events.
        /// </summary>
        [Serializable]
        public class ControllerState
        {
            [SerializeField]
            [Tooltip("Sets the controller state to be active. " +
                     "For the default states, setting this value to true will automatically update their StateUpdateEvent.")]
            private bool enabled;

            /// <summary>
            /// Sets the controller state to be active.
            /// For the default states, setting this value to true will automatically update their <see cref="StateUpdateEvent"/>.
            /// </summary>
            public bool Enabled
            {
                get => enabled;
                set => enabled = value;
            }

            [SerializeField] 
            [HideInInspector]
            private StateId id;

            /// <summary>
            /// Sets the identifier of the <see cref="ControllerState"/> from all the optional Controller States that the <see cref="ActionBasedControllerManager"/> holds.
            /// </summary>
            public StateId Id
            {
                get => id;
                set => id = value;
            }

            [SerializeField] 
            private StateEnterEvent onEnter = new StateEnterEvent();

            /// <summary>
            /// The <see cref="StateEnterEvent"/> that will be invoked when entering the controller state.
            /// </summary>
            public StateEnterEvent OnEnter
            {
                get => onEnter;
                set => onEnter = value;
            }

            [SerializeField] 
            private StateUpdateEvent onUpdate = new StateUpdateEvent();

            /// <summary>
            /// The <see cref="StateUpdateEvent"/> that will be invoked when updating the controller state.
            /// </summary>
            public StateUpdateEvent OnUpdate
            {
                get => onUpdate;
                set => onUpdate = value;
            }

            [SerializeField] 
            private StateExitEvent onExit = new StateExitEvent();

            /// <summary>
            /// The <see cref="StateExitEvent"/> that will be invoked when exiting the controller state.
            /// </summary>
            public StateExitEvent OnExit
            {
                get => onExit;
                set => onExit = value;
            }

            public ControllerState(StateId defaultId = StateId.None) => this.Id = defaultId;
        }

        [Space]
        [Header("Controller GameObjects")]
        [SerializeField]
        [Tooltip("The base controller GameObject, used for changing default settings on its components during state transitions.")]
        private GameObject BaseControllerGameObject;

        /// <summary>
        /// The base controller <see cref="GameObject"/>, used for changing default settings on its components during state transitions.
        /// </summary>
        public GameObject baseControllerGameObject
        {
            get => BaseControllerGameObject;
            set => BaseControllerGameObject = value;
        }

        [SerializeField]
        [Tooltip("The teleport controller GameObject, used for changing default settings on its components during state transitions.")]
        GameObject TeleportControllerGameObject;

        /// <summary>
        /// The teleport controller <see cref="GameObject"/>, used for changing default settings on its components during state transitions.
        /// </summary>
        public GameObject teleportControllerGameObject
        {
            get => TeleportControllerGameObject;
            set => TeleportControllerGameObject = value;
        }
        
        [SerializeField]
        [Tooltip("The UI controller GameObject, used for changing default settings on its components during state transitions.")]
        GameObject UiControllerGameObject;

        /// <summary>
        /// The UI controller <see cref="GameObject"/>, used for changing default settings on its components during state transitions.
        /// </summary>
        public GameObject uiControllerGameObject
        {
            get => UiControllerGameObject;
            set => UiControllerGameObject = value;
        }

        [Space]
        [Header("Controller Actions")]

        // State transition actions
        [SerializeField]
        [Tooltip("The reference to the action of activating the teleport mode for this controller.")]
        InputActionReference TeleportModeActivate;

        /// <summary>
        /// The reference to the action of activating the teleport mode for this controller."
        /// </summary>
        public InputActionReference teleportModeActivate
        {
            get => TeleportModeActivate;
            set => TeleportModeActivate = value;
        }

        [SerializeField]
        [Tooltip("The reference to the action of canceling the teleport mode for this controller.")]
        InputActionReference TeleportModeCancel;

        /// <summary>
        /// The reference to the action of canceling the teleport mode for this controller."
        /// </summary>
        public InputActionReference teleportModeCancel
        {
            get => TeleportModeCancel;
            set => TeleportModeCancel = value;
        }
        
        [SerializeField]
        [Tooltip("The reference to the action of activating the UI mode for this controller.")]
        InputActionReference UiModeActivate;

        /// <summary>
        /// The reference to the action of activating the teleport mode for this controller."
        /// </summary>
        public InputActionReference uiModeActivate
        {
            get => UiModeActivate;
            set => UiModeActivate = value;
        }

        // Character movement actions
        [SerializeField]
        [Tooltip("The reference to the action of turning the XR rig with this controller.")]
        InputActionReference Turn;

        /// <summary>
        /// The reference to the action of turning the XR rig with this controller.
        /// </summary>
        public InputActionReference turn
        {
            get => Turn;
            set => Turn = value;
        }

        [SerializeField]
        [Tooltip("The reference to the action of moving the XR rig with this controller.")]
        InputActionReference Move;

        /// <summary>
        /// The reference to the action of moving the XR rig with this controller.
        /// </summary>
        public InputActionReference move
        {
            get => Move;
            set => Move = value;
        }

        // Object control actions
        [SerializeField]
        [Tooltip("The reference to the action of translating the selected object of this controller.")]
        InputActionReference TranslateAnchor;

        /// <summary>
        /// The reference to the action of translating the selected object of this controller.
        /// </summary>
        public InputActionReference translateAnchor
        {
            get => TranslateAnchor;
            set => TranslateAnchor = value;
        }

        [SerializeField, FormerlySerializedAs("m_RotateObject")]
        [Tooltip("The reference to the action of rotating the selected object of this controller.")]
        InputActionReference RotateAnchor;

        /// <summary>
        /// The reference to the action of rotating the selected object of this controller.
        /// </summary>
        public InputActionReference rotateAnchor
        {
            get => RotateAnchor;
            set => RotateAnchor = value;
        }

        [Space]
        [Header("Default States")]

#pragma warning disable IDE0044 // Add readonly modifier -- readonly fields cannot be serialized by Unity
        [SerializeField]
        [Tooltip("The default Select state and events for the controller.")]
        ControllerState SelectState = new ControllerState(StateId.Select);

        /// <summary>
        /// (Read Only) The default Select state.
        /// </summary>
        public ControllerState selectState => SelectState;

        [SerializeField]
        [Tooltip("The default Teleport state and events for the controller.")]
        ControllerState TeleportState = new ControllerState(StateId.Teleport);

        /// <summary>
        /// (Read Only) The default Teleport state.
        /// </summary>
        public ControllerState teleportState => TeleportState;
        
        [SerializeField]
        [Tooltip("The default UI state and events for the controller.")]
        ControllerState UiState = new ControllerState(StateId.UI);
        
        /// <summary>
        /// (Read Only) The default UI state.
        /// </summary>
        public ControllerState uiState => UiState;

        [SerializeField]
        [Tooltip("The default Interact state and events for the controller.")]
        ControllerState InteractState = new ControllerState(StateId.Interact);

        /// <summary>
        /// (Read Only) The default Interact state.
        /// </summary>
        public ControllerState interactState => InteractState;
#pragma warning restore IDE0044

        // The list to store and run the default states
        private readonly List<ControllerState> defaultStates = new List<ControllerState>();

        // Components of the controller to switch on and off for different states
        XRBaseController BaseController;
        XRBaseInteractor BaseInteractor;
        XRInteractorLineVisual BaseLineVisual;

        XRBaseController TeleportController;
        XRBaseInteractor TeleportInteractor;
        XRInteractorLineVisual TeleportLineVisual;
        
        XRBaseController UiController;
        XRBaseInteractor UiInteractor;
        XRInteractorLineVisual UiLineVisual;

        protected void OnEnable()
        {
            FindBaseControllerComponents();
            FindTeleportControllerComponents();
            FindUiControllerComponents();

            // Add default state events.
            SelectState.OnEnter.AddListener(OnEnterSelectState);
            SelectState.OnUpdate.AddListener(OnUpdateSelectState);
            SelectState.OnExit.AddListener(OnExitSelectState);

            TeleportState.OnEnter.AddListener(OnEnterTeleportState);
            TeleportState.OnUpdate.AddListener(OnUpdateTeleportState);
            TeleportState.OnExit.AddListener(OnExitTeleportState);
            
            UiState.OnEnter.AddListener(OnEnterUiState);
            UiState.OnUpdate.AddListener(OnUpdateUiState);
            UiState.OnExit.AddListener(OnExitUiState);

            InteractState.OnEnter.AddListener(OnEnterInteractState);
            InteractState.OnUpdate.AddListener(OnUpdateInteractState);
            InteractState.OnExit.AddListener(OnExitInteractState);
        }

        protected void OnDisable()
        {
            // Remove default state events.
            SelectState.OnEnter.RemoveListener(OnEnterSelectState);
            SelectState.OnUpdate.RemoveListener(OnUpdateSelectState);
            SelectState.OnExit.RemoveListener(OnExitSelectState);

            TeleportState.OnEnter.RemoveListener(OnEnterTeleportState);
            TeleportState.OnUpdate.RemoveListener(OnUpdateTeleportState);
            TeleportState.OnExit.RemoveListener(OnExitTeleportState);
            
            UiState.OnEnter.RemoveListener(OnEnterUiState);
            UiState.OnUpdate.RemoveListener(OnUpdateUiState);
            UiState.OnExit.RemoveListener(OnExitUiState);

            InteractState.OnEnter.RemoveListener(OnEnterInteractState);
            InteractState.OnUpdate.RemoveListener(OnUpdateInteractState);
            InteractState.OnExit.RemoveListener(OnExitInteractState);
        }

        // Start is called before the first frame update
        protected void Start()
        {
            // Add states to the list
            defaultStates.Add(SelectState);
            defaultStates.Add(TeleportState);
            defaultStates.Add(UiState);
            defaultStates.Add(InteractState);

            // Initialize to start in m_SelectState
            TransitionState(null, SelectState);
        }

        // Update is called once per frame
        protected void Update()
        {
            foreach (ControllerState state in defaultStates)
            {
                if (state.Enabled)
                {
                    state.OnUpdate.Invoke();
                    return;
                }
            }
        }

        void TransitionState(ControllerState fromState, ControllerState toState)
        {
            if (fromState != null)
            {
                fromState.Enabled = false;
                fromState.OnExit.Invoke(toState?.Id ?? StateId.None);
            }

            if (toState != null)
            {
                toState.OnEnter.Invoke(fromState?.Id ?? StateId.None);
                toState.Enabled = true;
            }
        }

        void FindBaseControllerComponents()
        {
            if (BaseControllerGameObject == null)
            {
                Debug.LogWarning("Missing reference to Base Controller GameObject.", this);
                return;
            }

            if (BaseController == null)
            {
                BaseController = BaseControllerGameObject.GetComponent<XRBaseController>();
                if (BaseController == null)
                {
                    Debug.LogWarning($"Cannot find any {nameof(XRBaseController)} component on the Base Controller GameObject.", this);
                }
            }

            if (BaseInteractor == null)
            {
                BaseInteractor = BaseControllerGameObject.GetComponent<XRBaseInteractor>();
                if (BaseInteractor == null)
                {
                    Debug.LogWarning($"Cannot find any {nameof(XRBaseInteractor)} component on the Base Controller GameObject.", this);
                }
            }

            // Only check the line visual component for RayInteractor, since DirectInteractor does not use the line visual component
            if (BaseInteractor is XRRayInteractor && BaseLineVisual == null)
            {
                BaseLineVisual = BaseControllerGameObject.GetComponent<XRInteractorLineVisual>();
                if (BaseLineVisual == null)
                {
                    Debug.LogWarning($"Cannot find any {nameof(XRInteractorLineVisual)} component on the Base Controller GameObject.", this);
                }
            }
        }

        void FindTeleportControllerComponents()
        {
            if (TeleportControllerGameObject == null)
            {
                Debug.LogWarning("Missing reference to the Teleport Controller GameObject.", this);
                return;
            }

            if (TeleportController == null)
            {
                TeleportController = TeleportControllerGameObject.GetComponent<XRBaseController>();
                if (TeleportController == null)
                {
                    Debug.LogWarning($"Cannot find {nameof(XRBaseController)} component on the Teleport Controller GameObject.", this);
                }
            }

            if (TeleportLineVisual == null)
            {
                TeleportLineVisual = TeleportControllerGameObject.GetComponent<XRInteractorLineVisual>();
                if (TeleportLineVisual == null)
                {
                    Debug.LogWarning($"Cannot find {nameof(XRInteractorLineVisual)} component on the Teleport Controller GameObject.", this);
                }
            }

            if (TeleportInteractor == null)
            {
                TeleportInteractor = TeleportControllerGameObject.GetComponent<XRRayInteractor>();
                if (TeleportInteractor == null)
                {
                    Debug.LogWarning($"Cannot find {nameof(XRRayInteractor)} component on the Teleport Controller GameObject.", this);
                }
            }
        }
        
        void FindUiControllerComponents()
        {
            if (UiControllerGameObject == null)
            {
                Debug.LogWarning("Missing reference to the UI Controller GameObject.", this);
                return;
            }

            if (UiController == null)
            {
                UiController = UiControllerGameObject.GetComponent<XRBaseController>();
                if (UiController == null)
                {
                    Debug.LogWarning($"Cannot find {nameof(XRBaseController)} component on the UI Controller GameObject.", this);
                }
            }

            if (UiLineVisual == null)
            {
                UiLineVisual = UiControllerGameObject.GetComponent<XRInteractorLineVisual>();
                if (UiLineVisual == null)
                {
                    Debug.LogWarning($"Cannot find {nameof(XRInteractorLineVisual)} component on the UI Controller GameObject.", this);
                }
            }

            if (UiInteractor == null)
            {
                UiInteractor = UiControllerGameObject.GetComponent<XRRayInteractor>();
                if (UiInteractor == null)
                {
                    Debug.LogWarning($"Cannot find {nameof(XRRayInteractor)} component on the UI Controller GameObject.", this);
                }
            }
        }

        /// <summary>
        /// Find and configure the components on the base controller.
        /// </summary>
        /// <param name="enable"> Set it true to enable the base controller, false to disable it. </param>
        void SetBaseController(bool enable)
        {
            FindBaseControllerComponents();

            if (BaseController != null)
            {
                BaseController.enableInputActions = enable;
            }

            if (BaseInteractor != null)
            {
                BaseInteractor.enabled = enable;
            }

            if (BaseInteractor is XRRayInteractor && BaseLineVisual != null)
            {
                BaseLineVisual.enabled = enable;
            }
        }

        /// <summary>
        /// Find and configure the components on the teleport controller.
        /// </summary>
        /// <param name="enable"> Set it true to enable the teleport controller, false to disable it. </param>
        void SetTeleportController(bool enable)
        {
            FindTeleportControllerComponents();

            if (TeleportLineVisual != null)
            {
                TeleportLineVisual.enabled = enable;
            }

            if (TeleportController != null)
            {
                TeleportController.enableInputActions = enable;
            }

            if (TeleportInteractor != null)
            {
                TeleportInteractor.enabled = enable;
            }
        }
        
        /// <summary>
        /// Find and configure the components on the UI controller.
        /// </summary>
        /// <param name="enable"> Set it true to enable the UI controller, false to disable it. </param>
        void SetUiController(bool enable)
        {
            FindUiControllerComponents();

            if (UiLineVisual != null)
            {
                UiLineVisual.enabled = enable;
            }

            if (UiController != null)
            {
                UiController.enableInputActions = enable;
            }

            if (UiInteractor != null)
            {
                UiInteractor.enabled = enable;
            }
        }

        void OnEnterSelectState(StateId previousStateId)
        {
            // Change controller and enable actions depending on the previous state
            switch (previousStateId)
            {
                case StateId.None:
                    // Enable transitions to Teleport state 
                    EnableAction(TeleportModeActivate);
                    EnableAction(TeleportModeCancel);
                    EnableAction(UiModeActivate);

                    // Enable turn and move actions
                    EnableAction(Turn);
                    EnableAction(Move);

                    // Enable base controller components
                    SetBaseController(true);
                    break;
                case StateId.Select:
                    break;
                case StateId.Teleport:
                    EnableAction(Turn);
                    EnableAction(Move);
                    SetBaseController(true);
                    break;
                case StateId.Interact:
                    EnableAction(Turn);
                    EnableAction(Move);
                    break;
                case StateId.UI:
                    EnableAction(Turn);
                    EnableAction(Move);
                    SetBaseController(true);
                    break;
                default:
                    Debug.Assert(false, $"Unhandled case when entering Select from {previousStateId}.");
                    break;
            }
        }

        void OnExitSelectState(StateId nextStateId)
        {
            // Change controller and disable actions depending on the next state
            switch (nextStateId)
            {
                case StateId.None:
                    break;
                case StateId.Select:
                    break;
                case StateId.Teleport:
                    DisableAction(Turn);
                    DisableAction(Move);
                    SetBaseController(false);
                    break;
                case StateId.Interact:
                    DisableAction(Turn);
                    DisableAction(Move);
                    break;
                case StateId.UI:
                    DisableAction(Turn);
                    DisableAction(Move);
                    SetBaseController(false);
                    break;
                default:
                    Debug.Assert(false, $"Unhandled case when exiting Select to {nextStateId}.");
                    break;
            }
        }

        void OnEnterTeleportState(StateId previousStateId) => SetTeleportController(true);

        void OnExitTeleportState(StateId nextStateId) => SetTeleportController(false);

        void OnEnterUiState(StateId previousStateId) => SetUiController(true);

        void OnExitUiState(StateId nextStateId) => SetUiController(false);

        void OnEnterInteractState(StateId previousStateId)
        {
            // Enable object control actions
            EnableAction(TranslateAnchor);
            EnableAction(RotateAnchor);
        }

        void OnExitInteractState(StateId nextStateId)
        {
            // Disable object control actions
            DisableAction(TranslateAnchor);
            DisableAction(RotateAnchor);
        }

        /// <summary>
        /// This method is automatically called each frame to handle initiating transitions out of the Select state.
        /// </summary>
        void OnUpdateSelectState()
        {
            // Transition from Select state to Teleport state when the user triggers the "Teleport Mode Activate" action but not the "Cancel Teleport" action
            InputAction teleportModeAction = GetInputAction(TeleportModeActivate);
            InputAction cancelTeleportModeAction = GetInputAction(TeleportModeCancel);

            bool triggerTeleportMode = teleportModeAction != null && teleportModeAction.triggered;
            bool cancelTeleport = cancelTeleportModeAction != null && cancelTeleportModeAction.triggered;

            if (triggerTeleportMode && !cancelTeleport)
            {
                TransitionState(SelectState, TeleportState);
                return;
            }
            
            // Transition from Select state to UI state when the user triggers the "UI Mode Activate" action
            InputAction uiModeAction = GetInputAction(UiModeActivate);
            
            if (uiModeAction != null && uiModeAction.triggered)
            {
                TransitionState(SelectState, UiState);
            }

            // Transition from Select state to Interact state when the interactor has a selectTarget
            FindBaseControllerComponents();

            if (BaseInteractor.selectTarget != null)
            {
                TransitionState(SelectState, InteractState);
            }
        }

        /// <summary>
        /// Updated every frame to handle the transition to m_SelectState state.
        /// </summary>
        void OnUpdateTeleportState()
        {
            // Transition from Teleport state to Select state when we release the Teleport trigger or cancel Teleport mode

            InputAction teleportModeAction = GetInputAction(TeleportModeActivate);
            InputAction cancelTeleportModeAction = GetInputAction(TeleportModeCancel);

            bool cancelTeleport = cancelTeleportModeAction != null && cancelTeleportModeAction.triggered;
            bool releasedTeleport = teleportModeAction != null && teleportModeAction.phase == InputActionPhase.Waiting;

            if (cancelTeleport || releasedTeleport)
            {
                TransitionState(TeleportState, SelectState);
            }
        }
        
        /// <summary>
        /// Updated every frame to handle the transition to m_SelectState state.
        /// </summary>
        void OnUpdateUiState()
        {
            // Transition from UI state to Select state when we release the UI trigger.
            InputAction uiModeAction = GetInputAction(UiModeActivate);

            if (uiModeAction != null && uiModeAction.phase == InputActionPhase.Waiting)
            {
                TransitionState(UiState, SelectState);
            }
        }

        void OnUpdateInteractState()
        {
            // Transition from Interact state to Select state when the base interactor no longer has a select target
            if (BaseInteractor.selectTarget == null)
            {
                TransitionState(InteractState, SelectState);
            }
        }

        static void EnableAction(InputActionReference actionReference)
        {
            InputAction action = GetInputAction(actionReference);
            if (action != null && !action.enabled)
            {
                action.Enable();
            }
        }

        static void DisableAction(InputActionReference actionReference)
        {
            InputAction action = GetInputAction(actionReference);
            if (action != null && action.enabled)
            {
                action.Disable();
            }
        }

        static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
    }
}
