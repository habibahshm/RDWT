using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject UI;
  
    RDManager red_manager;
    bool configured;
    GameObject red_target;
    PathTrail pathTrail;
    //GameObject XROrigin;
    GameObject trackedArea;

    [HideInInspector] public bool debug = false;
    bool prev_state_touch = false;
    bool paused = false;
    bool prev_state_pause = false;
    
    [SerializeField] GameObject wallMarker;
    [SerializeField] GameObject dirMarker;
    [SerializeField] GameObject realPlanePrefab;
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;

    void Start()
    {
        red_manager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();
        pathTrail = GameObject.Find("Redirection Manager").GetComponent<PathTrail>();

        //Check if the boundary is configured
        configured = OVRManager.boundary.GetConfigured();
        if (configured)
        {
            ResetPos();
        }

    }

    void Update()
    {
        
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            ResetPos();
            
        }

    
        bool button_pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        if (button_pressed != prev_state_pause)
        {
            if (button_pressed)
            {
                
                Time.timeScale = paused ? 1 : 0;
                paused = !paused;
                
            }
            prev_state_pause = button_pressed;
        }

        bool secondary_t = OVRInput.Get(OVRInput.Touch.Two);
        if (secondary_t != prev_state_touch)
        {
            if (secondary_t)
            {
                debug = !debug;
                UI.SetActive(debug);
                red_target.SetActive(debug);   
            }
            prev_state_touch = secondary_t;
        }


    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (red_manager.redirection_target != null)
        {
            red_target.transform.position = red_manager.redirection_target;
        }
        trackedArea.transform.position = red_manager.center.transform.position + new Vector3(0, 0.05f, 0);
        trackedArea.transform.localRotation = red_manager.center.transform.rotation;

    }

    public void ResetPos()
    {
        float angleY = red_manager.startPos.rotation.eulerAngles.y - red_manager.headTransform.rotation.eulerAngles.y;
        red_manager.XRTransform.Rotate(0, angleY, 0);
        Vector3 distDiff = red_manager.startPos.position - red_manager.headTransform.position;
        red_manager.XRTransform.transform.position += new Vector3(distDiff.x, 0, distDiff.z);

        //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
        Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
        Vector3 boundrydim = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);

        //text1.SetText("dim: " + boundrydim);

        Vector3 p1 = boundaryPoints[0];
        Vector3 p2 = boundaryPoints[1];
        Vector3 p3 = boundaryPoints[2];
        Vector3 p4 = boundaryPoints[3];

        Vector3 p1Diff = p3 - p1;
        Vector3 p2Diff = p4 - p2;
        Vector3 center;
        if (LineIntersection(out center, p1, p1Diff, p2, p2Diff))
        {
            center += red_manager.XRTransform.position; // if OVRRig not aligned with world origin, then must shift by the diffrence.
            red_manager.center = new GameObject();
            red_manager.center.transform.position = center;
            red_manager.center.transform.localRotation = red_manager.startPos.transform.rotation;
            if(red_target == null)
                red_target = Instantiate(wallMarker, center, Quaternion.identity);

            if (trackedArea == null)
            {
                trackedArea = Instantiate(realPlanePrefab, center + new Vector3(0, 0.05f, 0), Quaternion.identity);
                trackedArea.transform.localScale = new Vector3(boundrydim.x / 10, 1, boundrydim.z / 10);
            }
           
        }

       /* if(XROrigin == null)
            XROrigin = Instantiate(dirMarker, red_manager.XRTransform.position + new Vector3(0, 0.1f, 0), red_manager.XRTransform.rotation);
        text1.SetText("XROrigin: " + XROrigin.transform.position.ToString());*/
    }

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

}
