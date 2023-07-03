using Redirection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class RDManager : MonoBehaviour
{
    [Tooltip("Maximum rotation gain applied")]
    [Range(1F, 2F)]
    public float MAX_ROT_GAIN = 1.3F;

    [Tooltip("Minimum rotation gain applied")]
    [Range(0.1F, 1F)]
    public float MIN_ROT_GAIN = 0.85F;

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
    public Vector3 currPos, prevPos, currDir, prevDir; //cur pos of user w.r.t the OVR rig which is aligned with the (0,0,0)

    [HideInInspector]
    public Vector3 deltaPos;
    [HideInInspector]
    public float deltaDir;

    enum RotationObject { UserHead, Env };

    [SerializeField] GameObject userDirVector;
    [SerializeField] GameObject dirTocenterVector;
    [SerializeField] RotationObject ObjectToRotate;

    private const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160;
    private const float S2C_TEMP_TARGET_DISTANCE = 4;
   
    private bool no_tmptarget = true;
    private Vector3 tmp_target;       // the curr redirection target

    private const float MOVEMENT_THRESHOLD = 0.2f; // meters per second
    private const float ROTATION_THRESHOLD = 1.5f; // degrees per second
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second

    // Auxiliary Parameters
    private float rotationFromCurvatureGain; //Proposed curvature gain based on user speed
    private float rotationFromRotationGain; //Proposed rotation gain based on head's yaw
    private float lastRotationApplied = 0f;

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        UpdateCurrentUserState();
        CalculateDelta();

        LineRenderer lineRenderer = dirTocenterVector.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, currPos);
        lineRenderer.SetPosition(1, center);

        LineRenderer lineRenderer2 = userDirVector.GetComponent<LineRenderer>();
        lineRenderer2.SetPosition(0, currPos);
        lineRenderer2.SetPosition(1, Utilities.FlattenedPos3D(headTransform.TransformPoint(Vector3.forward * 0.5f)));

        ApplyRedirection();

        UpdatePreviousUserState();

    }

    public void ApplyRedirection()
    {
        S2C_PickRedirectionTarget();

        rotationFromCurvatureGain = 0;

        if((deltaPos.magnitude/ Time.deltaTime) > MOVEMENT_THRESHOLD)
        {
            rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / CURVATURE_RADIUS);
            rotationFromCurvatureGain = Mathf.Min(rotationFromCurvatureGain, CURVATURE_GAIN_CAP_DEGREES_PER_SECOND)* Time.deltaTime;
        }

        //Compute desired facing vector for redirection
        Vector3 desiredFacingDirection = Utilities.FlattenedPos3D(redirection_target) - currPos;
        int signOfAngle = (int)Mathf.Sign(Utilities.GetSignedAngle(currDir, desiredFacingDirection));
        int desiredSteeringDirection = ObjectToRotate == RotationObject.UserHead ? (-1) * signOfAngle : signOfAngle;

        //Compute proposed rotation gain
        rotationFromRotationGain = 0;

        if (Mathf.Abs(deltaDir) / Time.deltaTime >= ROTATION_THRESHOLD)
        {
            //Determine if we need to rotate with or against the user
            if ((deltaDir * desiredSteeringDirection < 0 && ObjectToRotate == RotationObject.UserHead) || (deltaDir * desiredSteeringDirection > 0 && ObjectToRotate == RotationObject.Env))
            {
                //Rotating against the user
                rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * MIN_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND) * Time.deltaTime; 
            }
            else
            {  
                //Rotating with the user
                rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * MAX_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND) * Time.deltaTime;
            }
        }

        


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
                    tmp_target = currPos + S2C_TEMP_TARGET_DISTANCE * (Quaternion.Euler(0, (int)Mathf.Sign(signedAngle) * 90, 0) * currDir);
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

    void UpdateCurrentUserState()
    {
        currPos = Utilities.FlattenedPos3D(headTransform.position);
        currDir = Utilities.FlattenedDir3D(headTransform.forward);
    }

    void UpdatePreviousUserState()
    {
        prevPos = Utilities.FlattenedPos3D(headTransform.position);
        prevDir = Utilities.FlattenedDir3D(headTransform.forward);
    }

    void CalculateDelta()
    {
        deltaPos = currPos - prevPos;
        deltaDir = Utilities.GetSignedAngle(prevDir, currDir);
    }
}
