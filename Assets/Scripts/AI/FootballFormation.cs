
using System.Data;
using UnityEngine;

public enum FormationType
{
    FourFourTwo,

    DefenseTest
}

public class FootballFormation : MonoBehaviour
{
    [SerializeField]
    private Transform goalKeeperPosition;
    public Transform GoalKeeperPosition => goalKeeperPosition;
    
    [SerializeField]
    private Transform[] defensePosition;
    public Transform[] DefensePosition
    {
        get { return defensePosition; }
    }
    [SerializeField]
    private Transform[] midfieldPosition;
    public Transform[] MidfieldPosition
    {
        get { return midfieldPosition; }
    }
    [SerializeField]
    private Transform[] forwardPosition;
    public Transform[] ForwardPosition
    {
        get { return forwardPosition; }
    }
    [SerializeField]
    private Transform defenseLine;
    public Transform DefenseLine
    {
        get { return defenseLine; }
    }
    [SerializeField]
    private Transform midfieldLine;
    public Transform MidfieldLine
    {
        get { return midfieldLine; }
    }
    [SerializeField]
    private Transform forwardLine;
    public Transform ForwardLine
    {
        get { return forwardLine; }
    }
    [SerializeField]
    private FormationType formationType = FormationType.FourFourTwo;

    private bool isSetupDone = false;

    private void OnValidate()
    {

        SetupFormation();
        FillPositions();

    }
    private void Awake()
    {
        SetupFormation();
        FillPositions();
    }

    private void SetupFormation()
    {
        
        if (!isSetupDone)
        {
           // switch (formationType)
           // {
           //     case FormationType.FourFourTwo:
           //         defensePosition = new Transform[4];
           //         midfieldPosition = new Transform[4];
           //         forwardPosition = new Transform[2];
           //        
           //         break;
           //     case FormationType.DefenseTest:
           //         defensePosition = new Transform[1];
           //         break;
           // }
            isSetupDone = true;
        }
    }

    private void FillPositions()
    {
        if(defenseLine == null || midfieldLine == null || forwardLine == null || goalKeeperPosition == null)
        {
            Debug.LogError("Please assign all the lines");
            return;
        }

        for (int i = 0; i < defensePosition.Length; i++)
        {
            defensePosition[i] = defenseLine.GetChild(i);
        }
        for (int i = 0; i < midfieldPosition.Length; i++)
        {
            midfieldPosition[i] = midfieldLine.GetChild(i);
        }
        for (int i = 0; i < forwardPosition.Length; i++)
        {
            forwardPosition[i] = forwardLine.GetChild(i);
        }

    }
}

