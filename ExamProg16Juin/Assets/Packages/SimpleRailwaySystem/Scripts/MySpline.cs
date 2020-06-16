using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeometryTools;

public class MySpline
{
    List<Vector3> m_CtrlPts = null;
    List<Vector3> m_CtrlNormals = null;
    bool m_HasNormals { get { return m_CtrlNormals != null; } }

    List<Vector3> m_Pts = null;
    List<Vector3> m_Normals = null;
    List<Vector3> m_Tangents = null;

    public int NPts { get { return m_Pts == null ? -1 : m_Pts.Count; } }
    float m_PtsDensity;
    bool m_Closed;
    float m_Length;
    public float Length { get { return m_Length; } }

    bool m_IsValid = false;
    public bool IsValid { get { return m_IsValid; } }

    Vector3 ComputeBezierPos(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
            (-a + 3f * b - 3f * c + d) * (t * t * t)
            + (2f * a - 5f * b + 4f * c - d) * (t * t)
            + (-a + c) * t
            + 2f * b));
    }

    Vector3 ComputeBezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return (.5f * (
            3 * (-a + 3f * b - 3f * c + d) * (t * t)
            + 2 * (2f * a - 5f * b + 4f * c - d) * t
            + (-a + c)
            )).normalized;
    }

    public bool GetPositionNormalTangent(int index, out Vector3 pos, out Vector3 normal, out Vector3 tangent)
    {
        normal = Vector3.zero;
        pos = Vector3.zero;
        tangent = Vector3.zero;
        if (IsValid)
        {
            int tmpIndex = Mathf.Clamp(index, 0, m_Pts.Count - 1);
            pos = m_Pts[tmpIndex];
            if (m_HasNormals) normal = m_Normals[tmpIndex];
            tangent = m_Tangents[tmpIndex];
            return true;
        }
        return false;
    }

    public bool GetPositionNormalTangent(float t, out Vector3 pos, out Vector3 normal, out Vector3 tangent)
    {
        int segmentIndex;
        return GetPositionNormalTangent(t, out pos, out normal, out tangent, out segmentIndex);
    }

    public bool GetPositionNormalTangent(float t, out Vector3 pos, out Vector3 normal, out Vector3 tangent, out int segmentIndex)
    {
        normal = Vector3.zero;
        tangent = Vector3.zero;
        pos = Vector3.zero;
        segmentIndex = -1;
        if (IsValid)
        {
            t = Mathf.Clamp01(t);

            if (t == 0)
            {
                pos = m_Pts[0];
                if (m_HasNormals) normal = m_Normals[0];
                tangent = m_Tangents[0];
                segmentIndex = 0;
                return true;
            }
            if (t == 1)
            {
                segmentIndex = m_Closed ? m_Pts.Count - 1 : m_Pts.Count - 2;
                pos = m_Closed ? m_Pts[0] : m_Pts[m_Pts.Count - 1];
                if (m_HasNormals) normal = m_Closed ? m_Normals[0] : m_Normals[m_Pts.Count - 1];
                tangent = m_Closed ? m_Tangents[0] : m_Tangents[m_Pts.Count - 1];
                return true;
            }

            float targetDistance = t * m_Length;

            float tmpDistance = 0;

            int indexMax = m_Closed ? m_Pts.Count + 1 : m_Pts.Count;
            int index = 0;

            Vector3 pt0 = Vector3.zero;
            Vector3 pt1 = Vector3.zero;
            Vector3 normal0 = Vector3.zero;
            Vector3 normal1 = Vector3.zero;
            Vector3 tangent0 = Vector3.zero;
            Vector3 tangent1 = Vector3.zero;

            float distP0ToP1 = 0;
            do
            {
                tmpDistance += distP0ToP1;
                segmentIndex = index;

                pt0 = m_Pts[index % m_Pts.Count];
                if (m_HasNormals) normal0 = m_Normals[index % m_Pts.Count];
                tangent0 = m_Tangents[index % m_Pts.Count];

                pt1 = m_Pts[(index + 1) % m_Pts.Count];
                if (m_HasNormals) normal1 = m_Normals[(index + 1) % m_Pts.Count];
                tangent1 = m_Tangents[(index + 1) % m_Pts.Count];

                distP0ToP1 = Vector3.Distance(pt0, pt1);
            } while (tmpDistance + distP0ToP1 < targetDistance && (index++) < indexMax);

            float k = (targetDistance - tmpDistance) / distP0ToP1;
            pos = Vector3.Lerp(pt0, pt1, k);
            if (m_HasNormals) normal = Vector3.Slerp(normal0, normal1, k);
            tangent = Vector3.Slerp(tangent0, tangent1, k);

            return true;
        }

        return false;
    }


    public Vector3 this[int index]
    {
        get
        {
            Vector3 pos, normal, tangent;
            GetPositionNormalTangent(index, out pos, out normal, out tangent);
            return pos;
        }
    }

    public Vector3 this[float t]
    { // t between 0 ad 1
        get
        {
            Vector3 pos, normal, tangent;
            int indexSegment;
            GetPositionNormalTangent(t, out pos, out normal, out tangent, out indexSegment);
            return pos;
        }
    }

    public MySpline(List<Transform> ctrlPts, List<Vector3> ctrlNormals, bool closed, float ptsDensity)
        : this(ctrlPts.Select(item => item.position).ToList(), ctrlNormals, closed, ptsDensity)
    {
    }

    public MySpline(List<Vector3> ctrlPts, List<Vector3> ctrlNormals, bool closed, float ptsDensity)
    {
        //
        if ((!closed && ctrlPts.Count < 4)
            || (closed && ctrlPts.Count < 3)
            || (ctrlNormals != null && ctrlNormals.Count != ctrlPts.Count))
            return;


        m_Closed = closed;
        m_CtrlPts = new List<Vector3>(ctrlPts);
        if (ctrlNormals != null) m_CtrlNormals = new List<Vector3>(ctrlNormals);
        m_PtsDensity = ptsDensity;

        if (closed)
        {
            Vector3 ctrlPt0 = ctrlPts[0];
            Vector3 ctrlPt1 = ctrlPts[1];
            Vector3 ctrlPtNMinus1 = ctrlPts[ctrlPts.Count - 1];
            m_CtrlPts.Add(ctrlPt0);
            m_CtrlPts.Add(ctrlPt1);
            m_CtrlPts.Insert(0, ctrlPtNMinus1);

            if (m_HasNormals)
            {
                Vector3 ctrlNormal0 = ctrlNormals[0];
                Vector3 ctrlNormal1 = ctrlNormals[1];
                Vector3 ctrlNormalNMinus1 = ctrlNormals[ctrlNormals.Count - 1];
                m_CtrlNormals.Add(ctrlNormal0);
                m_CtrlNormals.Add(ctrlNormal1);
                m_CtrlNormals.Insert(0, ctrlNormalNMinus1);
            }
        }

        m_Pts = new List<Vector3>();
        m_Normals = new List<Vector3>();
        m_Tangents = new List<Vector3>();
        m_Length = 0;
        Vector3 prevPt = Vector3.zero;
        for (int i = 1; i < m_CtrlPts.Count - 2; i++)
        {
            Vector3 P0 = m_CtrlPts[i - 1];
            Vector3 P1 = m_CtrlPts[i];
            Vector3 P2 = m_CtrlPts[i + 1];
            Vector3 P3 = m_CtrlPts[i + 2];
            float distance = Vector3.Distance(P1, P2);
            int nPts = (int)Mathf.Max(3, distance * m_PtsDensity);

            Vector3 normal1 = Vector3.zero;
            Vector3 normal2 = Vector3.zero;
            if (m_HasNormals)
            {
                normal1 = m_CtrlNormals[i].normalized;
                normal2 = m_CtrlNormals[i + 1].normalized;
            }

            for (int j = 0; j < nPts; j++)
            {
                int nPtsDenominator = (i == m_CtrlPts.Count - 3) && !m_Closed ? nPts - 1 : nPts;
                float k = (float)j / nPtsDenominator;
                Vector3 pt = ComputeBezierPos(P0, P1, P2, P3, k);
                m_Pts.Add(pt);

                m_Tangents.Add(ComputeBezierTangent(P0, P1, P2, P3, k));
                if (m_HasNormals) m_Normals.Add(Vector3.Slerp(normal1, normal2, k));

                if (!(i == 1 && j == 0)) m_Length += Vector3.Distance(prevPt, pt);
                prevPt = pt;
            }
        }

        if (closed) m_Length += Vector3.Distance(m_Pts[0], m_Pts[m_Pts.Count - 1]);

        m_IsValid = true;
    }

    public bool GetSphereSplineIntersection(Vector3 sphCenter, float sphRadius, int startIndex, int searchDir,
        out Vector3 pos, out Vector3 normal, out Vector3 tangent, out int segmentIndex)
    { // searchDir est égal à 1 ou -1
        searchDir = (int)Mathf.Sign(searchDir); // au cas où searchDir serait autre chose que 1 ou -1

        pos = Vector3.zero;
        normal = Vector3.zero;
        tangent = Vector3.zero;
        segmentIndex = -1;

        int n = 0;
        while (n < NPts)
        {
            int tmpIndex1 = startIndex + n * searchDir;
            while (tmpIndex1 < 0) tmpIndex1 += NPts;
            int tmpIndex2 = startIndex + (n + 1) * searchDir;
            while (tmpIndex2 < 0) tmpIndex2 += NPts;

            Vector3 pt1 = m_Pts[tmpIndex1];
            Vector3 pt2 = m_Pts[tmpIndex2];

            Vector3 normal1 = Vector3.zero, normal2 = Vector3.zero;
            if (m_HasNormals)
            {
                normal1 = m_Normals[tmpIndex1];
                normal2 = m_Normals[tmpIndex2];
            }

            Vector3 tangent1 = Vector3.zero, tangent2 = Vector3.zero;
            tangent1 = m_Tangents[tmpIndex1];
            tangent2 = m_Tangents[tmpIndex2];

            Vector3 intersectionPt = Vector3.zero;
            if (Intersections.OrientedSegmentSphereIntersection_3D(pt1, pt2, sphCenter, sphRadius, out intersectionPt))
            {
                pos = intersectionPt;
                float k = Vector3.Distance(intersectionPt, pt1) / Vector3.Distance(pt1, pt2);
                if (m_HasNormals) normal = Vector3.Slerp(normal1, normal2, k);
                tangent = Vector3.Slerp(tangent1, tangent2, k).normalized;

                segmentIndex = searchDir > 0 ? tmpIndex1 : tmpIndex2;
                return true;
            }
            n++;
        }
        return false;
    }


    public void DrawGizmos(Color color, bool drawSphereAtPos = false, float sphereRadius = 1f, bool drawNormalAtPos = false, bool drawTangentAtPos = false)
    {
        if (m_Pts == null || m_Pts.Count < 2) return;

        Gizmos.color = color;
        int indexMax = m_Closed ? m_Pts.Count + 1 : m_Pts.Count;
        for (int i = 0; i < indexMax - 1; i++)
        {
            if (drawSphereAtPos) Gizmos.DrawSphere(m_Pts[i], sphereRadius);
            if (m_HasNormals && drawNormalAtPos) Gizmos.DrawLine(m_Pts[i], m_Pts[i] + m_Normals[i]);
            if (drawTangentAtPos) Gizmos.DrawLine(m_Pts[i], m_Pts[i] + m_Tangents[i]);
            Gizmos.DrawLine(m_Pts[i], m_Pts[(i + 1) % m_Pts.Count]);
        }
    }
}
