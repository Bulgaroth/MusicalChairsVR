using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public enum GameState { NOTINGAME, PHASE1, PHASE2, PHASE3 };
    public GameState currentState { get; private set; }
    public bool playerIsMoving { get; set; }

    [Header("General")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Transform chairsRoot;
    [SerializeField] private float chairsRotationSpeed = 5f;
    [SerializeField] private List<Chair> chairs;
    [SerializeField] private AI prefabAI;
    private bool playerHadFoundChair { get; set; }
    private List<AI> instancedAIs;


    [Header("Phase 1")]
    [SerializeField] private float aiRadius = 8f;
    [SerializeField] private float phaseOneBaseLength;

    [Header("Phase 3")]
    [SerializeField] private float phaseThreeEndLength = 3f;

    private float currentPhaseStart;

    void Awake()
    {
        instance = this;
        currentState = GameState.NOTINGAME;
        playerIsMoving = false;

        StartGame();
    }

    public void StartGame()
    {
        instancedAIs = new List<AI>();
        for (int i = 0; i < chairs.Count; i++)
        {
            instancedAIs.Add(Instantiate(
                prefabAI,
                Vector3.zero,
                Quaternion.identity));
        }

        StartPhase1();
    }

    void ResetStates(bool resetAIPositions)
    {
        playerHadFoundChair = false;

        foreach (Chair chair in chairs)
        {
            chair.SetUsedBy(null);
        }
        foreach (AI air in instancedAIs)
        {
            air.Reset();
        }

        if (!resetAIPositions) return;

        float slice = 2 * Mathf.PI / instancedAIs.Count;
        float t;
        for (int i = 0; i < instancedAIs.Count; i++)
        {
            t = slice * i;
            Vector3 position = new Vector3(
                aiRadius * Mathf.Cos(t) + 0f,
                0,
                aiRadius * Mathf.Sin(t) + 0f
            );

            instancedAIs[i].transform.position = position;
        }
    }

    public void StartPhase1()
    {
        ResetStates(true);
        foreach (AI ai in instancedAIs) ai.OnPhaseOne();
        musicSource.Play();
        currentState = GameState.PHASE1;
        currentPhaseStart = Time.time;
    }

    public void StartPhase2()
    {
        ResetStates(false);
        foreach (AI ai in instancedAIs) ai.OnPhaseTwo();
        musicSource.Pause();
        currentState = GameState.PHASE2;
        currentPhaseStart = Time.time;
    }

    public void StartPhase3()
    {
        foreach (AI ai in instancedAIs) ai.OnPhaseThree();
        musicSource.Pause();
        currentState = GameState.PHASE3;
        currentPhaseStart = Time.time;
    }

    public void EndGame()
    {
        musicSource.Pause();
        currentState = GameState.NOTINGAME;
        print("The game ended");
    }

    public Chair GetNearestEmptyChair(Vector3 position)
    {
        Chair nearest = null;
        float nearestDist = float.MaxValue;
        foreach (Chair chair in chairs)
        {
            float dist = Vector3.Distance(chair.transform.position, position);
            if (!chair.isUsed && dist < nearestDist)
            {
                nearest = chair;
                nearestDist = dist;
            }
        }

        return nearest;
    }


    void Update()
    {
        if (currentState == GameState.PHASE1)
        {
            if (!playerIsMoving)
            {
                chairsRoot.Rotate(Vector3.up, Time.deltaTime * chairsRotationSpeed);
            }

            if (Time.time - currentPhaseStart >= phaseOneBaseLength)
            {
                StartPhase2();
            }
        }
        else if (currentState == GameState.PHASE2)
        {

            if (Keyboard.current.enterKey.IsPressed() && !playerHadFoundChair)
            {
                Chair found = GetNearestEmptyChair(Vector3.zero);
                if (found)
                {
                    playerHadFoundChair = true;
                    found.SetUsedBy(transform);
                }
            }

            int amountDone = playerHadFoundChair ? 1 : 0;
            foreach (AI ai in instancedAIs)
            {
                if (ai.foundChair) amountDone++;
            }

            if (amountDone == chairs.Count)
            {
                StartPhase3();
            }
        }
        else if (currentState == GameState.PHASE3)
        {
            if (Time.time - currentPhaseStart >= phaseThreeEndLength)
            {
                // Check if done

                if (!playerHadFoundChair)
                {
                    EndGame();
                    return;
                }

                foreach (AI ai in instancedAIs)
                {
                    if (!ai.foundChair)
                    {
                        Destroy(ai.gameObject);
                        instancedAIs.Remove(ai);
                        break;
                    }
                }

                // Stop when only one chair remains
                if (chairs.Count == 1)
                {
                    EndGame();
                    return;
                }

                // Remove a random chair
                int destroyIdx = Random.Range(0, chairs.Count);
                Destroy(chairs[destroyIdx].gameObject);
                chairs.RemoveAt(destroyIdx);

                StartPhase1();
            }
        }

    }
}
