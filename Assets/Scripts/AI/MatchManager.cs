using System.Collections;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [SerializeField] private Transform centerSpot;
    [SerializeField] private Transform[] goalKickPositionsRed;
    [SerializeField] private Transform[] goalKickPositionsBlue;
    [SerializeField] private Transform[] cornerPositionsRed;
    [SerializeField] private Transform[] cornerPositionsBlue;

    [SerializeField] private TextMeshProUGUI redScoreText; // Home Team
    [SerializeField] private TextMeshProUGUI blueScoreText; // Away Team (player controlled)

    public int RedTeamScore { get; private set; }
    public int BlueTeamScore { get; private set; }

    private Football football;
    private bool isMatchPaused;

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
    }

    private void Start()
    {
        football = Football.Instance;
        if (football == null)
        {
            Debug.LogError("Football instance is null. Ensure Football is added to the scene.");
            return;
        }

        ResetMatch();
    }

    private void ResetMatch()
    {
        RedTeamScore = 0;
        BlueTeamScore = 0;
        UpdateScoreUI();
        RestartFromCenter();
    }

    public void HandleGoal(TeamFlag scoringTeam)
    {
        if (scoringTeam == TeamFlag.Red)
        {
            RedTeamScore++;
            Debug.Log("Goal for Red Team!");
        }
        else if (scoringTeam == TeamFlag.Blue)
        {
            BlueTeamScore++;
            Debug.Log("Goal for Blue Team!");
        }

        UpdateScoreUI();

        // Slow down time briefly after a goal
        StartCoroutine(SlowTimeAndRestart());
    }

    private IEnumerator SlowTimeAndRestart()
    {
        isMatchPaused = true;

        // Slow down time
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(1f);

        // Reset time scale to normal
        Time.timeScale = 1f;

        // Restart the match from the center
        RestartFromCenter();
    }

    public void HandleOut(TeamFlag lastTouchTeam)
    {
        TeamFlag goalKickTeam = lastTouchTeam == TeamFlag.Red ? TeamFlag.Blue : TeamFlag.Red;
        HandleGoalKick(goalKickTeam);
    }

    private void UpdateScoreUI()
    {
        redScoreText.text = RedTeamScore.ToString();
        blueScoreText.text = BlueTeamScore.ToString();
    }

    public void RestartFromCenter()
    {
        ResetFootball(centerSpot.position);
        ResetPlayersToFormation();
        Invoke(nameof(ResumeMatch), 1f);
    }

    public void HandleGoalKick(TeamFlag team)
    {
        isMatchPaused = true;
        Vector3 goalKickPosition = team == TeamFlag.Red ? goalKickPositionsRed[0].position : goalKickPositionsBlue[0].position;
        ResetFootball(goalKickPosition);
        ResetPlayersToFormation();
        Invoke(nameof(ResumeMatch), 1f);
    }

    private void ResetFootball(Vector3 position)
    {
        football.RigidBody.linearVelocity = Vector3.zero;
        football.RigidBody.angularVelocity = Vector3.zero;
        football.transform.position = position;

        // Free the ball from any owner
        football.ClearOwner();
    }

    private void ResetPlayersToFormation()
    {
        GameManager.Instance.RedFootballTeam.ResetToFormation();
        GameManager.Instance.BlueFootballTeam.ResetToFormation();
    }

    private void ResumeMatch()
    {
        isMatchPaused = false;
    }
}
