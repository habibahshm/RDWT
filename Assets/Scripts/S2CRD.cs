using Redirection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S2CRD : MonoBehaviour
{
    GameManager gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    RDManager red_manager = GameObject.Find("Redirection Manager").GetComponent<RDManager>();

    Vector3 center;

    public void PickRedirectionTarget()
    {
        if (gameManager.center != null)
        {
            center = Utilities.FlattenedPos3D(gameManager.center);
            Vector3 userToCenter = center - red_manager.currPos;
            float bearingToCenter = Vector3.Angle(red_manager.currDir, userToCenter);
            float signedAngle = Utilities.GetSignedAngle(red_manager.currDir, userToCenter);

        }      
    }
}
