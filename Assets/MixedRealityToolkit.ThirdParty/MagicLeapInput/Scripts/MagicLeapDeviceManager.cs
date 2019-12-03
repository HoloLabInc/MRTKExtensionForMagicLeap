using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace HoloLab.MixedReality.Toolkit.MagicLeapInput
{
    /// <summary>
    /// Manages Magic Leap Device
    /// </summary>
    [MixedRealityDataProvider(
        typeof(IMixedRealityInputSystem),
        SupportedPlatforms.Lumin | SupportedPlatforms.WindowsEditor | SupportedPlatforms.MacEditor | SupportedPlatforms.LinuxEditor,
        "Magic Leap Device Manager")]
    public class MagicLeapDeviceManager : BaseInputDeviceManager, IMixedRealityCapabilityCheck
    {
        protected static readonly Dictionary<byte, MagicLeapController> ActiveControllers = new Dictionary<byte, MagicLeapController>();
        private List<MLInputController> mlInputControllers = new List<MLInputController>();

        private bool mlInputStarted = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the data provider.</param>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapDeviceManager(
            IMixedRealityServiceRegistrar registrar,
            IMixedRealityInputSystem inputSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile profile = null) : base(registrar, inputSystem, name, priority, profile) {
        }

        public override void Update() {
            // MLInput.Start() should be called after Start() in MonoBehaviour is called
            if (!mlInputStarted)
            {
                StartMLInput();
                mlInputStarted = true;
            }

            foreach (var mlInputController in mlInputControllers)
            {
                if (ActiveControllers.TryGetValue(mlInputController.Id, out MagicLeapController controller))
                {
                    controller.UpdateController(mlInputController);
                }
            }
        }
       
        public override void Disable()
        {
            StopMLInput();

            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

            foreach (var genericJoystick in ActiveControllers)
            {
                if (genericJoystick.Value != null)
                {
                    inputSystem?.RaiseSourceLost(genericJoystick.Value.InputSource, genericJoystick.Value);
                }
            }

            ActiveControllers.Clear();
        }

        public override IMixedRealityController[] GetActiveControllers()
        {
            return ActiveControllers.Values.ToArray<IMixedRealityController>();
        }

        /// <inheritdoc />
        public bool CheckCapability(MixedRealityCapability capability)
        {
            // The MagicLeap platform supports motion controllers.
            return (capability == MixedRealityCapability.MotionController);
        }

        private void StartMLInput()
        {
            bool requestCFUID = true;

            if (!MLInput.IsStarted)
            {
                MLInputConfiguration config = new MLInputConfiguration(MLInputConfiguration.DEFAULT_TRIGGER_DOWN_THRESHOLD,
                                                            MLInputConfiguration.DEFAULT_TRIGGER_UP_THRESHOLD,
                                                            requestCFUID);
                MLResult result = MLInput.Start(config);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: ControllerConnectionHandler failed starting MLInput, disabling script. Reason: {0}", result);
                    return;
                }
                else
                {
                    // When using SDK 0.21.0+Unity2019.1.x
                    // controllers might be already connected at this frame.
                    // In that case the callback is never called.
                    // That's why try to find connected controllers.

                    var controller = MLInput.GetController(MLInput.Hand.Right) ?? MLInput.GetController(MLInput.Hand.Left);

                    if (controller != null)
                    {
                        HandleOnControllerConnected(controller.Id);
                    }
                }
            }

            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;
            MLInput.OnControllerButtonUp += HandleOnButtonUp;
            MLInput.OnControllerButtonDown += HandleOnButtonDown;
        }

        private void StopMLInput()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnControllerButtonUp -= HandleOnButtonUp;
                MLInput.OnControllerButtonDown -= HandleOnButtonDown;

                MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;
                MLInput.OnControllerConnected -= HandleOnControllerConnected;

                MLInput.Stop();
            }

            mlInputControllers.Clear();
        }

        private void HandleOnControllerConnected(byte controllerId)
        {
            MLInputController controller = MLInput.GetController(controllerId);

            if (mlInputControllers.Exists((device) => device.Id == controllerId))
            {
                Debug.LogWarning(string.Format("Connected controller with id {0} already connected.", controllerId));
                return;
            }

            mlInputControllers.Add(controller);

            // Generate MRTK Controller
            Handedness controllingHand;
            if (controller.Type == MLInputControllerType.Control && controller.Hand == MLInput.Hand.Left)
            {
                controllingHand = Handedness.Left;
            }
            else if (controller.Type == MLInputControllerType.Control && controller.Hand == MLInput.Hand.Right)
            {
                controllingHand = Handedness.Right;
            }
            else
            {
                controllingHand = Handedness.Other;
            }

            var currentControllerType = SupportedControllerType.GenericOpenVR;
            var controllerType = typeof(MagicLeapController);
            var pointers = RequestPointers(currentControllerType, controllingHand);

            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

            var inputSource = inputSystem?.RequestNewGenericInputSource($"{currentControllerType} Controller {controllingHand}", pointers, InputSourceType.Controller);
            var detectedController = new MagicLeapController(TrackingState.Tracked, controllingHand, inputSource);

            detectedController.SetupConfiguration(controllerType);

            for (int i = 0; i < detectedController.InputSource?.Pointers?.Length; i++)
            {
                detectedController.InputSource.Pointers[i].Controller = detectedController;
            }
            ActiveControllers.Add(controllerId, detectedController);
            inputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);
        }

        private void HandleOnControllerDisconnected(byte controllerId)
        {
            mlInputControllers.RemoveAll((device) => device.Id == controllerId);

            MagicLeapController controller;
            if(ActiveControllers.TryGetValue(controllerId, out controller))
            {
                IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

                inputSystem?.RaiseSourceLost(controller.InputSource, controller);

                foreach (IMixedRealityPointer pointer in controller.InputSource.Pointers)
                {
                    if (pointer != null)
                    {
                        pointer.Controller = null;
                    }
                }

                ActiveControllers.Remove(controllerId);
            }
        }

        private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
        {
            if (ActiveControllers.TryGetValue(controllerId, out MagicLeapController controller))
            {
                controller.OnButtonDown(button);
            }
        }

        private void HandleOnButtonUp(byte controllerId, MLInputControllerButton button)
        {
            if (ActiveControllers.TryGetValue(controllerId, out MagicLeapController controller))
            {
                controller.OnButtonUp(button);
            }
        }
    }
}
