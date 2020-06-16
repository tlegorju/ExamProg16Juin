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
        InitializeWagons();
        
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

    private void InitializeWagons()
    {
        units[0].Initialize(rail);
        units[0].InitializeDistanceToParent(this);

        if(units.Length < 2)
            return;
        
        for(int i=1;i<units.Length-1; i++)
        {
            units[i].Initialize(rail);
            units[i].transform.position = units[i-1].transform.position - new Vector3(0,0,units[i-1].distanceToRearAttach + units[i].distanceToFrontAttach);
            units[i].InitializeDistanceToParent(this);
        }
        units[units.Length-1].Initialize(rail);
        units[units.Length-1].transform.position = units[units.Length-2].transform.position - new Vector3(0,0,units[units.Length-2].distanceToRearAttach + units[units.Length-1].distanceToFrontAttach);
        units[units.Length-1].InitializeDistanceToParent(this);
        Debug.Log(units[units.Length-2].transform.position - new Vector3(0,0,units[units.Length-2].distanceToRearAttach + units[units.Length-1].distanceToFrontAttach));
    }
    
    public static float GetZDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(b.z-a.z);
    }

    public static float GetZDistance(float a, float b)
    {
        return Mathf.Abs(b-a);
    }
}
