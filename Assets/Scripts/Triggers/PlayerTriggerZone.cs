using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerZone : AbstractPlayerTrigger
{
    public UnityEvent<Rigidbody, CharacterController> onTriggerEnter;
    public UnityEvent<Rigidbody, CharacterController> onTriggerExit;

    protected override void OnEnter(Collider other, Rigidbody rigidbody, CharacterController characterController)
    {
        this.onTriggerEnter?.Invoke(rigidbody, characterController);
    }

    protected override void OnExit(Collider other, Rigidbody rigidbody, CharacterController characterController)
    {
        this.onTriggerExit?.Invoke(rigidbody, characterController);
    }
}