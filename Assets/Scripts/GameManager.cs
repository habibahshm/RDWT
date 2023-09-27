using Redirection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject UI;
  
    RDManager red_manager;
    bool configured;
    GameObject red_target;
    PathTrail pathTrail;
    

    [HideInInspector] public bool debug = false;
    [HideInInspector] public GameObject trackedArea;

    bool prev_state_touch = false;
    bool paused = false;
    bool prev_state_pause = false;
    
    [SerializeField] GameObject wallMarker;
    [SerializeField] GameObject dirMarker;
    [SerializeField] GameObject realPlanePrefab;
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;

  /*  [SerializeField] GameObject planeDir;
    [SerializeField] GameObject XRForward;*/


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
        
       /* if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            ResetPos();
            
        }*/

    
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
                //red_target.SetActive(debug);   
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
        //trackedArea.transform.position = red_manager.center.transform.position + new Vector3(0, 0.05f, 0);
       // trackedArea.transform.localRotation = red_manager.center.transform.localRotation;

       /* text1.SetText("center rot: " + red_manager.center.transform.localRotation.eulerAngles.ToString());
        text2.SetText("area rot: " + trackedArea.transform.localRotation.eulerAngles.ToString());*/
       
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

        text1.SetText("dim: " + boundrydim);

        Vector3 p1 = boundaryPoints[0];
        Vector3 p2 = boundaryPoints[1];
        Vector3 p3 = boundaryPoints[2];
        Vector3 p4 = boundaryPoints[3];

        /*Instantiate(wallMarker, p1, Quaternion.identity);
        Instantiate(wallMarker, p2, Quaternion.identity);*/
        

        Vector3 p1Diff = p3 - p1;
        Vector3 p2Diff = p4 - p2;
        Vector3 center; // Center of the tracked physical area
        if (LineIntersection(out center, p1, p1Diff, p2, p2Diff))
        {
            // if OVRRig/XR Origin is not aligned with world origin, then must shift and rotate by the diffrence.
            center = Quaternion.Euler(0, red_manager.XRTransform.rotation.eulerAngles.y, 0) * center;
            center += red_manager.XRTransform.position; 
            center = Utilities.FlattenedPos3D(center);

            if(red_manager.center == null)
                red_manager.center = new GameObject();
            red_manager.center.transform.position = center;
            red_manager.center.transform.localRotation = red_manager.startPos.transform.localRotation;

            if (red_target == null)
                red_target = Instantiate(wallMarker, center, Quaternion.identity);
            else
                red_target.transform.position = center;

           // red_target.transform.position += new Vector3(0, 0.3f, 0);

            Vector3 forwardDir = (p1 - p4);
            float angle = Vector3.Angle(Vector3.forward, Utilities.FlattenedDir3D(forwardDir));
            if (forwardDir.x < 0.0f)
            {
                angle = -angle;
                angle +=  360;
            }

            text2.SetText(red_target.transform.position.ToString());

            /*  forwardDir = Quaternion.Euler(0, red_manager.XRTransform.rotation.eulerAngles.y, 0) * forwardDir;
              forwardDir += red_manager.XRTransform.position;


              LineRenderer lineRenderer = planeDir.GetComponent<LineRenderer>();
              lineRenderer.SetPosition(0, red_manager.XRTransform.position);
              lineRenderer.SetPosition(1, forwardDir);

              LineRenderer lineRenderer2 = XRForward.GetComponent<LineRenderer>();
              lineRenderer2.SetPosition(0, red_manager.XRTransform.position);
              lineRenderer2.SetPosition(1, Utilities.FlattenedPos3D(red_manager.XRTransform.TransformPoint(Vector3.forward)));*/

            //float angle = Vector3.Angle(red_manager.XRTransform.forward, forwardDir);
            //angle = p1.x > 0 ? -angle : angle;
            /* float angle = Vector3.Angle(red_manager.XRTransform.forward, forwardDir);
             angle = forwardDir.x > 0.0 ? angle : -angle;*/

            if (trackedArea == null)
            {
                trackedArea = Instantiate(realPlanePrefab, center + new Vector3(0, 0.05f, 0), Quaternion.identity);
                trackedArea.transform.localScale = new Vector3(boundrydim.x / 10, 1, boundrydim.z / 10);
               
               trackedArea.transform.Rotate(0, angle, 0);
              
                
            }

           
        }

        //Instantiate(dirMarker, center, trackedArea.transform.localRotation);

        pathTrail.ClearTrail(PathTrail.REAL_TRAIL_NAME);
        pathTrail.ClearTrail(PathTrail.VIRTUAL_TRAIL_NAME);

        pathTrail.BeginTrailDrawing();

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
