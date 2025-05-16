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
    private bool isHumanControlled { get { return isHumanControlledSync.Value; } }
    private int currentBallAcqusitionStamina_;


    private bool enableAI = true;
    private NetworkObject networkObject;
    [SerializeField]
    private GameObject playerIndicator;


    private NetworkVariable<bool> isHumanControlledSync = new NetworkVariable<bool>();
    private NetworkVariable<Vector2> movementVectorClientAuthorized = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector2> mousePosClientAuthorized = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();  
        rigidbody_ = GetComponent<Rigidbody>();

        currentBallAcqusitionStamina_ =agentInfo_.MaxBallAcqusitionStamina;
       

    }
    public void init(ulong? ownerId)
    {
        if (ownerId != null)
        {
            networkObject.ChangeOwnership((ulong)ownerId);
        }

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
    public void OnBallPossesion()
    {
        if (IsServer)
        {
            OnBallPossesionCallback(this);
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
    private void Update()
    {
        if (IsClient && IsOwner && isHumanControlled)
        {
            movementVectorClientAuthorized.Value = moveAction_.ReadValue<Vector2>();
            mousePosClientAuthorized.Value = lookAction_.ReadValue<Vector2>();
        }
    }
    private void FixedUpdate()
    {
        if (IsClient) { return; }
        if (GameManager.Instance.GameState == EGameState.Running)
        {

            TickAISystem();
            HandleHumanInteraction();
            AdjustBallPosition();
        }
    }

    private void AdjustBallPosition()
    {
        if ( Football.Instance.CurrentOwnerPlayer == (IFootballAgent)this )
        {
            var targetPosition = FocusPointTransform.position;
            targetPosition.y = Football.Instance.RigidBody.position.y;
            Football.Instance.RigidBody.MovePosition(targetPosition);
        }
    }

    private void HandleHumanInteraction()
    {
        
        if(isHumanControlled)
        {
            currentState_.Move();
            currentState_.OnFixedUpdate();
        }
      
    }

    public void InitAISystems(FootballTeam team, PlayerType playerType,int index)
    {
        playerType_ = playerType;
        teamFlag_ = team.TeamFlag;
        var blackboardFactory = new FootballAiBlackboardFactory(this,team,index);

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

        if (TeamFlag == TeamFlag.Red)
        {

            GetComponent<MeshRenderer>().material.color = Color.red;
            if (IsServer) SetColorRedRpc();
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;
            if (IsServer) SetColorBlueRpc();
        }

    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SetColorRedRpc()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void SetColorBlueRpc()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
    public void TickAISystem()
    {
        if(!isInitialized_ || isHumanControlled || !enableAI)
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
        var dice6side =  UnityEngine.Random.Range(1, 7);
        return (currentBallAcqusitionStamina_ / agentInfo_.MaxBallAcqusitionStamina) * agentInfo_.BallAcqusitionPoint * dice6side;
    }
    public Action<IFootballAgent> OnBallPossesionCallback { get; set; }

    


    //human controllable things

    [SerializeField]
    private LayerMask groundMask_;
    private InputAction moveAction_;
    private InputAction sprintAction_;
    private InputAction lowActionA_ ;
    private InputAction lowActionB_ ;
    private InputAction highActionA_;
    private InputAction highActionB_;
    private InputAction lookAction_;

    private IPlayerState currentState_;
    public InputAction SprintAction { get => sprintAction_; }
    public Vector3 MovementVector
    {
        get
        {
            if (IsServer && !IsOwner)
            {
                return movementVectorClientAuthorized.Value.ToZXMinus();
            }
            else
            {
                return moveAction_.ReadValue<Vector2>()
                    .ToZXMinus();
            }

        }
    }
    public Vector3 WorldMousePosition
    {
        get
        {
            Vector2 mousePos;
            if (IsServer && !IsOwner)
            {
                mousePos = mousePosClientAuthorized.Value;
            }
            else
            {
                mousePos = lookAction_.ReadValue<Vector2>();
            }
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask_))
            {
                Vector3 worldPosition = hit.point;
                // Debug.Log("Mouse World Position: " + worldPosition);
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
    [Rpc(SendTo.ClientsAndHost)]
    private void OnPlayerIndicatorRpc()
    {
        playerIndicator.SetActive(true);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void OffPlayerIndicatorRpc()
    {
        playerIndicator.SetActive(false);
    }
    public void SetAsHumanControlled()
    {
        
        isHumanControlledSync.Value = true;
        playerIndicator.SetActive(true);
        if (IsServer) OnPlayerIndicatorRpc();
        SetState(new FreeFutbollerState(this));
    }
    public void SetAsAIControlled()
    {
        playerIndicator.SetActive(false);
        if (IsServer) OffPlayerIndicatorRpc();
        isHumanControlledSync.Value = false;
    }
    private void SetActions()
    {
        moveAction_ = InputSystem.actions.FindAction("Move");
        lookAction_ = InputSystem.actions.FindAction("Look");
        lowActionA_ = InputSystem.actions.FindAction("LowActionA");
        lowActionB_ = InputSystem.actions.FindAction("LowActionB");
        highActionA_ = InputSystem.actions.FindAction("HighActionA");
        highActionB_ = InputSystem.actions.FindAction("HighActionB");
        sprintAction_ = InputSystem.actions.FindAction("Sprint");
        lookAction_ = InputSystem.actions.FindAction("Look");
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowAEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnLowActionAEnter(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowAExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnLowActionAExit(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowBEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnLowActionBEnter(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionLowBExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnLowActionBExit(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionHighAEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnHighActionAEnter(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionHighAExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnHighActionAExit(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionHighBEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnHighActionBEnter(); }
    }

    [Rpc(SendTo.Server)]

    private void RequestActionHighBExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnHighActionBExit(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionSprintEnterFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnSprintEnter(); }
    }
    [Rpc(SendTo.Server)]

    private void RequestActionSprintExitFromServerRpc()
    {
        if (GameManager.Instance.GameState == EGameState.Running && IsServer) { currentState_.OnSprintExit(); }
    }
    private void BindActions()
    {
        if (IsClient && IsOwner)
        {
            lowActionA_.performed += context => { if (isHumanControlled) RequestActionLowAEnterFromServerRpc(); };
            lowActionA_.canceled += context => { if (isHumanControlled) RequestActionLowAExitFromServerRpc(); };

            lowActionB_.performed += context => { if (isHumanControlled) RequestActionLowBEnterFromServerRpc(); };
            lowActionB_.canceled += context => { if (isHumanControlled) RequestActionLowBExitFromServerRpc(); };


            highActionA_.performed += context => { if (isHumanControlled) RequestActionHighAEnterFromServerRpc(); };
            highActionA_.canceled += context => { if (isHumanControlled) RequestActionHighAExitFromServerRpc(); };

            highActionB_.performed += context => { if (isHumanControlled) RequestActionHighBEnterFromServerRpc(); };
            highActionB_.canceled += context => { if (isHumanControlled) RequestActionHighBExitFromServerRpc(); };

            sprintAction_.performed += context => { if (isHumanControlled) RequestActionSprintEnterFromServerRpc(); };
            sprintAction_.canceled += context => { if (isHumanControlled) RequestActionSprintExitFromServerRpc(); };
        }
        else
        {

            lowActionA_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) { currentState_.OnLowActionAEnter(); } };
            lowActionA_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnLowActionAExit(); };

            lowActionB_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnLowActionBEnter(); };
            lowActionB_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnLowActionBExit(); };


            highActionA_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnHighActionAEnter(); };
            highActionA_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnHighActionAExit(); };

            highActionB_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnHighActionBEnter(); };
            highActionB_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnHighActionBExit(); };

            sprintAction_.performed += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnSprintEnter(); };
            sprintAction_.canceled += context => { if (isHumanControlled && GameManager.Instance.GameState == EGameState.Running) currentState_.OnSprintExit(); };
        }
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
        if (Vector3.Distance(enemyGoal.transform.position, Transform.position) <AgentInfo.MaximumShootDistance)
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
    
  
}