using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
namespace Player.Controller
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {

        [SerializeField] private PlayerData playerData_;

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

        
        private InputAction lowActionA_;
        private InputAction lowActionB_;
        private InputAction highActionA_;
        private InputAction highActionB_;
        private InputAction lookAction_;

        public Vector3 WorldMousePosition { 
            get {
                return Camera.main.ScreenToWorldPoint(lookAction_.ReadValue<Vector2>().WithZ(Camera.main.transform.position.z - 1));
            } 
        }

        
        

        #endregion

        #region Components
        private Rigidbody rigidbody_;

        public Rigidbody Rigidbody { get { return rigidbody_; } }
        #endregion

        #region Utilities
        [SerializeField] private bool debugSwitch = true;
        DebugUtils debugger_ = new();
        #endregion

        #region States
        

        private IPlayerActions currentState_;
        #endregion

        public SoccerBall SoccerBall = null;

        private PersistenFunctionCollection<string> fixedUpdateFunctions_ = new();

        public PersistenFunctionCollection<string> FixedUpdateFunctions { get { return fixedUpdateFunctions_; } }


        public int GetAcqusitionScore()
        {
            return 0;
        }

        public void SetOwner(SoccerBall soccerBall)
        {
            
        }

        private void OnValidate()
        {
            debugger_.SetEnable(debugSwitch);
            
        }

        private void Awake()
        {
            debugger_.SetSeverity(Severity.INFO);
            rigidbody_ = GetComponent<Rigidbody>();
            SetActions();


            if (playerData_ == null) debugger_.Log("Player Data is null! Create an assign player data to agent", Severity.ERROR);
            
            currentState_ = new FreeFutbollerState(this);
            
            
            
            BindActions();


        }
        private void FixedUpdate()
        {
            currentState_.Move();
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


        private void OnCollisionEnter(Collision collision)
        {
            currentState_.HandleCollision(collision);
        }

    }



}