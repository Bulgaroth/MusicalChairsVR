using UnityEngine;

public class AI : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    private Chair currentTarget = null;
    private bool stopThinking = false;

    public bool foundChair { get; private set; }
    private float reactionTime;

    public void Reset()
    {
        stopThinking = false;
        foundChair = false;
        reactionTime = Random.Range(0.1f, 0.5f);
    }

    void Update()
    {
        if (GameManager.instance.currentState == GameManager.GameState.PHASE1)
        {
            if (GameManager.instance.playerIsMoving)
            {
                transform.RotateAround(Vector3.zero, Vector3.up, walkSpeed * Time.deltaTime);
            }
        }
        else if (GameManager.instance.currentState == GameManager.GameState.PHASE2)
        {
            if (stopThinking) return;

            if (reactionTime > 0)
            {
                reactionTime -= Time.deltaTime;
                return;
            }

            if (currentTarget == null || currentTarget.isUsed)
            {
                currentTarget = GameManager.instance.GetNearestEmptyChair(transform.position);
                if (currentTarget == null)
                {
                    stopThinking = true;
                    return;
                }
            }

            if (Vector3.Distance(transform.position, currentTarget.transform.position) <= 0.25f)
            {
                currentTarget.SetUsedBy(transform);
                foundChair = true;
                stopThinking = true;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position, walkSpeed * Time.deltaTime);
            }
        }
        else if (GameManager.instance.currentState == GameManager.GameState.PHASE3 && !foundChair)
        {
            transform.position += Vector3.up * Time.deltaTime * walkSpeed;
        }
    }
}
