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

    [SerializeField] CharacterJoint[] attachPoint;
    public CharacterJoint FrontAttachPoint{get{return attachPoint[0];}}
    public CharacterJoint RearAttachPoint{get{return attachPoint[1];}}

    public RailManager rail;

    public float ratioWheelRotationSpeed=10;

    public float distanceTrainToUnit;
    public float distanceUnitToFrontBogie;
    public float distanceUnitToRearBogie;

    // Start is called before the first frame update
    void Start()
    {
        Transform parent = GetComponentInParent<Train>().transform;
        distanceTrainToUnit = Vector3.Distance(new Vector3(0,0,parent.position.z), new Vector3(0,0,transform.position.z));
        distanceUnitToFrontBogie = Vector3.Distance(new Vector3(0,0,transform.position.z), new Vector3(0,0,bogies[0].transform.position.z));
        distanceUnitToRearBogie = -Vector3.Distance(new Vector3(0,0,transform.position.z), new Vector3(0,0,bogies[1].transform.position.z));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AlignUnitOnRail(float travelledDistance, float speed)
    {
        Vector3 tmpPos = Vector3.zero;
        Vector3 tmpNormal = Vector3.zero;
        Vector3 tmpTangent = Vector3.zero;
        int tmpCurrSegmentIndex = -1;

        rail.GetPositionNormalTangent((travelledDistance-distanceTrainToUnit) / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        transform.position=tmpPos;
        transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        rail.GetPositionNormalTangent((travelledDistance-distanceTrainToUnit+distanceUnitToFrontBogie) / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        //bogies[0].transform.position = tmpPos;
        bogies[0].transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        rail.GetPositionNormalTangent((travelledDistance-distanceTrainToUnit+distanceUnitToRearBogie) / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        //bogies[1].transform.position = tmpPos;
        bogies[1].transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        AlignCabinOnBogies();
        // OscillCabin(speed);
        // TurnWheel(speed);
    }

    public void AlignCabinOnBogies()
    {
        //cabin.transform.rotation = Quaternion.Slerp(bogies[0].transform.rotation, bogies[1].transform.rotation,0.5f);

    }

    public void OscillCabin(float speed)
    {
        cabin.transform.rotation *= Quaternion.Euler(0,0,10*Mathf.Cos(speed*Time.deltaTime));
    }

    public void TurnWheel(float trainSpeed)
    {
        for(int i=0; i<wheels.Length; i++)
        {
            wheels[i].transform.rotation *= Quaternion.Euler(trainSpeed*ratioWheelRotationSpeed*Time.deltaTime,0,0);
        }
    }


}
