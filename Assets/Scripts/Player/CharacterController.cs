using System;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Rigidbody rigidbody;
    public CapsuleCollider capsuleCollider;
    public Transform head;

    [Space]
    public float mouseSensitivity = 1f;
    public float suspendMouseLook = 1f;
    public float movementSpeed = 5.0f;
    public float jumpForce = 1f;
    public float jumpCooldown = 0.25f;
    public float airborneAcceleration = 1f;
    public float wallRunMinimumSpeed = 1f;
    public float wallRunSnapForce = 1f;
    public float graplingHookCooldown = 10f;

    [Space]
    public LayerMask layerMask = ~0;
    public float groundCastDistance = 0.1f;
    public float movementCastDistance = 1f;
    public float movementCastHeightOffset = 0.1f;
    public float maxGroundAngle = 10f;
    public float maxWallAngle = 10f;

    private Vector2 lookInput;
    private Vector2 moveInput;
    private bool jumpInput;
    private bool graplingHookInput;

    private Vector3? previousMousePosition;
    private float lastJumpTime;
    private float lastGrapplingHookTime;

    private Vector3 Velocity => this.rigidbody.linearVelocity;
    private Vector3 GroundVelocity => this.rigidbody.linearVelocity.SetComponent(Axis.Y, 0);
    private Vector3 RestVelocity => this.GroundRigidbody != null ? this.GroundRigidbody.linearVelocity : Vector3.zero;
    private float GroundSpeed => this.Velocity.SetComponent(Axis.Y, 0).magnitude;
    private bool Grounded { get; set; }
    private bool Walled { get; set; }
    private Vector3 GroundNormal { get; set; }
    private Vector3 WallDriection { get; set; }
    private Rigidbody GroundRigidbody { get; set; }

    private float ungroundedUntil;

    public void Unground(float duration)
    {
        float ungroundedUntil = Time.time + duration;

        this.ungroundedUntil = Mathf.Max(this.ungroundedUntil, ungroundedUntil);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.GetInput();
        this.CollisionCheck();
        this.Look();
        this.Move();
    }

    private void GetInput()
    {
        Vector3 mouseDelta = Input.mousePositionDelta;
        this.lookInput = new Vector2(mouseDelta.x, -mouseDelta.y) * this.mouseSensitivity;

        this.moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        this.jumpInput = Input.GetKey(KeyCode.Space);
    }

    private void CollisionCheck()
    {
        Vector3 movementDirection = this.transform.rotation * new Vector3(this.moveInput.x, 0, this.moveInput.y).normalized;
                
        RaycastHit[] groundCast = this.CapsuleCast(this.transform.position, Quaternion.identity, Vector3.down, this.groundCastDistance, this.layerMask);

        Vector3 heightOffset = this.movementCastHeightOffset * Vector3.up;
        RaycastHit[] movementCast = this.CapsuleCast(this.transform.position + heightOffset, Quaternion.identity, movementDirection, this.movementCastDistance, this.layerMask);

        RaycastHit? groundHit = this.GetGroundHit(groundCast);
        RaycastHit? wallHit = this.GetWallHit(movementCast);

        this.GroundRigidbody = groundHit.HasValue ? groundHit.Value.rigidbody : null;
        this.GroundNormal = groundHit.HasValue ? groundHit.Value.normal : Vector3.up;
        this.WallDriection = wallHit.HasValue ? this.GroundDirection(wallHit.Value.point) : Vector3.zero;

        this.Grounded = Time.time > this.ungroundedUntil && groundHit != null;
        this.Walled = wallHit != null;
    }

    private Vector3 GroundDirection(Vector3 point)
    {
        Vector3 playerPosition = this.transform.position.SetComponent(Axis.Y, 0);
        Vector3 direction = point.SetComponent(Axis.Y, 0) - playerPosition;

        return direction.normalized;
    }

    private RaycastHit? GetGroundHit(RaycastHit[] raycastHits)
    {
        foreach (RaycastHit hit in raycastHits)
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < this.maxGroundAngle)
            {
                return hit;
            }
        }

        return null;
    }

    private RaycastHit? GetWallHit(RaycastHit[] movementCast)
    {
        float min = 90 - this.maxWallAngle;
        float max = 90 + this.maxWallAngle;

        foreach (RaycastHit hit in movementCast)
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (min < angle && angle < max)
            {
                return hit;
            }
        }

        return null;
    }

    private RaycastHit[] CapsuleCast(Vector3 position, Quaternion rotation, Vector3 direction, float distance = float.MaxValue, int layerMask = ~0, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        Vector3 localCenter = this.capsuleCollider.center;
        float radius = this.capsuleCollider.radius - distance / 2;
        float height = this.capsuleCollider.height;       ;
        int capsuleDirection = this.capsuleCollider.direction;

        Vector3 localUp = capsuleDirection switch
        {
            0 => Vector3.right,
            1 => Vector3.up,
            2 => Vector3.forward,
            _ => throw new NotImplementedException(),
        };

        Vector3 up = rotation * localUp;
        Vector3 center = position + rotation * localCenter;

        float halfHeight = (height - 2 * radius) / 2;

        Vector3 lower = center - halfHeight * up;
        Vector3 upper = center + halfHeight * up;

        return Physics.CapsuleCastAll(lower, upper, radius, direction, distance, layerMask, queryTriggerInteraction);
    }

    private void Look()
    {
        if (Time.time < this.suspendMouseLook)
        {
            return;
        }

        this.transform.Rotate(new Vector3(0, this.lookInput.x * Time.deltaTime, 0), Space.World);
        this.head.Rotate(new Vector3(this.lookInput.y * Time.deltaTime, 0, 0), Space.Self);
    }

    private void Move()
    {
        if (this.Grounded)
        {
            this.GroundedMovement();

            return;
        }

        if (this.Walled)
        {
            this.WallMovement();

            return;
        }

        this.AirborneMovement();
    }

    private void GroundedMovement()
    {
        Vector3 forward = Quaternion.Euler(0, this.transform.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right = this.transform.right;

        Vector3 movement = this.moveInput.y * forward + this.moveInput.x * right;
        Vector3 normalizedMovement = this.moveInput != Vector2.zero ? movement.normalized : Vector3.zero;
                 
        Vector3 targetVelocity = this.RestVelocity + normalizedMovement * this.movementSpeed;
        targetVelocity.y = this.Velocity.y;

        this.rigidbody.linearVelocity = targetVelocity;

        float time = Time.time;

        if (this.Grounded && this.jumpInput && time - this.lastJumpTime > this.jumpCooldown)
        {
            this.lastJumpTime = time;
            this.rigidbody.AddForce(Vector3.up * this.jumpForce, ForceMode.Impulse);
        }        
    }

    private void WallMovement()
    {
        if (this.GroundSpeed < this.wallRunMinimumSpeed)
        {
            return;
        }

        this.rigidbody.linearVelocity = this.rigidbody.linearVelocity.SetComponent(Axis.Y, 0);
        this.rigidbody.AddForce(this.WallDriection * this.wallRunSnapForce, ForceMode.Force);
    }

    private void AirborneMovement()
    {
        Vector3 forward = Quaternion.Euler(0, this.transform.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right = this.transform.right;

        Vector3 movement = this.moveInput.y * forward + this.moveInput.x * right;
        Vector3 normalizedMovement = this.moveInput != Vector2.zero ? movement.normalized : Vector3.zero;

        Vector3 acceleration = normalizedMovement * this.airborneAcceleration;

        Quaternion heading = Quaternion.Euler(0, this.transform.eulerAngles.y, 0);
        Quaternion inverseHeading = Quaternion.Inverse(heading);

        Vector3 velocity = this.rigidbody.linearVelocity;
        Vector3 targetVelocity = velocity + acceleration * Time.deltaTime;

        Vector3 localVelocity = inverseHeading * velocity;
        Vector3 localTargetVelocity = inverseHeading * targetVelocity;

        if (Mathf.Abs(localTargetVelocity.x) > this.movementSpeed)
        {
            localTargetVelocity.x = localVelocity.x;
        }

        if (Mathf.Abs(localTargetVelocity.z) > this.movementSpeed)
        {
            localTargetVelocity.z = localVelocity.z;
        }

        targetVelocity = heading * localTargetVelocity;

        this.rigidbody.linearVelocity = targetVelocity;
    }
}
