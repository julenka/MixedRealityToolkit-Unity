using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using System;

/// <summary>
/// Attach this Monobehaviour to any object you want to act like a fake input source
/// When the Monobehaviour is enabled, it tell MRTK that a fake input source has been detected 
/// The position and rotation of the grab, poke, and far pointer will all match the object attached to
/// You can simulate the "select" or "grab" gestures being pressed using the isSelectPressed variable.
/// </summary>
public class FakeInputSource : MonoBehaviour
{


    #region FakeInputSource
    public bool isSelectPressed;
    private TestInputDeviceManager deviceManager;
    public Handedness handedness = Handedness.Right;
    public SupportedControllerType controllerType = SupportedControllerType.ArticulatedHand;

    void Awake()
    {
        // Generate a poke pointer
        deviceManager = new TestInputDeviceManager(CoreServices.InputSystem, "TestDeviceManager", 1, null);
        deviceManager.Initialize();
    }

    void OnEnable()
    {
        InputSourceType ist = InputSourceType.Hand;
        switch (controllerType)
        {
            case SupportedControllerType.ArticulatedHand:
            case SupportedControllerType.GGVHand:
                ist = InputSourceType.Hand;
                break;
            case SupportedControllerType.Mouse:
            case SupportedControllerType.TouchScreen:
                ist = InputSourceType.Other;
                break;
            default:
                ist = InputSourceType.Controller;
                break;
        }
        deviceManager.OnSourceDetected(handedness, controllerType, ist);
    }

    void OnDisable()
    {
        deviceManager.OnSourceLost();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isSelectPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            isSelectPressed = false;
        }
        deviceManager.Update(transform, transform, transform, isSelectPressed);
    }
    #endregion

    #region Helper Classes 
    class TestController : BaseController
    {
        public TestController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        public override void SetupDefaultInteractions()
        {
            var inputActions = MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions;
            MixedRealityInteractionMapping[] interactions = new[]
            {
                new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, Array.Find(inputActions, x => x.Description.Equals("Pointer Pose"))),
                new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, Array.Find(inputActions, x => x.Description.Equals("Grip Pose"))),
                new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, Array.Find(inputActions, x => x.Description.Equals("Select"))),
                new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, Array.Find(inputActions, x => x.Description.Equals("Grip Press"))),
                new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger, Array.Find(inputActions, x => x.Description.Equals("Index Finger Pose")))
            };

            AssignControllerMappings(interactions);
        }

    }

    class TestInputDeviceManager : BaseInputDeviceManager
    {
        public IMixedRealityInputSource inputSource;
        private TestController controller;
        IMixedRealityPointer[] pointers;
        public TestInputDeviceManager(
            IMixedRealityInputSystem inputSystem, 
            string name, 
            uint priority,
            BaseMixedRealityProfile profile) : base(inputSystem, name, priority, profile)
        {
        }

        public void SimulateArticulatedHandDetected(Handedness h)
        {
            OnSourceDetected(h, SupportedControllerType.ArticulatedHand, InputSourceType.Hand);
        }
        public void SimulateOpenVRControllerDetected(Handedness h)
        {
            OnSourceDetected(h, SupportedControllerType.GenericOpenVR, InputSourceType.Controller);
        }

        public void OnSourceDetected(Handedness handedness, SupportedControllerType controllerType, InputSourceType inputType)
        {
            // Request pointers
            pointers = RequestPointers(controllerType, handedness);
            inputSource = new BaseGenericInputSource("SimulateSelect", pointers, inputType);

            // Create a  controller
            // TODO: can we just do this in the constructor once and change its tracking state?
            controller = new TestController(TrackingState.Tracked, handedness, inputSource);


            foreach (var p in pointers)
            {
                p.Controller = controller;
            }

            CoreServices.InputSystem.RaiseSourceDetected(inputSource, controller);
        }

        public void OnSourceLost()
        {
            if (CoreServices.InputSystem != null)
            {
                CoreServices.InputSystem.RaiseSourceLost(inputSource);
            }
            foreach(var pointer in pointers)
            {
                if (pointer != null && (MonoBehaviour)pointer != null && ((MonoBehaviour)pointer).gameObject != null)
                {
                    GameObject.Destroy(((MonoBehaviour)pointer).gameObject);
                }
            }
            controller = null;
            pointers = null;
        }

        public void Update(Transform gripTransform, Transform rayTransform, Transform fingertipTransform, bool isSelectPressed)
        {

            var gripPose = new MixedRealityPose(gripTransform.position, gripTransform.rotation);
            var pointingPose = new MixedRealityPose(rayTransform.position, rayTransform.rotation);
            var indexPose = new MixedRealityPose(fingertipTransform.position, fingertipTransform.rotation);
            for (int i = 0; i < controller.Interactions?.Length; i++)
            {
                switch (controller.Interactions[i].InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        controller.Interactions[i].PoseData = pointingPose;
                        CoreServices.InputSystem.RaisePoseInputChanged(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction, pointingPose);
                        break;
                    case DeviceInputType.SpatialGrip:
                        controller.Interactions[i].PoseData = gripPose;
                        CoreServices.InputSystem.RaisePoseInputChanged(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction, gripPose);
                        break;
                    case DeviceInputType.Select:
                        controller.Interactions[i].BoolData = isSelectPressed;
                        if (controller.Interactions[i].Changed)
                        {
                            if (isSelectPressed)
                            {
                                CoreServices.InputSystem.RaiseOnInputDown(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem.RaiseOnInputUp(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction);

                            }
                        }
                        break;
                    case DeviceInputType.TriggerPress:
                        controller.Interactions[i].BoolData = isSelectPressed;
                        if (controller.Interactions[i].Changed)
                        {
                            if (isSelectPressed)
                            {
                                CoreServices.InputSystem.RaiseOnInputDown(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem.RaiseOnInputUp(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                    case DeviceInputType.IndexFinger:
                        controller.Interactions[i].PoseData = indexPose;
                        CoreServices.InputSystem.RaisePoseInputChanged(inputSource, controller.ControllerHandedness, controller.Interactions[i].MixedRealityInputAction, indexPose);
                        break;

                }
            }
        }
    }
    #endregion
}
