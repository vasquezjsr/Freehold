using Freehold.Needs;
using UnityEngine;

namespace Freehold.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerNeeds))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 4.5f;
        [SerializeField] float sprintSpeed = 7.5f;
        [SerializeField] float jumpHeight = 1.4f;
        [SerializeField] float gravity = -20f;
        [SerializeField] float turnSmoothTime = 0.08f;

        CharacterController _controller;
        PlayerNeeds _needs;
        Transform _cameraPivot;
        float _verticalVelocity;
        float _turnSmoothVelocity;

        public void BindCameraPivot(Transform cameraPivot)
        {
            _cameraPivot = cameraPivot;
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _needs = GetComponent<PlayerNeeds>();
        }

        void Update()
        {
            var grounded = _controller.isGrounded;
            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            var wantsSprint = Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0.01f;
            var canSprint = wantsSprint && _needs.TrySpendStamina(Time.deltaTime);
            if (!canSprint)
            {
                _needs.RegenStamina(Time.deltaTime);
            }

            var speed = canSprint ? sprintSpeed : walkSpeed;
            var moveDir = Vector3.zero;

            if (input.sqrMagnitude > 0.01f)
            {
                var yaw = _cameraPivot != null ? _cameraPivot.eulerAngles.y : transform.eulerAngles.y;
                var targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + yaw;
                var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }

            if (grounded && Input.GetButtonDown("Jump"))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _verticalVelocity += gravity * Time.deltaTime;
            var velocity = moveDir * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
