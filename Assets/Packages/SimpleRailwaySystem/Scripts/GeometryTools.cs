using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryTools
{
    /// <summary>
    /// Rectangle
    /// </summary>
    public struct Rectangle
    {
        public Vector2 origin;
        public Vector2 size;
        public Rectangle(Vector2 origin, Vector2 size) { this.origin = origin; this.size = size; }
    }

    /// <summary>
    /// Range
    /// </summary>
    public struct Range
    {

        public float minimum;
        public float maximum;
        public Range(float minimum, float maximum) { this.minimum = minimum; this.maximum = maximum; }

        public static Range SortRange(Range r)
        {

            Range sorted = r;
            if (r.minimum > r.maximum)
            {
                sorted.minimum = r.maximum;
                sorted.maximum = r.minimum;
            }
            return sorted;
        }

        public static bool RangesOverlap(Range a, Range b)
        {
            return b.minimum <= a.maximum && a.minimum <= b.maximum;
        }
    }

    /// <summary>
    /// Vector2D
    /// </summary>
    public static class Vector2D
    {
        public static bool AreVectorsParallel_2D(Vector2 a, Vector2 b)
        {
            Vector2 na = new Vector2(-a.y, a.x);
            return (!(0 == a.x && 0 == a.y) && !(0 == b.x && 0 == b.y) && Mathf.Approximately(0, Vector3.Dot(na, b)));
        }
    }

    /// <summary>
    /// Triangle2D
    /// </summary>
    public static class Triangle2D
    {
        public static bool IsPointInTriangle_2D(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
            var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

            return A < 0 ?
                    (s <= 0 && s + t >= A) :
                    (s >= 0 && s + t <= A);
        }
    }

    /// <summary>
    /// Intersections
    /// </summary>
    public static class Intersections
    {
        //2D METHODS

        public static bool AreSegmentPointsOnSameAxisSide_2D(Vector2 axisBasePt, Vector2 axisDir, Vector2 segmentPt1, Vector2 segmentPt2)
        {
            Vector2 d1 = axisBasePt - segmentPt1;
            Vector2 d2 = axisBasePt - segmentPt2;
            Vector2 n = new Vector2(-axisDir.y, axisDir.x);//
            return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0;
        }

        public static bool LineCollidesWithSegment_2D(Vector2 axisBasePt, Vector2 axisDir, Vector2 segmentPt1, Vector2 segmentPt2)
        {
            return !AreSegmentPointsOnSameAxisSide_2D(axisBasePt, axisDir, segmentPt1, segmentPt2);
        }


        public static bool SegmentsCollide_2D(Vector2 seg1Pt1, Vector2 seg1Pt2, Vector2 seg2Pt1, Vector2 seg2Pt2)
        {
            Vector2 dir1 = seg1Pt2 - seg1Pt1, dir2 = seg2Pt2 - seg2Pt1;
            if ((dir1.x == 0 && dir1.y == 0) || (dir2.x == 0 && dir2.y == 0))
                return false;

            if (AreSegmentPointsOnSameAxisSide_2D(seg1Pt1, dir1, seg2Pt1, seg2Pt2))
                return false;

            if (AreSegmentPointsOnSameAxisSide_2D(seg2Pt1, dir2, seg1Pt1, seg1Pt2))
                return false;

            return true;
        }


        public static bool LineCollidesWithRectangle_2D(Vector2 linePt, Vector2 lineDir, Rectangle r)
        {
            Vector2 n = new Vector2(-lineDir.y, lineDir.x);

            float dp1, dp2, dp3, dp4;

            Vector2 c1 = r.origin;
            Vector2 c2 = c1 + r.size;
            Vector2 c3 = new Vector2(c2.x, c1.y);
            Vector2 c4 = new Vector2(c1.x, c2.y);

            c1 = c1 - linePt;
            c2 = c2 - linePt;
            c3 = c3 - linePt;
            c4 = c4 - linePt;

            dp1 = Vector3.Dot(n, c1);
            dp2 = Vector3.Dot(n, c2);
            dp3 = Vector3.Dot(n, c3);
            dp4 = Vector3.Dot(n, c4);

            return (dp1 * dp2 <= 0) || (dp2 * dp3 <= 0) || (dp3 * dp4 <= 0);
        }

        public static bool SegmentCollidesWithRectangle_2D(Rectangle r, Vector2 segmentPt1, Vector2 segmentPt2)
        {
            Range rRange = new Range(r.origin.x, r.origin.x + r.size.x);
            Range sRange = new Range(segmentPt1.x, segmentPt2.x);
            sRange = Range.SortRange(sRange);
            if (!Range.RangesOverlap(rRange, sRange))
                return false;

            rRange.minimum = r.origin.y;
            rRange.maximum = r.origin.y + r.size.y;
            sRange.minimum = segmentPt1.y;
            sRange.maximum = segmentPt2.y;
            sRange = Range.SortRange(sRange);
            if (!Range.RangesOverlap(rRange, sRange))
                return false;

            Vector2 linePt = segmentPt1;
            Vector2 lineDir = segmentPt2 - segmentPt1;
            return LineCollidesWithRectangle_2D(linePt, lineDir, r);
        }


        public static bool LineLineIntersection_2D(Vector2 pt11, Vector2 pt12, Vector2 pt21, Vector2 pt22, out Vector2 interPt)
        { //https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
            interPt = new Vector2();

            Vector2 dir1 = pt12 - pt11;
            Vector2 dir2 = pt22 - pt21;
            Vector2 n2 = new Vector2(-dir2.y, dir2.x);

            if (Mathf.Approximately(0, Vector2.Dot(dir1, n2))) return false;

            float denominator = (pt11.x - pt12.x) * (pt21.y - pt22.y) - (pt11.y - pt12.y) * (pt21.x - pt22.x);
            float numeratorX = (pt11.x * pt12.y - pt11.y * pt12.x) * (pt21.x - pt22.x) - (pt11.x - pt12.x) * (pt21.x * pt22.y - pt21.y * pt22.x);
            float numeratorY = (pt11.x * pt12.y - pt11.y * pt12.x) * (pt21.y - pt22.y) - (pt11.y - pt12.y) * (pt21.x * pt22.y - pt21.y * pt22.x);
            interPt = new Vector2(numeratorX / denominator, numeratorY / denominator);
            return true;
        }

        public static bool LineSegmentIntersection_2D(Vector2 segPt1, Vector2 segPt2, Vector2 linePt1, Vector2 linePt2, out Vector2 interPt)
        {
            return LineLineIntersection_2D(segPt1, segPt2, linePt1, linePt2, out interPt)
                && Vector2.Dot(interPt - segPt1, interPt - segPt2) < 0;
        }


        //public static bool SegmentSegmentIntersection(Vector2 seg1Pt1, Vector2 seg1Pt2, Vector2 seg2Pt1, Vector2 seg2Pt2, out Vector2 interPt)
        //{
        //    return LineLineIntersection(seg1Pt1, seg1Pt2, seg2Pt1, seg2Pt2, out interPt)
        //        && (Vector2.Dot(interPt - seg1Pt1, interPt - seg1Pt2) < 0)
        //        && (Vector2.Dot(interPt - seg2Pt1, interPt - seg2Pt2) < 0);
        //}
        public static bool SegmentSegmentIntersection_2D(Vector2 seg1Pt1, Vector2 seg1Pt2, Vector2 seg2Pt1, Vector2 seg2Pt2, out Vector2 interPt)
        {
            bool lineline = LineLineIntersection_2D(seg1Pt1, seg1Pt2, seg2Pt1, seg2Pt2, out interPt);
            float dot1 = Vector2.Dot(interPt - seg1Pt1, interPt - seg1Pt2);
            if (Mathf.Abs(dot1) < 1e-6) dot1 = 0;   // important lorsque l'extrémité d'un segment touche l'autre segment
            float dot2 = Vector2.Dot(interPt - seg2Pt1, interPt - seg2Pt2);
            if (Mathf.Abs(dot2) < 1e-6) dot2 = 0;   // important lorsque l'extrémité d'un segment touche l'autre segment
            return lineline
                && dot1 <= 0
                && dot2 <= 0;
        }

        //3D METHODS
        public static bool OrientedSegmentSphereIntersection_3D(Vector3 segPt1, Vector3 segPt2, Vector3 sphCentre, float sphRadius,
            out Vector3 intersectionPt)
        {
            intersectionPt = Vector3.zero;

            Vector3 sphCenterToSegPt1 = segPt1 - sphCentre;
            Vector3 segDir = segPt2 - segPt1;

            float a = segDir.sqrMagnitude;
            float b = 2 * Vector3.Dot(sphCenterToSegPt1, segDir);
            float c = sphCenterToSegPt1.sqrMagnitude - sphRadius * sphRadius;

            float delta = b * b - 4 * a * c;

            if (delta < 0)
            {
                return false; // no intersection 
            }

            float t1 = (-b + Mathf.Sqrt(delta)) / (2 * a);
            float t2 = (-b - Mathf.Sqrt(delta)) / (2 * a);

            if (t1 < 0 || t1 > 1) t1 = float.PositiveInfinity;
            if (t2 < 0 || t2 > 1) t2 = float.PositiveInfinity;

            float t = Mathf.Min(t1, t2);
            if (t < 0 || t > 1) return false;

            intersectionPt = segPt1 + segDir * t;

            return true;
        }

        //public static int GetPolygoneHalfLineNbIntersections(List<Segment2D> polygoneSegments, Vector2 linePt, Vector2 lineDir)
        //{
        //    int nIntersections = 0;
        //    for (int i = 0; i < polygoneSegments.Count; i++)
        //    {
        //        Segment2D segment2D = polygoneSegments[i];
        //        Vector2 interPt;
        //        if (LineSegmentIntersection(segment2D.Pt1, segment2D.Pt2, linePt, linePt + lineDir, out interPt)
        //            && Vector2.Dot(interPt - linePt, lineDir) > 0)
        //            nIntersections++;
        //    }
        //    return nIntersections;
        //}

    }
}
