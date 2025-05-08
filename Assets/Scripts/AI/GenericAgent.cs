using BT_Implementation;
using Player.Controller.States;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody))]
public class GenericAgent : MonoBehaviour, IFootballAgent
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
    private int playerIndex_ = -1; // -1 means not assigned to a player

    public bool IsHumanControlled => isHumanControlled;

    private bool enableAI = true;

    [SerializeField]
    private GameObject playerIndicator;

    [Header("Team Materials")]
    [SerializeField] private Material redTeamMaterial;
    [SerializeField] private Material blueTeamMaterial;

    private void Awake()
    {
        rigidbody_ = GetComponent<Rigidbody>();

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
        if ( Football.Instance.CurrentOwnerPlayer == this )
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
    public void SetPlayerIndex(int index)
    {
        // Unbind existing actions first
        UnbindActions();
        
        playerIndex_ = index;
        SetActions();
        BindActions();
    }
    private void SetActions()
    {
        // Create player-specific action maps
        string playerPrefix = playerIndex_ >= 0 ? $"Player{playerIndex_ + 1}/" : "";
        moveAction_ = InputSystem.actions?.FindAction($"{playerPrefix}Move");
        lookAction_ = InputSystem.actions?.FindAction($"{playerPrefix}Look");
        lowActionA_ = InputSystem.actions?.FindAction($"{playerPrefix}LowActionA");
        lowActionB_ = InputSystem.actions?.FindAction($"{playerPrefix}LowActionB");
        highActionA_ = InputSystem.actions?.FindAction($"{playerPrefix}HighActionA");
        highActionB_ = InputSystem.actions?.FindAction($"{playerPrefix}HighActionB");
        sprintAction_ = InputSystem.actions?.FindAction($"{playerPrefix}Sprint");

        // Log warning if actions are not found
        if (moveAction_ == null || lookAction_ == null || lowActionA_ == null || 
            lowActionB_ == null || highActionA_ == null || highActionB_ == null || 
            sprintAction_ == null)
        {
            Debug.LogWarning($"Some input actions not found for player {playerIndex_ + 1}. Make sure the input actions asset is properly configured.");
        }
    }
    private void BindActions()
    {
        // Only bind actions if they exist
        if (lowActionA_ != null)
        {
            lowActionA_.performed += OnLowActionAPerformed;
            lowActionA_.canceled += OnLowActionACanceled;
        }

        if (lowActionB_ != null)
        {
            lowActionB_.performed += OnLowActionBPerformed;
            lowActionB_.canceled += OnLowActionBCanceled;
        }

        if (highActionA_ != null)
        {
            highActionA_.performed += OnHighActionAPerformed;
            highActionA_.canceled += OnHighActionACanceled;
        }

        if (highActionB_ != null)
        {
            highActionB_.performed += OnHighActionBPerformed;
            highActionB_.canceled += OnHighActionBCanceled;
        }

        if (sprintAction_ != null)
        {
            sprintAction_.performed += OnSprintPerformed;
            sprintAction_.canceled += OnSprintCanceled;
        }
    }

    private void UnbindActions()
    {
        if (lowActionA_ != null)
        {
            lowActionA_.performed -= OnLowActionAPerformed;
            lowActionA_.canceled -= OnLowActionACanceled;
        }

        if (lowActionB_ != null)
        {
            lowActionB_.performed -= OnLowActionBPerformed;
            lowActionB_.canceled -= OnLowActionBCanceled;
        }

        if (highActionA_ != null)
        {
            highActionA_.performed -= OnHighActionAPerformed;
            highActionA_.canceled -= OnHighActionACanceled;
        }

        if (highActionB_ != null)
        {
            highActionB_.performed -= OnHighActionBPerformed;
            highActionB_.canceled -= OnHighActionBCanceled;
        }

        if (sprintAction_ != null)
        {
            sprintAction_.performed -= OnSprintPerformed;
            sprintAction_.canceled -= OnSprintCanceled;
        }
    }

    private void OnLowActionAPerformed(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnLowActionAEnter();
        }
        FootballerAnimator footballerAnimator = GetComponent<FootballerAnimator>();
        if (footballerAnimator != null && currentState_ is DribblingFutbollerState)
        {
            footballerAnimator.PlayShootAnimation();
        }
    }

    private void OnLowActionACanceled(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnLowActionAExit();
        }
    }

    private void OnLowActionBPerformed(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnLowActionBEnter();
        }
    }

    private void OnLowActionBCanceled(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnLowActionBExit();
        }
    }

    private void OnHighActionAPerformed(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnHighActionAEnter();
        }
    }

    private void OnHighActionACanceled(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnHighActionAExit();
        }
    }

    private void OnHighActionBPerformed(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnHighActionBEnter();
        }
    }

    private void OnHighActionBCanceled(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnHighActionBExit();
        }
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnSprintEnter();
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        if (isHumanControlled)
        {
            currentState_.OnSprintExit();
        }
    }

    private void OnDestroy()
    {
        UnbindActions();
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

    public void AssignTeamMaterial(TeamFlag tf)
    {
        GameObject child1 = transform.GetChild(0).gameObject;
        GameObject child2 = child1.transform.GetChild(0).gameObject;
        GameObject child3 = child2.transform.GetChild(0).gameObject;

        SkinnedMeshRenderer agentRenderer = child3.GetComponent<SkinnedMeshRenderer>();
        if (agentRenderer != null)
        {
            Material teamMaterial = tf == TeamFlag.Red ? redTeamMaterial : blueTeamMaterial;

            Material[] currentMaterials = agentRenderer.materials;
            currentMaterials[4] = teamMaterial; // Only change the first material  
            agentRenderer.materials = currentMaterials;
        }
        else
        {
            Debug.LogError($"SkinnedMeshRenderer component not found in children of {gameObject.name}");
        }
    }

}

