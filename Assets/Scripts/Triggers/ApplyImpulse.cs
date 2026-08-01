using UnityEngine;

public class ApplyImpulse : MonoBehaviour
{
    public bool resetVelocity = true;
    public float force = 1f;

    public void OnPlayerEnter(Rigidbody rigidbody, CharacterController characterController)
    {
        if (this.resetVelocity)
        {
            Vector3 localVelocity = this.transform.InverseTransformVector(rigidbody.linearVelocity);

            localVelocity.y = 0f;

            rigidbody.linearVelocity = this.transform.TransformVector(localVelocity);
        }

        rigidbody.AddForce(this.force * this.transform.up, ForceMode.Impulse);
    }
}
