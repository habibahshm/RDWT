using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject wallMarker;
    [SerializeField] GameObject UI;
    [SerializeField] TextMeshProUGUI debugUI;
    [SerializeField] TextMeshProUGUI posUI;

    OVRCameraRig overCameraRig;
    private Vector3 pos;
    bool UIactive = false;
    bool prev_state_touch = false;

    public static bool LineIntersection(out Vector3 intersection, Vector3 linePoint1,
    Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2)
                    / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    void Start()
    {
        //Get the user's current position and rotation in world coordinates, OVRCameraRig transform must be reset to align with wirld coordinates
        overCameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
        
        //need to wait a while to get the right measurments, if we get them immediatley the values are zero.
        StartCoroutine(LateStart(0.1f));

        //Check if the boundary is configured
        bool configured = OVRManager.boundary.GetConfigured();
        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
            Vector3 boundrydim  = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
            debugUI.text = "dim: "+ boundrydim.ToString() + "\n";

            //Generate a bunch of tall thin cubes to mark the outline
            foreach (Vector3 pos in boundaryPoints)
            {      
                //debugUI.text = debugUI.text + pos.ToString() + "\n";
                Instantiate(wallMarker, pos, Quaternion.identity);
            }

            Vector3 p1 = boundaryPoints[0];
            Vector3 p2 = boundaryPoints[1];
            Vector3 p3 = boundaryPoints[2];
            Vector3 p4 = boundaryPoints[3];

            Vector3 center;
            Vector3 p1Diff = p3 - p1;
            Vector3 p2Diff = p4 - p2;
            if (LineIntersection(out center, p1, p1Diff, p2, p2Diff))
            {
                Instantiate(wallMarker, center, Quaternion.identity);
            }
        }
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }


    void Update()
    {
        OVRInput.Update();
        pos = overCameraRig.centerEyeAnchor.position;

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            SceneManager.LoadScene(0);
        }

        bool secondary_t = OVRInput.Get(OVRInput.Touch.Two);
        
        if (secondary_t != prev_state_touch)
        {
            if (secondary_t)
            {
                UIactive = !UIactive;
                UI.SetActive(UIactive);
            }
            prev_state_touch = secondary_t;
        }
       
        if (UIactive)
        {
            posUI.text = "user pos: " + pos.ToString() + "\n";
        }

    }
}
