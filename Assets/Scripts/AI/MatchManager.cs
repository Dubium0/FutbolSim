using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [SerializeField] private Transform centerSpot;
    [SerializeField] private Transform[] goalKickPositionsHome;
    [SerializeField] private Transform[] goalKickPositionsAway;
    [SerializeField] private Transform[] cornerPositionsHome;
    [SerializeField] private Transform[] cornerPositionsAway;

    [SerializeField] private TextMeshProUGUI homeScoreText; // Home Team
    [SerializeField] private TextMeshProUGUI awayScoreText; // Away Team (player controlled)

    public int HomeTeamScore { get; private set; }
    public int AwayTeamScore { get; private set; }

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
        HomeTeamScore = 0;
        AwayTeamScore = 0;
        UpdateScoreUI();
        RestartFromCenter(TeamFlag.Home);
    }

    public void HandleGoal(TeamFlag scoringTeam)
    {
        if (scoringTeam == TeamFlag.Home)
        {
            HomeTeamScore++;
            Debug.Log("Goal for Home Team!");
        }
        else if (scoringTeam == TeamFlag.Away)
        {
            AwayTeamScore++;
            Debug.Log("Goal for Away Team!");
        }

        UpdateScoreUI();

        // Slow down time briefly after a goal
        StartCoroutine(SlowTimeAndRestart(scoringTeam));
    }

    private IEnumerator SlowTimeAndRestart(TeamFlag scorerTeam)
    {
        isMatchPaused = true;

        // Slow down time
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(1f);

        // Reset time scale to normal
        Time.timeScale = 1f;

        // Restart the match from the center
        RestartFromCenter(scorerTeam);
    }

    public void HandleOut(TeamFlag lastTouchTeam)
    {
        TeamFlag goalKickTeam = lastTouchTeam == TeamFlag.Home ? TeamFlag.Away : TeamFlag.Home;
        HandleGoalKick(goalKickTeam);
    }

    private void UpdateScoreUI()
    {
        homeScoreText.text = HomeTeamScore.ToString();
        awayScoreText.text = AwayTeamScore.ToString();
    }

    public void RestartFromCenter(TeamFlag startingTeam)
    {
        ResetFootball(centerSpot.position);
        ResetPlayersToFormation(startingTeam);
        Invoke(nameof(ResumeMatch), 1f);
    }

    public void HandleGoalKick(TeamFlag team)
    {
        isMatchPaused = true;
        Vector3 goalKickPosition = team == TeamFlag.Home ? goalKickPositionsHome[0].position : goalKickPositionsAway[0].position;
        ResetFootball(goalKickPosition);
        ResetPlayersToFormation(team);
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

    private void ResetPlayersToFormation(TeamFlag startingTeam)
    {
        switch (startingTeam)
        {
            case TeamFlag.Home:
                GameManager.Instance.homeFootballTeam.ResetToFormation(FormationPhase.AttackStart);
                GameManager.Instance.awayFootballTeam.ResetToFormation(FormationPhase.DefenseStart);
                break;
            case TeamFlag.Away:
                GameManager.Instance.homeFootballTeam.ResetToFormation(FormationPhase.DefenseStart);
                GameManager.Instance.awayFootballTeam.ResetToFormation(FormationPhase.AttackStart);
                break;
            case TeamFlag.None:
                break;
        }
      
        
    }

    private void ResumeMatch()
    {
        isMatchPaused = false;
    }
}
