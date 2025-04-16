using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayInteractDetector : MonoBehaviour, IXRInputButtonReader
{
	[SerializeField] private XRRayInteractor rayInteractor;
	[SerializeField] private bool left;

	private string thumbTag;

	private bool alreadyInteracted;

	private bool interacting;

	private void Awake()
	{
		thumbTag = (left ? "Left" : "Right") + "Thumb";
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag(thumbTag)) return;
		ConfirmSelect();
		alreadyInteracted = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag(thumbTag)) return;
		alreadyInteracted = false;
	}

	public void StartSelect()
	{
		print((left ? "Left" : "Right") + " startSelect");
		rayInteractor.gameObject.SetActive(true);
	}

	public void ConfirmSelect()
	{
		if(alreadyInteracted) return;
		print((left ? "Left" : "Right") + " confirmSelect");
		interacting = true;
	}

	public void CancelSelect()
	{
		print((left ? "Left" : "Right") + " stopSelect");
		rayInteractor.gameObject.SetActive(false);
	}

	public bool ReadIsPerformed() => true;

	public bool ReadWasPerformedThisFrame() => true;

	public bool ReadWasCompletedThisFrame() => true;

	public float ReadValue()
	{
		float value = interacting ? 1f : 0f;
		interacting = false;
		return value;
	}

	public bool TryReadValue(out float value)
	{
		value = interacting ? 1f : 0f;
		return true;
	}
}
