using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject wallMarker;
    [SerializeField] TextMeshProUGUI debugUI;

    OVRCameraRig overCameraRig;
    private Vector3 pos;
    private Vector3 orient;


    void Start()
    {
        //Get the user's current position and rotation in world coordinates, OVRCameraRig transform must be reset to align with wirld coordinates
        overCameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
        pos = overCameraRig.centerEyeAnchor.position;
        orient = overCameraRig.centerEyeAnchor.eulerAngles;

        //need to wait a while to get the right measurments, if we get them immediatley the values are zero.
        StartCoroutine(LateStart(0.1f));

        

        //Check if the boundary is configured
        bool configured = OVRManager.boundary.GetConfigured();
        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
            Vector3 boundrydim  = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
            debugUI.text = debugUI.text + "dim: "+ boundrydim.ToString() + "\n";

            //Generate a bunch of tall thin cubes to mark the outline
            foreach (Vector3 pos in boundaryPoints)
            {      
                debugUI.text = debugUI.text + pos.ToString() + "\n";
                Instantiate(wallMarker, pos, Quaternion.identity);
}
        }
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }



    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            SceneManager.LoadScene(0);
        }

        debugUI.text = debugUI.text + "user pos: " + pos.ToString() + "\n";
    }
}
