
using UnityEngine;
[CreateAssetMenu(fileName = "Football Formation", menuName = "Futbol Sim/Football Formation")]
public class FootballFormation : ScriptableObject
{
    public Transform[] FormationPosition = new Transform[10];// align defense-midfield-forward  

    public int defenseCount;
    public int midfieldCount;
    public int forwardCount;


}

