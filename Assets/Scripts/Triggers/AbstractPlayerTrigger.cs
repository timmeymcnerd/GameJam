using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class AbstractPlayerTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!this.IsPlayer(other, out Rigidbody rigidbody, out CharacterController characterController))
        {
            return;
        }

        this.OnEnter(other, rigidbody, characterController);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!this.IsPlayer(other, out Rigidbody rigidbody, out CharacterController characterController))
        {
            return;
        }

        this.OnExit(other, rigidbody, characterController);
    }

    protected abstract void OnEnter(Collider other, Rigidbody rigidbody, CharacterController characterController);
    protected abstract void OnExit(Collider other, Rigidbody rigidbody, CharacterController characterController);

    private bool IsPlayer(Collider other, out Rigidbody rigidbody, out CharacterController characterController)
    {
        rigidbody = other.attachedRigidbody;

        if (rigidbody == null)
        {
            characterController = null;

            return false;
        }

        characterController = rigidbody.GetComponent<CharacterController>();

        return characterController != null;
    }
}