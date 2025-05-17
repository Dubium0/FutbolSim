
using Unity.Netcode;
using UnityEngine;

namespace FootballSim.Player
{

    public class FootballPlayer : NetworkBehaviour
    {

        private IFootballPlayerInputSource m_InputSource;
        public IFootballPlayerInputSource InputSource { get { return m_InputSource; } }

        public Rigidbody Rigidbody { get { return GetComponent<Rigidbody>(); } }
        public Transform Transform { get { return transform; } }

        public FootballPlayerData Data { get { return m_Data; } }

        [SerializeField]
        private FootballPlayerData m_Data;


        private IPlayerState m_CurrentState;

        // always sycn values
        private NetworkVariable<Vector3> m_SyncedPosition = new NetworkVariable<Vector3>(
    Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<Quaternion> m_SyncedRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        private void Update()
        {
            if (IsServer)
            {
                m_CurrentState.OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                m_CurrentState.OnFixedUpdate();
            }
        }





    }   

}