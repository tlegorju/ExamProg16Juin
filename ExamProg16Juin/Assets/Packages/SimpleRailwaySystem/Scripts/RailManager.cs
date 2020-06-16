using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleRailwaySystem
{
    public class RailManager : MonoBehaviour
    {
        [Header("Rail path based on spline")]
        [SerializeField] bool m_AlwaysRefreshInEditor = true;
        [SerializeField] LayerMask m_ProjectionLayer=0;
        [SerializeField] List<Transform> m_RailWayCtrlPoints = new List<Transform>();
        [SerializeField] float m_PtsDensity=10;
        [SerializeField] bool m_Closed = true;

        [Header("Rail 3D Mesh Generation")]
        [SerializeField] Vector2 m_RailCrossSectionSize = Vector2.zero;
        [SerializeField] float m_DistanceBetweenLeftAndRightRail=1;
        [SerializeField] Vector3 m_RailSleeperSize = Vector3.zero;
        [SerializeField] float m_RailDistanceBetweenSleepers=1;
        [SerializeField] GameObject m_SleeperCubePrefab=null;
        [SerializeField] Transform m_SleepersContainer = null;

        int NSleepers { get { return m_RailBaseSpline != null && m_RailBaseSpline.IsValid ? (int)(m_RailBaseSpline.Length / m_RailDistanceBetweenSleepers) : -1; } }

        public float DistanceBetweenTerrainAndTopOfSleeper { get { return m_RailSleeperSize.y * .5f; } }

        bool GetProjectedPosition(Vector3 pos, Vector3 projDir, out Vector3 projectedPos, out Vector3 projectedNormal)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos - projDir * 1000f, projDir, out hit, float.PositiveInfinity, m_ProjectionLayer))
            {
                projectedPos = hit.point;
                projectedNormal = hit.normal;
                return true;
            }
            else
            {
                projectedPos = pos;
                projectedNormal = Vector3.zero;
                return false;
            }
        }

        MySpline m_RailBaseSpline;
        MySpline RailBaseSpline
        {
            get
            {
                if (m_RailBaseSpline == null
#if UNITY_EDITOR
                || (!Application.isPlaying && (m_AlwaysRefreshInEditor || Selection.activeGameObject == gameObject))
#endif
                )
                {
                    List<Vector3> projectedNormals;

                    List<Vector3> projectedCtrlPts = new List<Vector3>();

                    Vector3 tmpPos, tmpNormal;
                    for (int i = 0; i < m_RailWayCtrlPoints.Count; i++)
                    {
                        if (GetProjectedPosition(m_RailWayCtrlPoints[i].position, Vector3.down, out tmpPos, out tmpNormal))
                            projectedCtrlPts.Add(tmpPos);
                    }
                    m_RailBaseSpline = new MySpline(projectedCtrlPts, null, m_Closed, m_PtsDensity);

                    projectedCtrlPts = new List<Vector3>();
                    projectedNormals = new List<Vector3>();
                    for (int i = 0; i < m_RailBaseSpline.NPts; i++)
                    {
                        if (GetProjectedPosition(m_RailBaseSpline[i], Vector3.down, out tmpPos, out tmpNormal))
                        {
                            projectedCtrlPts.Add(tmpPos);
                            projectedNormals.Add(tmpNormal);
                        }
                    }
                    m_RailBaseSpline = new MySpline(projectedCtrlPts, projectedNormals, m_Closed, m_PtsDensity);

                }

                return m_RailBaseSpline;
            }
        }

        public float Length { get { return m_RailBaseSpline.Length; } }
        public Vector3 this[float t]
        { // t between 0 ad 1
            get { return m_RailBaseSpline[t]; }
        }

        public bool GetPositionNormalTangent(float t, out Vector3 pos, out Vector3 normal, out Vector3 tangent, out int segmentIndex)
        {
            return m_RailBaseSpline.GetPositionNormalTangent(t, out pos, out normal, out tangent, out segmentIndex);
        }

        public bool GetSphereRailIntersection(Vector3 sphCenter, float sphRadius, int startIndex, int searchDir,
            out Vector3 pos, out Vector3 normal, out Vector3 tangent, out int segmentIndex)
        {
            return m_RailBaseSpline.GetSphereSplineIntersection(sphCenter, sphRadius, startIndex, searchDir,
            out pos, out normal, out tangent, out segmentIndex);
        }

        void GenerateRailSleepers()
        {
            int nSleepers = NSleepers;
            if (nSleepers <= 0) return;

            for (int i = 0; i < nSleepers; i++)
            {
                float k = (float)i / (m_Closed ? nSleepers : (nSleepers - 1));
                Vector3 pos, normal, tangent;
                m_RailBaseSpline.GetPositionNormalTangent(k, out pos, out normal, out tangent);
                GetProjectedPosition(pos, -normal, out pos, out normal);
                normal.Normalize();
                tangent.Normalize();
                Vector3 dir = Vector3.Cross(tangent, normal).normalized;

                GameObject newSleeperGO = Instantiate(m_SleeperCubePrefab);
                newSleeperGO.name = "RailSleeper_" + i;
                newSleeperGO.transform.localScale = m_RailSleeperSize;
                newSleeperGO.transform.position = pos;
                newSleeperGO.transform.rotation = Quaternion.LookRotation(tangent, normal);
                newSleeperGO.transform.SetParent(m_SleepersContainer);
            }
        }

        Mesh GenerateRailMesh()
        {
            if (RailBaseSpline == null || !RailBaseSpline.IsValid) return null;

            int nSleepers = NSleepers;
            if (nSleepers <= 0) return null;

            Mesh mesh = new Mesh();
            mesh.name = "rail";

            int nSegments = m_Closed ? nSleepers : nSleepers - 1;
            Vector3[] leftVertices = new Vector3[(nSegments + 1) * 4];
            Vector3[] leftNormals = new Vector3[leftVertices.Length];
            Vector3[] rightVertices = new Vector3[(nSegments + 1) * 4];
            Vector3[] rightNormals = new Vector3[leftVertices.Length];
            int[] leftTriangles = new int[nSegments * 3 * 2 * 3];
            int[] rightTriangles = new int[nSegments * 3 * 2 * 3];

            for (int i = 0; i < nSegments + 1; i++)
            {
                Vector3 pos, normal, tangent;
                float k = (float)i / (m_Closed ? nSleepers : (nSleepers - 1));
                m_RailBaseSpline.GetPositionNormalTangent(k, out pos, out normal, out tangent);
                GetProjectedPosition(pos, -normal, out pos, out normal);
                pos += normal * m_RailSleeperSize.y * .5f;
                normal.Normalize();
                tangent.Normalize();
                Vector3 dir = Vector3.Cross(tangent, normal).normalized;

                //left rail
                Vector3 pt1 = pos + dir * (m_DistanceBetweenLeftAndRightRail + m_RailCrossSectionSize.x) * .5f;
                Vector3 pt2 = pt1 + normal * m_RailCrossSectionSize.y;
                Vector3 pt3 = pt2 - dir * m_RailCrossSectionSize.x;
                Vector3 pt4 = pt3 - normal * m_RailCrossSectionSize.y;

                leftVertices[i * 4 + 0] = pt1;
                leftVertices[i * 4 + 1] = pt2;
                leftVertices[i * 4 + 2] = pt3;
                leftVertices[i * 4 + 3] = pt4;

                leftNormals[i * 4 + 0] = dir;
                leftNormals[i * 4 + 1] = (dir + normal).normalized;
                leftNormals[i * 4 + 2] = (-dir + normal).normalized;
                leftNormals[i * 4 + 3] = -dir;

                rightVertices[i * 4 + 0] = pt1 - dir * m_DistanceBetweenLeftAndRightRail;
                rightVertices[i * 4 + 1] = pt2 - dir * m_DistanceBetweenLeftAndRightRail;
                rightVertices[i * 4 + 2] = pt3 - dir * m_DistanceBetweenLeftAndRightRail;
                rightVertices[i * 4 + 3] = pt4 - dir * m_DistanceBetweenLeftAndRightRail;

                rightNormals[i * 4 + 0] = dir;
                rightNormals[i * 4 + 1] = (dir + normal).normalized;
                rightNormals[i * 4 + 2] = (-dir + normal).normalized;
                rightNormals[i * 4 + 3] = -dir;
            }

            //Les triangles
            int index = 0;
            for (int i = 0; i < nSegments; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int startIndex1 = i * 4 + j;
                    int startIndex2 = (i + 1) * 4 + j;

                    leftTriangles[index] = startIndex1;
                    rightTriangles[index++] = startIndex1 + (nSegments + 1) * 4;

                    leftTriangles[index] = startIndex2;
                    rightTriangles[index++] = startIndex2 + (nSegments + 1) * 4;

                    leftTriangles[index] = startIndex1 + 1;
                    rightTriangles[index++] = startIndex1 + 1 + (nSegments + 1) * 4;

                    leftTriangles[index] = startIndex2;
                    rightTriangles[index++] = startIndex2 + (nSegments + 1) * 4;

                    leftTriangles[index] = startIndex2 + 1;
                    rightTriangles[index++] = startIndex2 + 1 + (nSegments + 1) * 4;

                    leftTriangles[index] = startIndex1 + 1;
                    rightTriangles[index++] = startIndex1 + 1 + (nSegments + 1) * 4;
                }
            }

            List<Vector3> vertices = new List<Vector3>(leftVertices);
            vertices.AddRange(rightVertices);

            List<Vector3> normals = new List<Vector3>(leftNormals);
            normals.AddRange(rightNormals);

            List<int> triangles = new List<int>(leftTriangles);
            triangles.AddRange(rightTriangles);

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!Application.isPlaying) return;

            MeshFilter mf = GetComponent<MeshFilter>();
            mf.sharedMesh = GenerateRailMesh();
            GenerateRailSleepers();
        }

#if UNIY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            if (m_RailWayCtrlPoints != null && m_RailWayCtrlPoints.Count > 0)
            {
                for (int i = 0; i < m_RailWayCtrlPoints.Count; i++)
                {
                    Gizmos.DrawLine(m_RailWayCtrlPoints[i].position, m_RailWayCtrlPoints[(i + 1) % m_RailWayCtrlPoints.Count].position);
                    DrawString(i.ToString(), m_RailWayCtrlPoints[i].position, Color.red);
                }
            }

            if (RailBaseSpline != null && RailBaseSpline.IsValid)
                m_RailBaseSpline.DrawGizmos(Color.red, true, .25f, true, true);

        }


        void DrawString(string text, Vector3 worldPos, Color? colour = null)
        {
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            if (view == null) return;

            UnityEditor.Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (colour.HasValue) GUI.color = colour.Value;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }

            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
        }
#endif
    }
}