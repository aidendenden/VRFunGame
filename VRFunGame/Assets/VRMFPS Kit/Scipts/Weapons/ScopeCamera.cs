using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Makes sure the scope camera is only rendering when held,
    /// and makes sure no more than one camera can render at the same time
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ScopeCamera : MonoBehaviour
    {
        private static ScopeCamera currentRenderingCamera;

        private Camera _camera;
        private XRGrabInteractable _interactable;

        // Update is called once per frame
        void Update()
        {
            if (!_interactable.isSelected)
                TryStopRendering();
            else
                TryStartRendering();

            _camera.enabled = currentRenderingCamera == this;
        }

        private void TryStartRendering()
        {
            if (currentRenderingCamera == this)
                return;
            if (currentRenderingCamera != null)
            {
                Debug.LogWarning("Two scope cameras are trying to render at the same time");
                return;
            }

            currentRenderingCamera = this;
        }

        private void TryStopRendering()
        {
            if (currentRenderingCamera != this)
                return;

            currentRenderingCamera = null;
        }

        private void OnDestroy()
        {
            if (currentRenderingCamera == this)
            {
                TryStopRendering();
                Debug.Log("Current rendering scope camera was destroyed, decommissioning...");
            }
        }

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _interactable = GetComponentInParent<XRGrabInteractable>();
        }
    }
}
