using BT_Implementation;
using Player.Controller.States;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class GoalkeeperAgent : NetworkBehaviour, IFootballAgent
{
    private PlayerType playerType_;



    [SerializeField]
    private FootballAgentInfo agentInfo_;

    private Rigidbody rigidbody_;

    private bool isInitialized_ = false;

    [HideInInspector]
    public FootballAgentInfo AgentInfo => agentInfo_;
    [HideInInspector]
    public PlayerType PlayerType => playerType_;
    [HideInInspector]
    public Rigidbody Rigidbody => rigidbody_;

    public bool IsInitialized => isInitialized_;

    public Transform Transform => transform;

    private TeamFlag teamFlag_;
    public TeamFlag TeamFlag => teamFlag_;

    [SerializeField]
    private Transform focusPointTransform_;
    public Transform FocusPointTransform => focusPointTransform_;

    [SerializeField]
    private bool isDebugMode_ = false;
    public bool IsDebugMode => isDebugMode_;

    private BTRoot btRoot_;

    private bool isHumanControlled = false;
    private int currentBallAcqusitionStamina_;

    private NetworkObject networkObject;

    private bool enableAI = true;

    [SerializeField]
    private GameObject playerIndicator;
    private void Awake()
    {
        rigidbody_ = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
        currentBallAcqusitionStamina_ = agentInfo_.MaxBallAcqusitionStamina;
        SetActions();
        BindActions();

    }

    private void FixedUpdate()
    {
        TickAISystem();
        HandleHumanInteraction();
        AdjustBallPosition();
    }

    private void AdjustBallPosition()
    {
        if (Football.Instance.CurrentOwnerPlayer == this)
        {
            var targetPosition = FocusPointTransform.position;
            targetPosition.y = Football.Instance.RigidBody.position.y;
            Football.Instance.RigidBody.MovePosition(targetPosition);
        }
    }

    private void HandleHumanInteraction()
    {

        if (isHumanControlled)
        {
            currentState_.Move();
            currentState_.OnFixedUpdate();
        }

    }

    public void InitAISystems(FootballTeam team, PlayerType playerType, int index)
    {
        playerType_ = playerType;
        teamFlag_ = team.TeamFlag;
        var blackboardFactory = new FootballAiBlackboardFactory(this, team, index);

        switch (playerType_)
        {
            case PlayerType.Goalkeeper:
                btRoot_ = new GoalkeeperAIFacade(blackboardFactory.GetBlackboard());
                break;
            case PlayerType.Defender:
                btRoot_ = new GenericAIFacade(blackboardFactory.GetBlackboard());
                break;
            case PlayerType.Midfielder:
                btRoot_ = new GenericAIFacade(blackboardFactory.GetBlackboard());
                break;
            case PlayerType.Forward:
                btRoot_ = new GenericAIFacade(blackboardFactory.GetBlackboard());
                break;

        }

        btRoot_.ConstructBT();

        isInitialized_ = true;
        SetAsAIControlled();

    }

    public void TickAISystem()
    {
        if (!isInitialized_ || isHumanControlled || !enableAI)
        {
            return;
        }
        btRoot_.ExecuteBT();


    }


    public int TryToAcquireBall()
    {

        if (currentBallAcqusitionStamina_ - agentInfo_.BallAcqusitionStaminaReductionRate > 0)
        {
            currentBallAcqusitionStamina_ -= agentInfo_.BallAcqusitionStaminaReductionRate;

        }
        else
        {
            currentBallAcqusitionStamina_ = 0;
        }
        var dice6side = UnityEngine.Random.Range(1, 7);
        return (currentBallAcqusitionStamina_ / agentInfo_.MaxBallAcqusitionStamina) * agentInfo_.BallAcqusitionPoint * dice6side;
    }
    public Action<IFootballAgent> OnBallPossesionCallback { get; set; }




    //human controllable things

    [SerializeField]
    private LayerMask groundMask_;
    private InputAction moveAction_;
    private InputAction sprintAction_;
    private InputAction lowActionA_;
    private InputAction lowActionB_;
    private InputAction highActionA_;
    private InputAction highActionB_;
    private InputAction lookAction_;

    private IPlayerState currentState_;
    public InputAction SprintAction { get => sprintAction_; }
    public Vector3 MovementVector
    {
        get
        {
            return moveAction_.ReadValue<Vector2>()
             .ToZXMinus();
        }
    }
    public Vector3 WorldMousePosition
    {
        get
        {
            Ray ray = Camera.main.ScreenPointToRay(lookAction_.ReadValue<Vector2>());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask_))
            {
                Vector3 worldPosition = hit.point;
                Debug.Log("Mouse World Position: " + worldPosition);
                return worldPosition;
            }
            return Vector3.zero;
        }
    }
    public void SetState(IPlayerState state)
    {
        if (currentState_ != null)
        {
            currentState_.OnExit();
        }
        currentState_ = state;
        currentState_.OnEnter();
    }

    public void SetAsHumanControlled()
    {

        isHumanControlled = true;
        playerIndicator.SetActive(true);
        SetState(new FreeFutbollerState(this));
    }
    public void SetAsAIControlled()
    {
        playerIndicator.SetActive(false);
        isHumanControlled = false;
    }
    private void SetActions(int t_playerIndex = 0)
    {
        string playerPrefix = t_playerIndex >= 0 ? $"Player{t_playerIndex + 1}/" : "";
        Debug.Log(playerPrefix);
        moveAction_ = InputSystem.actions?.FindAction($"{playerPrefix}Move");
        lookAction_ = InputSystem.actions?.FindAction($"{playerPrefix}Look");
        lowActionA_ = InputSystem.actions?.FindAction($"{playerPrefix}LowActionA");
        lowActionB_ = InputSystem.actions?.FindAction($"{playerPrefix}LowActionB");
        highActionA_ = InputSystem.actions?.FindAction($"{playerPrefix}HighActionA");
        highActionB_ = InputSystem.actions?.FindAction($"{playerPrefix}HighActionB");
        sprintAction_ = InputSystem.actions?.FindAction($"{playerPrefix}Sprint");
    }
    private void BindActions()
    {
       if(IsClient && IsOwner)
        {
            lowActionA_.performed += context => { if(isHumanControlled) RequestActionLowAEnterFromServerRpc(); };
            lowActionA_.canceled += context =>  { if(isHumanControlled) RequestActionLowAExitFromServerRpc();  };
                                               
            lowActionB_.performed += context => { if(isHumanControlled) RequestActionLowBEnterFromServerRpc(); };
            lowActionB_.canceled += context =>  { if(isHumanControlled) RequestActionLowBExitFromServerRpc();  };
                                                
                                             
            highActionA_.performed += context =>{ if(isHumanControlled) RequestActionHighAEnterFromServerRpc();};
            highActionA_.canceled += context => { if(isHumanControlled) RequestActionHighAExitFromServerRpc(); };
                                              
            highActionB_.performed += context =>{ if(isHumanControlled) RequestActionHighBEnterFromServerRpc();};
            highActionB_.canceled += context => { if(isHumanControlled) RequestActionHighBExitFromServerRpc();};
                                               
            sprintAction_.performed += context=>{ if(isHumanControlled) RequestActionSprintEnterFromServerRpc();};
            sprintAction_.canceled += context =>{ if (isHumanControlled) RequestActionSprintExitFromServerRpc();};
        }
        else
        {

            lowActionA_.performed += context => { if(isHumanControlled && GameManager.Instance.GameState == EGameState.Playing)  { currentState_.OnLowActionAEnter(); }};
            lowActionA_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnLowActionAExit(); };

            lowActionB_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnLowActionBEnter(); };
            lowActionB_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnLowActionBExit(); };

            highActionA_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnHighActionAEnter(); };
            highActionA_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnHighActionAExit(); };

            highActionB_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnHighActionBEnter(); };
            highActionB_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnHighActionBExit(); };

            sprintAction_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnSprintEnter(); };
            sprintAction_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Playing) currentState_.OnSprintExit(); };
        }
    }


    [Rpc(SendTo.Server)]

    private void RequestActionLowAEnterFromServerRpc( )
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnLowActionAEnter(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowAExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnLowActionAExit(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowBEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnLowActionBEnter(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowBExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnLowActionBExit(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionHighAEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnHighActionAEnter(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionHighAExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnHighActionAExit(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionHighBEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnHighActionBEnter(); }
        Debug.Log("Rpcrequested from server :O");
    }

    [Rpc(SendTo.Server)]

    private void RequestActionHighBExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnHighActionBExit(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionSprintEnterFromServerRpc()
    {
        if ( GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnSprintEnter(); }
        Debug.Log("Rpcrequested from server :O");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionSprintExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Playing && IsServer) { currentState_.OnSprintExit(); }
        Debug.Log("Rpcrequested from server :O");
    }

    public void ChangeToGhostLayer(float time)
    {
        StartCoroutine(ChangeToGhostLayerForATime(time));
    }

    private const int ghostLayer_ = 6;

    private IEnumerator ChangeToGhostLayerForATime(float time)
    {
        var prevMaxVel = rigidbody_.maxLinearVelocity;
        rigidbody_.maxLinearVelocity = 1;
        var prevLayer = gameObject.layer;
        gameObject.layer = ghostLayer_;
        yield return new WaitForSeconds(time);
        gameObject.layer = prevLayer;
        rigidbody_.maxLinearVelocity = prevMaxVel;

    }

    public int GetShootScore()
    {

        var enemyGoal = GameManager.Instance.GetEnemyGoalInstance(TeamFlag);
        var enemyLayer = GameManager.Instance.GetLayerMaskOfEnemy(TeamFlag);
        var possibleLocations = enemyGoal.GetHitPointPositions();
        List<Transform> shootablePositions = new();
        if (Vector3.Distance(enemyGoal.transform.position, Transform.position) < AgentInfo.MaximumShootDistance)
        {
            foreach (var possibleLocation in possibleLocations)
            {
                var direction = possibleLocation.position - Transform.position;
                if (!Physics.Raycast(Transform.position, direction.normalized, direction.magnitude, enemyLayer))
                {
                    shootablePositions.Add(possibleLocation);
                }
            }

        }

        return shootablePositions.Count;


    }

    public void DisableAIForATime(float time)
    {
        StartCoroutine(DisableAIForATimeRoutine(time));
    }

    private IEnumerator DisableAIForATimeRoutine(float time)
    {

        enableAI = false;
        var prevMaxVel = rigidbody_.maxLinearVelocity;
        rigidbody_.maxLinearVelocity = 1;
        var prevLayer = gameObject.layer;
        gameObject.layer = ghostLayer_;
        yield return new WaitForSeconds(time);
        gameObject.layer = prevLayer;
        rigidbody_.maxLinearVelocity = prevMaxVel;
        enableAI = true;
    }

    public void OnBallPossesion()
    {
        if (IsServer)
        {
            OnBallPossesionCallback(this);
        }
    }

    public void init(ulong? ownerId = null,int t_playerIndex = 0)
    {
        if (ownerId != null)
        {
            networkObject.ChangeOwnership((ulong)ownerId);

            if (IsServer && IsOwner)
            {
                Debug.Log("This object is server owned");
                SetActions();
                BindActions();
            }
            else
            {
                NotifyClientItIsTheOwnerRpc();
            }
        }
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyClientItIsTheOwnerRpc()
    {
        if (IsClient && IsOwner)
        {
            Debug.Log("This object is ownded by client");
            SetActions();
            BindActions();
        }
    }

  
}