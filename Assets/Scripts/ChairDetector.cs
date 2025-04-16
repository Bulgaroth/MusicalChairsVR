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
		if (!chair.isUsed && GameManager.instance.currentState == GameManager.GameState.PHASE2)
		{
			chair.SetUsedBy(rigTr);
			if (GameManager.instance) GameManager.instance.playerHadFoundChair = true;
		}
	}

	public void ResetChair() => chair = null;
}
