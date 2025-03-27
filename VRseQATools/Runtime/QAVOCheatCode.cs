using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRseQATools.Runtime
{
    /// <summary>
    /// Allows pitch control of an AudioSource using Oculus controller button combos.
    /// Pressing X + A decreases pitch, Y + B increases pitch. Haptic feedback is provided.
    /// </summary>
    public class QAVOCheatCode : MonoBehaviour
    {
        private AudioSource _audioSource;

        [SerializeField]
        [Tooltip("VO Speed Increment Amount")]
        private float _pitchIncreaseAmount = 0.3f;

        [SerializeField]
        [Tooltip("Minimum pitch value allowed.")]
        [Range(0.1f, 7f)]
        private float _minPitch = 0.1f;

        [SerializeField]
        [Tooltip("Maximum pitch value allowed.")]
        [Range(0.1f, 7f)]
        private float _maxPitch = 3f;

        private bool _increaseComboActivated = false;
        private bool _decreaseComboActivated = false;

        [SerializeField]
        [Tooltip("Time required to register the combo again.")]
        private float _comboCooldownDuration = 0.5f;
        private float _lastComboActivationTime = 0;

        /// <summary>
        /// Initializes the AudioSource reference.
        /// </summary>
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Checks for combo inputs each frame and modifies pitch accordingly.
        /// </summary>
        private void Update()
        {
            if (Time.time - _lastComboActivationTime < _comboCooldownDuration)
                return;

            bool xPressed = OVRInput.Get(OVRInput.Button.Three); // Button X
            bool yPressed = OVRInput.Get(OVRInput.Button.Four);  // Button Y
            bool aPressed = OVRInput.Get(OVRInput.Button.One);   // Button A
            bool bPressed = OVRInput.Get(OVRInput.Button.Two);   // Button B

            if (xPressed && aPressed && !_increaseComboActivated)
            {
                _lastComboActivationTime = Time.time;
                _increaseComboActivated = true;

                StartHaptics();
                DecreasePitch();
            }

            if (yPressed && bPressed && !_decreaseComboActivated)
            {
                _lastComboActivationTime = Time.time;
                _decreaseComboActivated = true;

                StartHaptics();
                IncreasePitch();
            }

            if (!xPressed || !aPressed)
            {
                _increaseComboActivated = false;
            }
            if (!yPressed || !bPressed)
            {
                _decreaseComboActivated = false;
            }
        }

        /// <summary>
        /// Increases the pitch of the AudioSource, clamped within min and max pitch values.
        /// </summary>
        private void IncreasePitch()
        {
            if (_audioSource != null)
            {
                _audioSource.pitch = Mathf.Clamp(_audioSource.pitch + _pitchIncreaseAmount, _minPitch, _maxPitch);
            }
        }

        /// <summary>
        /// Decreases the pitch of the AudioSource, clamped within min and max pitch values.
        /// </summary>
        private void DecreasePitch()
        {
            if (_audioSource != null)
            {
                _audioSource.pitch = Mathf.Clamp(_audioSource.pitch - _pitchIncreaseAmount, _minPitch, _maxPitch);
            }
        }

        /// <summary>
        /// Starts controller vibration on both left and right controllers.
        /// </summary>
        private void StartHaptics()
        {
            OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
            Invoke(nameof(StopHaptics), 0.2f);
        }

        /// <summary>
        /// Stops controller vibration on both left and right controllers.
        /// </summary>
        private void StopHaptics()
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }
}
