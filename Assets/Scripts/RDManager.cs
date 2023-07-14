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

    [Tooltip("Baseline rotation applied")]
    [Range(0, 1)]
    public float BASELINE_ROT = 0.1F;

    [Tooltip("Threshold Angle in degrees to apply rotational dampening if using the original is unckecked")]
    [Range(0, 160)]
    public int AngleThreshDamp = 45;  // TIMOFEY: 45.0f;

    [Tooltip("Threshold distance within which dampening is applied")]
    [Range(0, 5)]
    public float DistThreshDamp = 1.25F;  // TIMOFEY: 45.0f;

    [Tooltip("Smoothing between rotations per frame")]
    [Range(0, 1)]
    public float SMOOTHING_FACTOR = 0.125f;

    [Tooltip("Use Original dampening method as proposed by razzaque or use the new one by Hodgson")]
    public bool original_dampening = true;

    [Tooltip("The game object that is being physically tracked (probably user's head)")]
    public Transform headTransform;

    public Transform XRTransform;

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

    [SerializeField] GameObject userDirVector;
    [SerializeField] GameObject dirTocenterVector;
    [SerializeField] TextMeshProUGUI text1;
    [SerializeField] TextMeshProUGUI text2;
    [SerializeField] TextMeshProUGUI text3;


    private const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160;
    private const float S2C_TEMP_TARGET_DISTANCE = 4;

    private const float MOVEMENT_THRESHOLD = 0.1f; // meters per second
    private const float ROTATION_THRESHOLD = 1f; // degrees per second
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second

    private bool no_tmptarget = true;
    private Vector3 tmp_target;       // the curr redirection target

    // Auxiliary Parameters
    private float rotationFromCurvatureGain; //Proposed curvature gain based on user speed
    private float rotationFromRotationGain; //Proposed rotation gain based on head's yaw
    private float lastRotationApplied = 0f;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }
    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        UpdateCurrentUserState();
        CalculateDelta();
        ApplyRedirection();
        UpdatePreviousUserState();

        if (gameManager.debug)
        {
            LineRenderer lineRenderer = dirTocenterVector.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, currPos);
            lineRenderer.SetPosition(1, center);

            LineRenderer lineRenderer2 = userDirVector.GetComponent<LineRenderer>();
            lineRenderer2.SetPosition(0, currPos);
            lineRenderer2.SetPosition(1, Utilities.FlattenedPos3D(headTransform.TransformPoint(Vector3.forward * 0.5f)));
        }
    }

    public void ApplyRedirection()
    {
        S2C_PickRedirectionTarget();

        rotationFromCurvatureGain = 0;

        if ((deltaPos.magnitude / Time.deltaTime) > MOVEMENT_THRESHOLD)
        {
            rotationFromCurvatureGain = Mathf.Rad2Deg * (deltaPos.magnitude / CURVATURE_RADIUS);
            rotationFromCurvatureGain = Mathf.Min(rotationFromCurvatureGain, CURVATURE_GAIN_CAP_DEGREES_PER_SECOND) * Time.deltaTime;
        }

        //Compute desired facing vector for redirection
        Vector3 desiredFacingDirection = Utilities.FlattenedPos3D(redirection_target) - currPos;
        int signOfAngle = (int)Mathf.Sign(Utilities.GetSignedAngle(currDir, desiredFacingDirection));
        int desiredSteeringDirection = (-1) * signOfAngle;

        //Compute proposed rotation gain
        rotationFromRotationGain = 0;

        if (Mathf.Abs(deltaDir) / Time.deltaTime >= ROTATION_THRESHOLD)
        {
            //Determine if we need to rotate with or against the user
            if (deltaDir * desiredSteeringDirection < 0)
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

        float rotationProposed = desiredSteeringDirection * Mathf.Max(rotationFromRotationGain, rotationFromCurvatureGain);

        //if user is stationary, apply baseline rotation
        if (Mathf.Approximately(rotationProposed, 0))
        {
            rotationProposed = desiredSteeringDirection * BASELINE_ROT * Time.deltaTime; 
        }

        //DAMPENING METHODS
      /*  float bearingToTarget = Vector3.Angle(currDir, desiredFacingDirection);
        if (original_dampening)
        {
            // Razzaque et al.
            rotationProposed *= Mathf.Sin(Mathf.Deg2Rad * bearingToTarget);

        }
        else
        {
            // Hodgson et al.
            if (bearingToTarget <= AngleThreshDamp)
                rotationProposed *= Mathf.Sin(Mathf.Deg2Rad * 90 * bearingToTarget / AngleThreshDamp);
        }*/


        // MAHDI: Linearly scaling the rotation when the distance is near zero
        if (desiredFacingDirection.magnitude <= DistThreshDamp)
        {
            rotationProposed *= desiredFacingDirection.magnitude / DistThreshDamp;
        }

        // Implement additional rotation with smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * lastRotationApplied + SMOOTHING_FACTOR * rotationProposed;
        lastRotationApplied = finalRotation;

        text3.SetText("Final Rotation: " + finalRotation);

        XRTransform.RotateAround(Utilities.FlattenedPos3D(headTransform.position), Vector3.up, finalRotation);
        center = Utilities.RotatePointAroundPivot(center, headTransform.position, new Vector3(0, finalRotation, 0));
    }

    public void S2C_PickRedirectionTarget()
    {
        if (center != null)
        {
            center = Utilities.FlattenedPos3D(center);
            Vector3 userToCenter = center - currPos;
            float bearingToCenter = Vector3.Angle(currDir, userToCenter);
            float signedAngle = Utilities.GetSignedAngle(currDir, userToCenter);

            //text2.SetText("Angle to center: " + bearingToCenter + "\n Signed angle: " + signedAngle);

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
        text1.SetText("delta pos: " + deltaPos);
        text2.SetText("Speed: " + (deltaPos.magnitude / Time.deltaTime));
    }





}
