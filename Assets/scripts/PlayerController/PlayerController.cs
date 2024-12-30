
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
using Player.Controller.States;
using System.Collections;
namespace Player.Controller
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour    
    {

        [SerializeField] private PlayerData playerData_;
        [SerializeField] private Transform focusPoint_;
        private const int ghostLayer_ = 6;
        private const int playerLayer_ = 3;
        private const int groundLayer_ = 7;
        public PlayerData PlayerData {  get { return playerData_; } }

        #region Input Actions

        private InputAction moveAction_;

        public Vector3 MovementVector { 
            get {
               return moveAction_.ReadValue<Vector2>()
                .ToZXMinus();
            } 
        }

        private InputAction sprintAction_;

        public InputAction SprintAction {  get { return sprintAction_; } }

        public Vector3 LinearVelocity { get { return rigidbody_.linearVelocity; } }

        private InputAction lowActionA_;
        private InputAction lowActionB_;
        private InputAction highActionA_;
        private InputAction highActionB_;
        private InputAction lookAction_;

        public Vector3 WorldMousePosition { 
            get {
                Ray ray = Camera.main.ScreenPointToRay(lookAction_.ReadValue<Vector2>());
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer_))
                {
                    Vector3 worldPosition = hit.point;
                    Debug.Log("Mouse World Position: " + worldPosition);
                    return worldPosition;
                }
                return Vector3.zero;
            } 
        }

        
        

        #endregion

        #region Components
        private Rigidbody rigidbody_;

        public Rigidbody Rigidbody { get { return rigidbody_; } }
        #endregion

        #region Utilities
        [SerializeField] private bool debugSwitch = true;
        private DebugUtils debugger_ = new();

        public DebugUtils Debugger { get { return debugger_; } }
        #endregion

        #region States
        

        private IPlayerState currentState_;
        #endregion

        private PersistenFunctionCollection<string> fixedUpdateFunctions_ = new();

        public PersistenFunctionCollection<string> FixedUpdateFunctions { get { return fixedUpdateFunctions_; } }
        
        private int currentBallAcqusitionStamina_;


      

        private void OnValidate()
        {
            debugger_.SetEnable(debugSwitch);
            
        }

        private void Awake()
        {
            gameObject.layer = playerLayer_;
            debugger_.SetSeverity(Severity.INFO);
            rigidbody_ = GetComponent<Rigidbody>();
            SetActions();


            if (playerData_ == null) debugger_.Log("Player Data is null! Create an assign player data to agent", Severity.ERROR);
            
            
            SetState(new FreeFutbollerState(this));
            currentBallAcqusitionStamina_ = playerData_.MaxBallAcqusitionStamina;
            
            
            BindActions();


        }
        private void FixedUpdate()
        {
            currentState_.Move();
            currentState_.OnFixedUpdate();
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
        private void BindActions()
        {
            lowActionA_.performed += context => { currentState_.OnLowActionAEnter(); };
            lowActionA_.canceled  += context => { currentState_.OnLowActionAExit(); };

            lowActionB_.performed += context => { currentState_.OnLowActionBEnter(); };
            lowActionB_.canceled += context => { currentState_.OnLowActionBExit(); };


            highActionA_.performed += context => { currentState_.OnHighActionAEnter(); };
            highActionA_.canceled += context => { currentState_.OnHighActionAExit(); };
            
            highActionB_.performed += context => { currentState_.OnHighActionBEnter(); };
            highActionB_.canceled += context => { currentState_.OnHighActionBExit(); };

            sprintAction_.performed += context => { currentState_.OnSprintEnter(); };
            sprintAction_.canceled += context => { currentState_.OnSprintExit(); };
        }
        
        public int TryToAcquireBall()
        {
            if ( currentBallAcqusitionStamina_ - PlayerData.BallAcqusitionStaminaReductionRate >  0 )
            {
                currentBallAcqusitionStamina_ -= PlayerData.BallAcqusitionStaminaReductionRate;

            }
            else
            {
                currentBallAcqusitionStamina_ = 0;
            }
            return ( currentBallAcqusitionStamina_/ playerData_.MaxBallAcqusitionStamina) * playerData_.BallAcqusitionPoint;
        }

        public int GetTeamIndex()
        {
            return 0;
        }

        public Vector3 GetFocusPoint()
        {
            return focusPoint_.position;
        }
        public void OnBallStateChanged()
        {
            if (currentState_ != null)
            {
                currentState_.HandleTransition();
            }
        }
        public void SetState(IPlayerState state)
        {
            if (currentState_ != null) { 
                currentState_.OnExit();
            }
            currentState_ = state;
            currentState_.OnEnter();    
        }

        public void ChangeToGhostLayer()
        {
            StartCoroutine(ChangeToGhostLayerForATime(0.4f));
        }
        private IEnumerator ChangeToGhostLayerForATime(float time)
        {

            gameObject.layer = ghostLayer_;
            yield return new WaitForSeconds(time);  
            gameObject.layer = playerLayer_;
        }
    }



}