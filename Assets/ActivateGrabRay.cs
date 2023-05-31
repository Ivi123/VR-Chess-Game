using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ActivateGrabRay : MonoBehaviour
{
    public GameObject leftGrabRayObject;
    public GameObject rightGrabRayObject;


    public XRDirectInteractor leftDirectInteractor;
    public XRDirectInteractor rightDirectInteractor;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        leftGrabRayObject.SetActive(leftDirectInteractor.interactablesSelected.Count == 0);
        rightGrabRayObject.SetActive(rightDirectInteractor.interactablesSelected.Count == 0);

    }
}
