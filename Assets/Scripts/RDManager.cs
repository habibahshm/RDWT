using Redirection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class RDManager : MonoBehaviour
{

    [Tooltip("Maximum translation gain applied")]
    [Range(0, 5)]
    public float MAX_TRANS_GAIN = 0.26F;

    [Tooltip("Minimum translation gain applied")]
    [Range(-0.99F, 0)]
    public float MIN_TRANS_GAIN = -0.14F;

    [Tooltip("Maximum rotation gain applied")]
    [Range(0, 5)]
    public float MAX_ROT_GAIN = 0.49F;

    [Tooltip("Minimum rotation gain applied")]
    [Range(-0.99F, 0)]
    public float MIN_ROT_GAIN = -0.2F;

    [Tooltip("Radius applied by curvature gain")]
    [Range(1, 23)]
    public float CURVATURE_RADIUS = 7.5F;

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;

    public TextMeshProUGUI debugUI;

    [HideInInspector]
    public Vector3 redirection_target;

    [HideInInspector] 
    public Vector3 center; // center of the tracking area

    [HideInInspector]
    public Vector3 currPos; //cur pos of user w.r.t the OVR rig which is aligned with the (0,0,0)

    [HideInInspector]
    public Vector3 currDir; // cur forward direction of the user

    [SerializeField] GameObject userDir;
    [SerializeField] GameObject dirTocenter;

    private const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160;
    private const float S2C_TEMP_TARGET_DISTANCE = 4;
   
    private bool no_tmptarget = true;
    private Vector3 tmp_target;       // the curr redirection target


    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        currPos = Utilities.FlattenedPos3D(headTransform.position);
        currDir = Utilities.FlattenedDir3D(headTransform.forward);

        LineRenderer lineRenderer = dirTocenter.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, currPos);
        lineRenderer.SetPosition(1, center);

        LineRenderer lineRenderer2 = userDir.GetComponent<LineRenderer>();
        lineRenderer2.SetPosition(0, currPos);
        lineRenderer2.SetPosition(1, Utilities.FlattenedPos3D(headTransform.TransformPoint(Vector3.forward * 0.5f)));
        
        S2C_PickRedirectionTarget();

    }

    public void S2C_PickRedirectionTarget()
    {
        if (center != null)
        {
            center = Utilities.FlattenedPos3D(center);
            Vector3 userToCenter = center - currPos;
            float bearingToCenter = Vector3.Angle(currDir, userToCenter);
            float signedAngle = Utilities.GetSignedAngle(currDir, userToCenter);

            debugUI.SetText("Angle to center: " + bearingToCenter + "\n Signed angle: " + signedAngle);

            if(bearingToCenter >= S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE)
            {
                if (no_tmptarget)
                {
                    tmp_target = currPos + S2C_TEMP_TARGET_DISTANCE * (Quaternion.Euler(0, Utilities.GetSignOfAngle(currDir, userToCenter) * 90, 0) * currDir);
                    no_tmptarget = false;
                }
                redirection_target = tmp_target;
            }
            else
            {
                redirection_target = center;
                no_tmptarget = true;
            }

        }
    }
}
