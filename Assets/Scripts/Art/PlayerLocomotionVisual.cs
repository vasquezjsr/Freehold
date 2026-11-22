using UnityEngine;

namespace Freehold.Art
{
    /// <summary>
    /// Procedural idle / walk / run / jump limb motion for the low-poly humanoid.
    /// No Animator Controller or FBX clips required.
    /// </summary>
    public class PlayerLocomotionVisual : MonoBehaviour
    {
        CharacterController _controller;

        Transform _hips;
        Transform _torso;
        Transform _head;
        Transform _leftThigh;
        Transform _leftShin;
        Transform _rightThigh;
        Transform _rightShin;
        Transform _leftUpper;
        Transform _leftLower;
        Transform _rightUpper;
        Transform _rightLower;

        float _phase;

        public void Configure(
            Transform hips,
            Transform torso,
            Transform head,
            Transform leftThigh,
            Transform leftShin,
            Transform rightThigh,
            Transform rightShin,
            Transform leftUpper,
            Transform leftLower,
            Transform rightUpper,
            Transform rightLower)
        {
            _hips = hips;
            _torso = torso;
            _head = head;
            _leftThigh = leftThigh;
            _leftShin = leftShin;
            _rightThigh = rightThigh;
            _rightShin = rightShin;
            _leftUpper = leftUpper;
            _leftLower = leftLower;
            _rightUpper = rightUpper;
            _rightLower = rightLower;
            _controller = GetComponent<CharacterController>();
        }

        void LateUpdate()
        {
            if (_hips == null || _controller == null)
            {
                return;
            }

            var horizontal = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            var speed = horizontal.magnitude;
            var grounded = _controller.isGrounded;
            var sprinting = speed > 5.5f;
            var moving = speed > 0.2f;

            var stepRate = sprinting ? 9.5f : moving ? 7f : 1.6f;
            _phase += Time.deltaTime * stepRate;

            if (!grounded)
            {
                ApplyJumpPose();
                return;
            }

            if (!moving)
            {
                ApplyIdlePose();
                return;
            }

            ApplyWalkRunPose(sprinting);
        }

        void ApplyIdlePose()
        {
            var breathe = Mathf.Sin(Time.time * 1.8f) * 2f;
            _hips.localRotation = Quaternion.identity;
            _torso.localRotation = Quaternion.Euler(breathe * 0.3f, 0f, 0f);
            _head.localRotation = Quaternion.Euler(-breathe * 0.2f, 0f, 0f);
            _leftThigh.localRotation = Quaternion.Euler(4f, 0f, 0f);
            _rightThigh.localRotation = Quaternion.Euler(-2f, 0f, 0f);
            _leftShin.localRotation = Quaternion.Euler(6f, 0f, 0f);
            _rightShin.localRotation = Quaternion.Euler(4f, 0f, 0f);
            _leftUpper.localRotation = Quaternion.Euler(8f, 0f, 12f);
            _rightUpper.localRotation = Quaternion.Euler(8f, 0f, -12f);
            _leftLower.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            _rightLower.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        }

        void ApplyWalkRunPose(bool sprinting)
        {
            var amp = sprinting ? 38f : 26f;
            var swing = Mathf.Sin(_phase) * amp;
            var swingOpp = -swing;
            var shinBend = Mathf.Max(0f, -Mathf.Sin(_phase)) * (sprinting ? 50f : 35f);
            var shinBendOpp = Mathf.Max(0f, Mathf.Sin(_phase)) * (sprinting ? 50f : 35f);

            _hips.localRotation = Quaternion.Euler(sprinting ? 8f : 3f, 0f, Mathf.Sin(_phase) * 2f);
            _torso.localRotation = Quaternion.Euler(sprinting ? 10f : 4f, Mathf.Sin(_phase) * 4f, 0f);
            _head.localRotation = Quaternion.Euler(sprinting ? -4f : 0f, 0f, 0f);

            _leftThigh.localRotation = Quaternion.Euler(swing, 0f, 0f);
            _rightThigh.localRotation = Quaternion.Euler(swingOpp, 0f, 0f);
            _leftShin.localRotation = Quaternion.Euler(shinBend, 0f, 0f);
            _rightShin.localRotation = Quaternion.Euler(shinBendOpp, 0f, 0f);

            _leftUpper.localRotation = Quaternion.Euler(swingOpp * 0.7f, 0f, 10f);
            _rightUpper.localRotation = Quaternion.Euler(swing * 0.7f, 0f, -10f);
            _leftLower.localRotation = Quaternion.Euler(-20f - Mathf.Abs(swingOpp) * 0.2f, 0f, 0f);
            _rightLower.localRotation = Quaternion.Euler(-20f - Mathf.Abs(swing) * 0.2f, 0f, 0f);
        }

        void ApplyJumpPose()
        {
            _hips.localRotation = Quaternion.identity;
            _torso.localRotation = Quaternion.Euler(-6f, 0f, 0f);
            _head.localRotation = Quaternion.Euler(8f, 0f, 0f);
            _leftThigh.localRotation = Quaternion.Euler(-18f, 0f, 8f);
            _rightThigh.localRotation = Quaternion.Euler(-10f, 0f, -8f);
            _leftShin.localRotation = Quaternion.Euler(35f, 0f, 0f);
            _rightShin.localRotation = Quaternion.Euler(28f, 0f, 0f);
            _leftUpper.localRotation = Quaternion.Euler(-40f, 0f, 25f);
            _rightUpper.localRotation = Quaternion.Euler(-35f, 0f, -25f);
            _leftLower.localRotation = Quaternion.Euler(-20f, 0f, 0f);
            _rightLower.localRotation = Quaternion.Euler(-20f, 0f, 0f);
        }
    }
}
