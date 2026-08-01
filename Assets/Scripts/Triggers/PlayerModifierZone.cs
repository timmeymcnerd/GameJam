using UnityEngine;
using UnityEngine.Events;

public class PlayerModifierZone : AbstractPlayerTrigger
{
    public UnityEvent<Rigidbody, CharacterController> updatePlayer;

    private bool hasPlayer;
    private Collider playerCollider;
    private Rigidbody playerRigidbody;
    private CharacterController characterController;

    private void FixedUpdate()
    {
        if (!this.hasPlayer)
        {
            return;
        }

        this.updatePlayer.Invoke(this.playerRigidbody, this.characterController);
    }

    protected override void OnEnter(Collider other, Rigidbody rigidbody, CharacterController characterController)
    {
        this.hasPlayer = true;
        this.playerCollider = other;
        this.playerRigidbody = rigidbody;
        this.characterController = characterController;
    }

    protected override void OnExit(Collider other, Rigidbody rigidbody, CharacterController characterController)
    {
        this.hasPlayer = false;
        this.playerCollider = null;
        this.playerRigidbody = null;
        this.characterController = null;
    }
}
