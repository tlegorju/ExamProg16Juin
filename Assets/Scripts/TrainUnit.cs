using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleRailwaySystem;

/* Comportement d'un élément roulant

*/
public class TrainUnit : MonoBehaviour
{
    [SerializeField] GameObject[] wheels;
    [SerializeField] GameObject cabin;
    [SerializeField] GameObject[] bogies;

    [SerializeField] Transform[] attachPoint;
    public Transform FrontAttachPoint{get{return attachPoint[0];}}
    public Transform RearAttachPoint{get{return attachPoint[1];}}

    private RailManager rail;

    public float ratioWheelRotationSpeed=100;

    public float distanceTrainToUnit;
    public float distanceUnitToFrontBogie;
    public float distanceUnitToRearBogie;
    public float distanceToFrontAttach;
    public float distanceToRearAttach;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(RailManager rail)
    {
        this.rail = rail;

        distanceUnitToFrontBogie = Train.GetZDistance(transform.position.z, bogies[0].transform.position.z);
        distanceUnitToRearBogie = -Train.GetZDistance(transform.position.z, bogies[1].transform.position.z);

        distanceToFrontAttach = Train.GetZDistance(transform.position.z, FrontAttachPoint.position.z);
        distanceToRearAttach = Train.GetZDistance(transform.position.z, RearAttachPoint.position.z);

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    public void InitializeDistanceToParent(Train train)
    {
        Transform parent = train.transform;
        distanceTrainToUnit = Train.GetZDistance(parent.position, transform.position);
    }

    public void AlignUnitOnRail(float travelledDistance, float speed)
    {
        Vector3 tmpPos = Vector3.zero;
        Vector3 tmpNormal = Vector3.zero;
        Vector3 tmpTangent = Vector3.zero;
        int tmpCurrSegmentIndex = -1;

        float wagonTravelledDistance = (travelledDistance-distanceTrainToUnit);
        if(wagonTravelledDistance>rail.Length)
            wagonTravelledDistance-=rail.Length;
        else if(wagonTravelledDistance<0)
            wagonTravelledDistance+=rail.Length;

        rail.GetPositionNormalTangent(wagonTravelledDistance / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        transform.position=tmpPos;
        transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        rail.GetPositionNormalTangent((wagonTravelledDistance+distanceUnitToFrontBogie) / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        //bogies[0].transform.position = tmpPos;
        bogies[0].transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        rail.GetPositionNormalTangent((wagonTravelledDistance+distanceUnitToRearBogie) / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        //bogies[1].transform.position = tmpPos;
        bogies[1].transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        //AlignCabinOnBogies();
        OscillCabin(speed);
        TurnWheel(speed);
    }

    public void OscillCabin(float speed)
    {
        //Debug.Log(Mathf.Cos(speed*Time.time));
        cabin.transform.localRotation = Quaternion.Euler(0,0,speed*Mathf.Cos(10*Time.time));
    }

    public void TurnWheel(float trainSpeed)
    {
        for(int i=0; i<wheels.Length; i++)
        {
            wheels[i].transform.rotation *= Quaternion.Euler(trainSpeed*ratioWheelRotationSpeed*Time.deltaTime,0,0);
        }
    }


}
