using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleRailwaySystem;

public class SteerTrainOnRail : MonoBehaviour
{
    [SerializeField] RailManager m_RailManager=null;
    [SerializeField] float m_Speed=1f;

    float m_TravelledDistance=0;

    Transform m_Transform;

    private void Awake()
    {
        m_Transform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tmpPos = Vector3.zero;
        Vector3 tmpNormal = Vector3.zero;
        Vector3 tmpTangent = Vector3.zero;
        int tmpCurrSegmentIndex = -1;

        m_RailManager.GetPositionNormalTangent(m_TravelledDistance / m_RailManager.Length, out tmpPos, out tmpNormal, out tmpTangent, out tmpCurrSegmentIndex);
        m_Transform.position = tmpPos;
        m_Transform.rotation = Quaternion.LookRotation(tmpTangent, tmpNormal);

        m_TravelledDistance += Time.deltaTime * m_Speed;
    }
}
