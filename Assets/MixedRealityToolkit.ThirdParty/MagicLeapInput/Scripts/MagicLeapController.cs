using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace HoloLab.MixedReality.Toolkit.MagicLeapInput
{
    [MixedRealityController(
        SupportedControllerType.GenericOpenVR,
        new[] { Handedness.Left, Handedness.Right })]
    public class MagicLeapController : BaseController
    {
        protected Vector3 CurrentControllerPosition = Vector3.zero;
        protected Quaternion CurrentControllerRotation = Quaternion.identity;
        protected MixedRealityPose CurrentControllerPose = MixedRealityPose.ZeroIdentity;

        protected bool triggerDown = false;
        protected float triggerUpThreshold = 0.15f;
        protected float triggerDownThreshold = 0.2f;

        public MagicLeapController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            // Define input mapping to default InputAction
            // https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Input/InputActions.html
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, new MixedRealityInputAction(4, "Pointer Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(1, "Select", AxisType.Digital, DeviceInputType.ButtonPress, new MixedRealityInputAction(1, "Select", AxisType.Digital)),
        };

        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }

        internal void UpdateController(MLInputController controller)
        {
            if (controller.Type == MLInputControllerType.Control)
            {
                CurrentControllerPosition = controller.Position;
                CurrentControllerRotation = controller.Orientation;

                // Update the interaction data source
                CurrentControllerPosition = MixedRealityPlayspace.TransformPoint(CurrentControllerPosition);
                CurrentControllerRotation = MixedRealityPlayspace.Rotation * CurrentControllerRotation;

                CurrentControllerPose.Position = CurrentControllerPosition;
                CurrentControllerPose.Rotation = CurrentControllerRotation;

                InputSystem?.RaiseSourcePoseChanged(InputSource, this, CurrentControllerPose);


                var interactionMapping = Interactions[0];
                interactionMapping.PoseData = CurrentControllerPose;
                if (interactionMapping.Changed)
                {
                    // Raise input system Event if it enabled
                    InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.PoseData);
                }

                // Fire Select event
                var trigger = controller.TriggerValue;
                if (triggerDown)
                {
                    if(trigger < triggerUpThreshold)
                    {
                        triggerDown = false;
                        RaiseOnSelectUp();
                    }
                }
                else
                {
                    if (trigger > triggerDownThreshold)
                    {
                        triggerDown = true;
                        RaiseOnSelectDown();
                    }
                }
            }
        }

        private void RaiseOnSelectDown()
        {
            InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[1].MixedRealityInputAction);
        }

        private void RaiseOnSelectUp()
        {
            InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[1].MixedRealityInputAction);
        }


        public void OnButtonDown(MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                // add bumper action
            }
        }

        public void OnButtonUp(MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.Bumper)
            {
                // add bumper action
            }
        }
    }
}
