using System.Collections;
using UnityEngine;

public class FootballerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GenericAgent genericAgent;

    [Header("Animation Clip Names")]
    public string idleAnimation = "Offensive Idle";
    public string runAnimation = "Running";
    public string shootAnimation = "Soccer Pass";

    [Header("Settings")]
    public float runSpeedThreshold = 0.1f;
    public float minRunMultiplier = 0.5f;
    public float maxRunMultiplier = 2.0f;

    private bool isShooting = false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on " + gameObject.name);
            }
        }
    }

    private void Start()
    {
        if (genericAgent == null)
        {
            genericAgent = GetComponent<GenericAgent>();
            if (genericAgent == null)
            {
                Debug.LogError("GenericAgent component not found on " + gameObject.name);
            }
        }
    }

    private void Update()
    {
        if (isShooting || genericAgent == null)
            return;

        float speed = genericAgent.Rigidbody.linearVelocity.magnitude;

        if (speed > runSpeedThreshold)
        {
            PlayRunAnimation(speed);
        }
        else
        {
            PlayIdleAnimation();
        }
    }

    public void PlayShootAnimation()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;
        animator.speed = 3f;
        animator.Play(shootAnimation);
        yield return new WaitForSeconds(GetAnimationClipLength(shootAnimation));
        isShooting = false;
        animator.speed = 1f;
    }

    private void PlayIdleAnimation()
    {
        if (!IsCurrentAnimation(idleAnimation))
        {
            animator.Play(idleAnimation);
            animator.speed = 1f;
        }
    }

    private void PlayRunAnimation(float speed)
    {
        if (!IsCurrentAnimation(runAnimation))
        {
            animator.Play(runAnimation);
        }
        float runMultiplier = Mathf.Clamp(speed, minRunMultiplier, maxRunMultiplier);
        animator.speed = runMultiplier;
    }

    private bool IsCurrentAnimation(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }

    private float GetAnimationClipLength(string animationName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        Debug.LogWarning($"Animation clip {animationName} not found; defaulting to 0.5s wait.");
        return 0.5f;
    }
}