using UnityEngine;

public class ArmSwingingManager : MonoBehaviour
{
	[SerializeField] private Transform[] handsTr;
	[SerializeField] private float speed; 
	[SerializeField] private float handSpeedDetectionThreshold; 

	private Transform camTr;
	private CharacterController ctrl;

	private Vector3 lastPlayerPos;
	private Vector3 currentPlayerPos;

	private readonly Vector3[] handsLastPos = new Vector3[2];
	private readonly Vector3[] handsCurrentPos = new Vector3[2];
	private Vector3 forwardDir;

	public bool LeftHandGestureOk { get; set; }
	public bool RightHandGestureOk { get; set; }

	private void Awake()
	{
		camTr = Camera.main.transform;
		ctrl = GetComponent<CharacterController>();
	}

	void Start()
	{
		lastPlayerPos = transform.position;
		for (int i = 0; i < handsTr.Length; ++i) handsLastPos[i] = handsTr[i].position;
	}

	void FixedUpdate()
	{
		print($"Left:{LeftHandGestureOk}");
		print($"Right:{RightHandGestureOk}");
		if(LeftHandGestureOk && RightHandGestureOk)
		{
			forwardDir = Vector3.ProjectOnPlane(camTr.forward, Vector3.up).normalized;

			currentPlayerPos = transform.position;
			float playerDstMoved = Vector3.Distance(currentPlayerPos, lastPlayerPos);

			float[] handsDstMoved = new float[handsTr.Length];
			for (int i = 0; i < handsTr.Length; ++i)
			{
				handsCurrentPos[i] = handsTr[i].position;
				handsDstMoved[i] = Vector3.Distance(handsLastPos[i], handsCurrentPos[i]);
			}

			float handSpeed = (handsDstMoved[0] - playerDstMoved) + (handsDstMoved[1] - playerDstMoved);

			print(handSpeed);
			if (Time.timeSinceLevelLoad > 1f && handSpeed > handSpeedDetectionThreshold)
				ctrl.Move(handSpeed * speed * Time.fixedDeltaTime * forwardDir);
		}

		lastPlayerPos = currentPlayerPos;
		for (int i = 0; i < handsTr.Length; ++i) handsLastPos[i] = handsCurrentPos[i];
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, forwardDir * speed);
	}
}
