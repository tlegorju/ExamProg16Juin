using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleRailwaySystem;

/* Comportement d'un train complet
*/
public class Train : MonoBehaviour
{
    [SerializeField] TrainUnit[] units;
    [SerializeField] RailManager rail;

    public float speed;
    public float acceleration=10;
    public const float MAX_SPEED=4;
    private float positionOnRail;
    private float speedToRailRatio = 0.001f;

    public float travelledDistance=0;

    Vector3 pos, normal, tangent;
    int segmentindex;

    // Start is called before the first frame update
    void Start()
    {
        InitializeJointConnection();
        units[0].rail = rail;
        //rail.GetPositionNormalTangent(0, out pos, out normal, out tangent, out segmentindex);
        //units[0].transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tmpPos = Vector3.zero;
        Vector3 tmpNormal = Vector3.zero;
        Vector3 tmpTangent = Vector3.zero;
        int tmpCurrSegmentIndex = -1;

        rail.GetPositionNormalTangent(travelledDistance / rail.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        /*transform.position = tmpPos;
        transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);*/

        for(int i=0; i<units.Length; i++)
        {
            units[i].AlignUnitOnRail(travelledDistance, speed);
        }

        travelledDistance += Time.deltaTime * speed;
        if(travelledDistance>rail.Length)
            travelledDistance-=rail.Length;
        else if(travelledDistance<0)
            travelledDistance+=rail.Length;
    }

    private void InitializeJointConnection()
    {
        if(units.Length < 2)
            return;
        units[0].RearAttachPoint.connectedBody = units[1].GetComponent<Rigidbody>();
        for(int i=1;i<units.Length-1; i++)
        {
            units[i].FrontAttachPoint.connectedBody = units[i-1].GetComponent<Rigidbody>();
            units[i].RearAttachPoint.connectedBody = units[i+1].GetComponent<Rigidbody>();
        }
        units[units.Length-1].FrontAttachPoint.connectedBody = units[units.Length-2].GetComponent<Rigidbody>();
    }
}
