
using Player.Controller;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum PicthZone
{
    RedZone,
    BlueZone
}

[RequireComponent(typeof(Rigidbody),typeof(Collider))]

public class Football : MonoBehaviour
{
    [SerializeField]
    private float playerCheckRadius_ = 2;
    [SerializeField] private LayerMask playerCheckLayer_;

    [SerializeField] private LayerMask groundCheckLayer_;

    private IFootballAgent currentOwnerPlayer_ = null;
    public IFootballAgent CurrentOwnerPlayer { get { return currentOwnerPlayer_; } }

    private HashSet<IFootballAgent> strugglingPlayers_ = new HashSet<IFootballAgent>();

    public static Football Instance { get; private set; }

    [SerializeField]
    private Rigidbody rigidbody_;
    public Rigidbody RigidBody { get => rigidbody_; }

    private PicthZone pitchZone_;
    public PicthZone PitchZone { get => pitchZone_; }

    private int sectorNumber_;

    public int SectorNumber { get { return sectorNumber_; } }


    [SerializeField]
    private bool enableDebug_ = true;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {

            Destroy(this);

        }
        else
        {
            Instance = this;
        }

        rigidbody_ = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckPlayerCollision();

    }


    private bool isStruggling_ = false;
    private void CheckPlayerCollision()
    {


        var colliders = Physics.OverlapSphere(transform.position, playerCheckRadius_, playerCheckLayer_);

        IFootballAgent playerToAssign = currentOwnerPlayer_;



        foreach (var collider in colliders)
        {

            if (collider.TryGetComponent<IFootballAgent>(out var otherPlayer))
            {
                strugglingPlayers_.Add(otherPlayer);
                if (playerToAssign != null && playerToAssign.TeamFlag != otherPlayer.TeamFlag)
                {
                    var currentPlayerScore = playerToAssign.TryToAcquireBall();
                    var otherPlayerScore = otherPlayer.TryToAcquireBall();
                    playerToAssign = currentPlayerScore > otherPlayerScore ? playerToAssign : otherPlayer;
                    isStruggling_ = true;

                }
                else
                {
                    playerToAssign = otherPlayer;
                }
            }
        }


        if (!isStruggling_)
        {
            strugglingPlayers_.Clear();
        }

        var prevOwner = currentOwnerPlayer_;
        
        currentOwnerPlayer_ = playerToAssign;
        if(currentOwnerPlayer_ != prevOwner) currentOwnerPlayer_.OnBallPossesion();

    }

    public bool IsPlayerStruggling(IFootballAgent player)
    {

        return strugglingPlayers_.Contains(player);
    }

    public Vector3 GetDropPointAfterTSeconds(float time)
    {
        Vector3 currentPosition = rigidbody_.position;
        Vector3 currentVelocity = rigidbody_.linearVelocity;
        
        Vector3 acceleration = Physics.gravity; // Example: gravity

        float deltaTime = 0.02f; // Small time step
        Vector3 position = currentPosition;
        Vector3 velocity = currentVelocity;
        var collider = GetComponent<Collider>();
        var physicsMaterial = collider.material;

        float raycastDistance = 0.1f; // Adjust based on your object's size

        for (float t = 0; t < time; t += deltaTime)
        {
            
            var isOnGroundNow = Physics.Raycast(position, Vector3.down, raycastDistance, groundCheckLayer_) || Physics.Raycast(position, Vector3.up, 10, groundCheckLayer_);



            if (isOnGroundNow)
            {
                velocity.y = 0; // Stop vertical motion
                acceleration.y = 0;
                velocity *= Mathf.Clamp01(1 - physicsMaterial.dynamicFriction * deltaTime); // Apply friction
            }
           

            velocity += acceleration * deltaTime; // Apply gravity or other forces

            position += velocity * deltaTime;
        }

        return position;
    }


    public void HitBall(Vector3 direction, float shootPower)
    {
        rigidbody_.AddForce(direction * shootPower, ForceMode.VelocityChange);
        currentOwnerPlayer_ = null;
    }
    public bool IsGrounded()
    {
        float raycastDistance = 0.1f; // Adjust based on your object's size
        return Physics.Raycast(rigidbody_.position, Vector3.down, raycastDistance,groundCheckLayer_);

    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedZone"))
        {
            pitchZone_ = PicthZone.RedZone;
            var pair = other.gameObject.name.Trim().Split(' ');
            int number = -1;
            bool successfull = int.TryParse(pair[1], out number);
            if (successfull) sectorNumber_ = number;
            Debug.Log($"Red ZONE!! {sectorNumber_}");
        }
        else if (other.CompareTag("BlueZone"))
        {
            pitchZone_ = PicthZone.BlueZone;
            var pair = other.gameObject.name.Trim().Split(' ');
            int number = -1;
            bool successfull = int.TryParse(pair[1], out number);
            if (successfull) sectorNumber_ = number;
            Debug.Log("Blue ZONE!!");
        }

    }

    private void OnDrawGizmos()
    {
        if (!enableDebug_) return;
        
        for ( float t = 0 ; t < 2; t += 0.2f)
        {
            var drop_point = GetDropPointAfterTSeconds(t);
            float raycastDistance = 0.1f; // Adjust based on your object's size
            var isOnGroundNow =  Physics.Raycast(drop_point, Vector3.down, raycastDistance, groundCheckLayer_) || Physics.Raycast(drop_point, Vector3.up, 10, groundCheckLayer_);
            if (isOnGroundNow) break;
            Gizmos.DrawSphere(drop_point, 0.5f);
        }
        Gizmos.DrawSphere (transform.position, playerCheckRadius_);
      
    }
}


