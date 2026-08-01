using UnityEngine;

public class ApplyImpulse : MonoBehaviour
{
    public bool resetVerticalVelocity = false;
    public float lateralVelocityScale = 1.0f;
    public float ungroundDuration = -1.0f;
    public float force = 1f;

    public void OnPlayerEnter(Rigidbody rigidbody, CharacterController characterController)
    {
        Vector3 localVelocity = this.transform.InverseTransformVector(rigidbody.linearVelocity);

        localVelocity.y *= this.resetVerticalVelocity ? 0f : 1;

        float tempY = localVelocity.y;
        localVelocity *= this.lateralVelocityScale;
        localVelocity.y = tempY;

        rigidbody.linearVelocity = this.transform.TransformVector(localVelocity);

        rigidbody.AddForce(this.force * this.transform.up, ForceMode.Impulse);

        if (this.ungroundDuration > 0)
        {
            characterController.Unground(this.ungroundDuration);
        }
    }
}
