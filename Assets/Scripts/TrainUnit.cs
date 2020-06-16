using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AlignUnitOnRail()
    {

    }

    public void AlignCabinOnBogies()
    {

    }

    public void OscillCabin()
    {

    }

    public void TurnWheel(float trainSpeed)
    {

    }


}
