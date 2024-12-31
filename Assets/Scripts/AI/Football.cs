
using Player.Controller;
using System.Collections.Generic;
using UnityEngine;

public enum PicthZone
{
    RedZone,
    BlueZone
}

[RequireComponent(typeof(Rigidbody))]
public class Football : MonoBehaviour
{
    [SerializeField]
    private float playerCheckRadius_ = 2;
    [SerializeField] private LayerMask playerCheckLayer_;

    private IFootballAgent currentOwnerPlayer_ = null;
    public IFootballAgent CurrentOwnerPlayer { get { return currentOwnerPlayer_; } }

    private HashSet<IFootballAgent> strugglingPlayers_ = new HashSet<IFootballAgent>();

    public static Football Instance { get; private set; }

    private Rigidbody rigidbody_;
    public Rigidbody RigidBody { get => rigidbody_; }

    private PicthZone pitchZone_;
    public PicthZone PitchZone { get => pitchZone_; }

    private int sectorNumber_;
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
        AdjustBallPosition();
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

    }

    private void AdjustBallPosition()
    {
        if (currentOwnerPlayer_ != null)
        {
            var transformedPosition = transform.position;
            transform.position = currentOwnerPlayer_.FocusPointTransform.position;
            transformedPosition.x = currentOwnerPlayer_.FocusPointTransform.position.x;
            transformedPosition.z = currentOwnerPlayer_.FocusPointTransform.position.z;
            transform.position = transformedPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedZone"))
        {
            pitchZone_ = PicthZone.RedZone;

            var name = other.gameObject.name.Trim();

        }
        else if (other.CompareTag("BlueZone"))
        {
            pitchZone_ = PicthZone.BlueZone;
        }

    }
}


