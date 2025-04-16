using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }

	public enum GameState { NOTINGAME, PHASE1, PHASE2, PHASE3 };
	public GameState currentState { get; private set; }
	public bool playerIsMoving { get; set; }
	public bool inPause { get { return pauseMenuRoot.activeInHierarchy; } }

	[Header("General")]
	[SerializeField] private AudioSource musicSource;
	[SerializeField] private Transform chairsRoot;
	[SerializeField] private float chairsRotationSpeed = 5f;
	[SerializeField] private Chair chairPrefab;
	[SerializeField] private AI prefabAI;
	[SerializeField] private float chairRadius;
	[SerializeField] private ArmSwingingManager continuousMove;
	[SerializeField] private TeleportDetector[] tpDetectors;
	[SerializeField] private InputActionReference menuBtn;

	public bool playerHadFoundChair { get; set; }
	private List<AI> instancedAIs;
	private List<Chair> chairs;


	[Header("Phase 1")]
	[SerializeField] private float aiRadius = 8f;
	[SerializeField] private float phaseOneBaseLength;

	[Header("Phase 3")]
	[SerializeField] private float phaseThreeEndLength = 3f;

	[Header("Menus")]
	[SerializeField] private GameObject startMenuRoot;
	[SerializeField] private GameObject pauseMenuRoot;
	private float pauseStart;

	private float currentPhaseStart;

	void Awake()
	{
		instance = this;
		currentState = GameState.NOTINGAME;
		playerIsMoving = false;

		menuBtn.action.performed += _ => PauseMenu();
	}

	void Start()
	{
		startMenuRoot.SetActive(true);
		pauseMenuRoot.SetActive(false);
		startMenuRoot.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
		startMenuRoot.transform.forward = Camera.main.transform.forward;
	}


	public void StartGame(int amountOfChairs)
	{
		startMenuRoot.SetActive(false);

		instancedAIs = new List<AI>();
		for (int i = 0; i < amountOfChairs; i++)
		{
			instancedAIs.Add(Instantiate(
				prefabAI,
				Vector3.zero,
				Quaternion.identity));
		}

		chairs = new List<Chair>();
		for (int i = 0; i < amountOfChairs; i++)
		{
			chairs.Add(Instantiate(
				chairPrefab,
				chairsRoot));
		}

		StartPhase1();

		ToggleMenu(false);
	}

	void ResetStates(bool resetPositions)
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

		if (!resetPositions) return;

		float slice = 2 * Mathf.PI / (instancedAIs.Count + 1);
		float t;
		for (int i = 0; i < instancedAIs.Count; i++)
		{
			t = slice * (i + 1);
			Vector3 position = new Vector3(
				aiRadius * Mathf.Cos(t) + 0f,
				0,
				aiRadius * Mathf.Sin(t) + 0f
			);

			instancedAIs[i].transform.position = position;
		}


		continuousMove.transform.position = new Vector3(
			aiRadius * Mathf.Cos(0) + 0f,
			-3f,
			aiRadius * Mathf.Sin(0) + 0f
		);
		continuousMove.transform.rotation = Quaternion.Euler(0, -90, 0);


		slice = 2 * Mathf.PI / chairs.Count;
		for (int i = 0; i < chairs.Count; i++)
		{
			t = slice * i;
			Vector3 position = new Vector3(
				chairRadius * Mathf.Cos(t) + 0f,
				0,
				chairRadius * Mathf.Sin(t) + 0f
			);

			chairs[i].transform.position = position;
			chairs[i].transform.rotation = Quaternion.Euler(0, 90 - i * (slice * Mathf.Rad2Deg), 0);
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

		foreach (AI ai in instancedAIs) Destroy(ai.gameObject);
		instancedAIs.Clear();
		foreach (Chair chair in chairs) Destroy(chair.gameObject);
		chairs.Clear();

		startMenuRoot.SetActive(true);
		startMenuRoot.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
		startMenuRoot.transform.forward = Camera.main.transform.forward;

		ToggleMenu(true);
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

	public void ChangeVignette(bool useVignette)
	{
		continuousMove.useVignette = useVignette;
	}

	public void ChangeContinuous(bool useContinuous)
	{
		continuousMove.enabled = useContinuous;
	}

	public void PauseMenu()
	{
		bool on = !pauseMenuRoot.activeInHierarchy;
		pauseMenuRoot.SetActive(on);
		pauseMenuRoot.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
		pauseMenuRoot.transform.forward = Camera.main.transform.forward;

		if (inPause)
		{
			pauseStart = Time.time;
		}
		else
		{
			currentPhaseStart = Mathf.Clamp(currentPhaseStart + Time.time - pauseStart, 0f, 100f);
		}
		print("Pause ?");

		ToggleMenu(on);
	}

	private void ToggleMenu(bool on)
	{
		continuousMove.inMenu = on;
		foreach (var t in tpDetectors) t.inMenu = on;
	}

	void Update()
	{
		if (inPause) return;

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

			if (Keyboard.current.enterKey.wasPressedThisFrame && !playerHadFoundChair)
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
