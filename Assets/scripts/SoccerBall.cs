using UnityEngine;
[RequireComponent (typeof(Rigidbody))]
public class SoccerBall : MonoBehaviour
{



    PlayerController ownerPlayer_;
    private Rigidbody rigidBody_;

    
    private void Awake()
    {
        rigidBody_ = GetComponent<Rigidbody> ();
    }
    public void AcqusitionRequest(PlayerController aPlayer)
    {
        HandleAcqusitionRequest(aPlayer);   
    }
    private void HandleAcqusitionRequest(PlayerController aPlayer)
    {
        // check states etc 

        transform.position = aPlayer.PlayerFrontTransform.position;
      
       
    }

    public void ApplyForce(Vector3 forceVector)
    {
        rigidBody_.AddForce(forceVector,ForceMode.Impulse);
    }

}
