using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ChairDetector : MonoBehaviour
{
	private Chair chair;
	[SerializeField] private Transform rigTr;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Chair")) return;
		chair = other.GetComponent<Chair>();
		chair.SetUsedBy(rigTr);
	}

	public void ResetChair() => chair = null;
}
