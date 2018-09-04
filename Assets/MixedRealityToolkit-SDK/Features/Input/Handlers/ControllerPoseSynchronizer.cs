﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Core.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using Microsoft.MixedReality.Toolkit.Core.EventDatum.Input;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem.Handlers;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.Input.Handlers
{
    /// <summary>
    /// Waits for a controller to be initialized, then synchronizes its transform position to a specified handedness.
    /// </summary>
    public class ControllerPoseSynchronizer : InputSystemGlobalListener, IMixedRealitySourcePoseHandler, IMixedRealityControllerPoseSynchronizer
    {
        #region IMixedRealityControllerPoseSynchronizer Implementation

        [SerializeField]
        [Tooltip("The handedness this controller should synchronize with.")]
        private Handedness handedness = Handedness.Left;

        /// <inheritdoc />
        public Handedness Handedness => handedness;

        [SerializeField]
        [Tooltip("Should this GameObject clean itself up when it's controller is lost?")]
        private bool destroyOnSourceLost = true;

        /// <inheritdoc />
        public bool DestroyOnSourceLost
        {
            get { return destroyOnSourceLost; }
            set { destroyOnSourceLost = value; }
        }

        /// <summary>
        /// Is the controller this Synchronizer is registered to currently tracked?
        /// </summary>
        public bool IsTracked { get; protected set; } = false;

        /// <summary>
        /// The current tracking state of the assigned <see cref="IMixedRealityController"/>
        /// </summary>
        protected TrackingState TrackingState = TrackingState.NotTracked;

        private IMixedRealityController controller;

        /// <inheritdoc />
        public virtual IMixedRealityController Controller
        {
            get { return controller; }
            set
            {
                handedness = value.ControllerHandedness;
                controller = value;
                gameObject.name = $"{handedness}_{gameObject.name}";
            }
        }

        [SerializeField]
        [Tooltip("Should the Transform's position be driven from the source pose or from input handler?")]
        private bool useSourcePoseData = true;

        /// <inheritdoc />
        public bool UseSourcePoseData
        {
            get { return useSourcePoseData; }
            set { useSourcePoseData = value; }
        }

        [SerializeField]
        [Tooltip("The input action that will drive the Transform's pose, position, or rotation.")]
        private MixedRealityInputAction poseAction = MixedRealityInputAction.None;

        /// <inheritdoc />
        public MixedRealityInputAction PoseAction
        {
            get { return poseAction; }
            set { poseAction = value; }
        }

        #endregion IMixedRealityControllerPoseSynchronizer Implementation

        #region IMixedRealitySourcePoseHandler Implementation

        /// <inheritdoc />
        public virtual void OnSourceDetected(SourceStateEventData eventData) { }

        /// <inheritdoc />
        public virtual void OnSourceLost(SourceStateEventData eventData)
        {
            if (Controller == null ||
                eventData.Controller == null ||
                eventData.Controller.InputSource.SourceId != Controller.InputSource.SourceId)
            {
                return;
            }

            if (eventData.Controller?.ControllerHandedness == Handedness)
            {
                IsTracked = false;
                TrackingState = TrackingState.NotTracked;

                if (destroyOnSourceLost)
                {
                    if (Application.isEditor)
                    {
                        DestroyImmediate(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnSourcePoseChanged(SourcePoseEventData eventData)
        {
            if (Controller == null ||
                eventData.Controller == null ||
                eventData.Controller.InputSource.SourceId != Controller.InputSource.SourceId)
            {
                return;
            }

            if (eventData.TrackingState != TrackingState)
            {
                IsTracked = eventData.TrackingState == TrackingState.Tracked;
                TrackingState = eventData.TrackingState;
            }

            if (UseSourcePoseData && TrackingState == TrackingState.Tracked)
            {
                transform.localPosition = eventData.MixedRealityPose.Position;
                transform.localRotation = eventData.MixedRealityPose.Rotation;
            }
        }

        #endregion IMixedRealitySourcePoseHandler Implementation

        #region IMixedRealityInputHandler Implementation

        public virtual void OnInputUp(InputEventData eventData) { }

        public virtual void OnInputDown(InputEventData eventData) { }

        public virtual void OnInputPressed(InputEventData<float> eventData) { }

        public virtual void OnPositionInputChanged(InputEventData<Vector2> eventData) { }

        #endregion  IMixedRealityInputHandler Implementation

        #region IMixedRealitySpatialInputHandler Implementation

        /// <inheritdoc />
        public virtual void OnPositionChanged(InputEventData<Vector3> eventData)
        {
            if (eventData.SourceId == Controller?.InputSource.SourceId)
            {
                if (!UseSourcePoseData &&
                    PoseAction == eventData.MixedRealityInputAction)
                {
                    IsTracked = true;
                    TrackingState = TrackingState.Tracked;
                    transform.localPosition = eventData.InputData;
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnRotationChanged(InputEventData<Quaternion> eventData)
        {
            if (eventData.SourceId == Controller?.InputSource.SourceId)
            {
                if (!UseSourcePoseData &&
                    PoseAction == eventData.MixedRealityInputAction)
                {
                    IsTracked = true;
                    TrackingState = TrackingState.Tracked;
                    transform.localRotation = eventData.InputData;
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnPoseInputChanged(InputEventData<MixedRealityPose> eventData)
        {
            if (eventData.SourceId == Controller?.InputSource.SourceId)
            {
                if (!UseSourcePoseData &&
                    PoseAction == eventData.MixedRealityInputAction)
                {
                    IsTracked = true;
                    TrackingState = TrackingState.Tracked;
                    transform.localPosition = eventData.InputData.Position;
                    transform.localRotation = eventData.InputData.Rotation;
                }
            }
        }

        #endregion IMixedRealitySpatialInputHandler Implementation 
    }
}