using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperClasses
{

    public class mPoint
    {
        public double x;
        public double y;
    }
    public class SimplePolygon
    {

        const double eps = 1e-8;

        static int SignofDouble(double x)
        {
            if (x > eps) return 1;
            return x < -eps ? -1 : 0;
        }
        
        static double cross(mPoint a, mPoint b, mPoint c)
        {
            return (a.x - c.x) * (b.y - c.y) - (b.x - c.x) * (a.y - c.y);
        }
        static mPoint intersection(mPoint a, mPoint b, mPoint c, mPoint d)
        {
            mPoint p = a;
            double t = ((a.x - c.x) * (c.y - d.y) - (a.y - c.y) * (c.x - d.x)) / ((a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x));
            p.x += (b.x - a.x) * t;
            p.y += (b.y - a.y) * t;
            return p;
        }
        
        public static double PolygonArea(List<mPoint> p, int n)
        {
            if (n < 3) return 0.0;
            double s = p[0].y * (p[n - 1].x - p[1].x);
            p[n] = p[0];
            for (int i = 1; i < n; ++i)
                s += p[i].y * (p[i - 1].x - p[i + 1].x);
            return Math.Abs(s * 0.5);
        }

        /// <summary>
        /// ConvexPolygonIntersectArea
        ///     return the intersection area, 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="intersectPolygon">full close loop polygon</param>
        /// <returns></returns>
        public static double CPIA(List<mPoint> a, List<mPoint> b, out List<mPoint> intersectPolygon)
        {

            int na = a.Count;
            int nb = b.Count;
            
            int tn, sflag, eflag;
            
            a.Add(a[0]);
            b.Add(b[0]);

            intersectPolygon = new List<mPoint>();
            List<mPoint> tmp = new List<mPoint>();

            foreach(mPoint pa in a)
            {
                intersectPolygon.Add(pa);
            }

            for (int i = 0; i < na && nb > 2; i++)
            {
                sflag = SignofDouble(cross(a[i + 1], intersectPolygon[0], a[i]));
                for (int j = tn = 0; j < nb; j++, sflag = eflag)
                {
                    if (sflag >= 0)
                    {
                        tmp.Add(intersectPolygon[j]);
                        tn++;
                    }
                    eflag = SignofDouble(cross(a[i + 1], intersectPolygon[j + 1], a[i]));
                    if ((sflag ^ eflag) == -2)
                    {
                        mPoint intersets = intersection(a[i], a[i + 1], intersectPolygon[j], intersectPolygon[j + 1]); ///find the intersection point
                        tmp.Add(intersets);
                        tn++;
                    }
                        
                }
                //memcpy(p, tmp, sizeof(Point) * tn);
                intersectPolygon.Clear();
                intersectPolygon = tmp;
                //for(int k = 0; k < tn; k++)
                //{
                //    p.Add(tmp[k]);
                //}
                nb = tn;
                intersectPolygon.Add(intersectPolygon[0]);
            }
            if (nb < 3) return 0.0;
            return PolygonArea(intersectPolygon, nb);
        }

        /// <summary>
        /// /// SimplePolygonIntersectArea 
        ///     Decompose to triangles and calculate the intersection area.
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="intersectPolygon">full close loop polygon</param>
        /// <returns></returns>
        public static double SPIA(List<mPoint> a, List<mPoint> b, out List<List<mPoint>> intersectPolygon)///SimplePolygonIntersectArea 
        {
            intersectPolygon = new List<List<mPoint>>();
            int na = a.Count;
            int nb = b.Count;
            int i, j;
            mPoint[] t1 = new mPoint[3];
            mPoint [] t2 = new mPoint[3];
            double res = 0, num1, num2;
            a.Add(t1[0] = a[0]);
            b.Add(t2[0] = b[0]);
            for (i = 2; i < na; i++)
            {
                t1[1] = a[i - 1];
                t1[2] = a[i];
                num1 = SignofDouble(cross(t1[1], t1[2], t1[0]));
                // area with directions. positive when sweeping towards the end, negative when backwards
                if (num1 < 0) {
                    GenericMethods.Swap<mPoint>(ref t1[1], ref t1[2]);
                }
                for (j = 2; j < nb; j++)
                {
                    t2[1] = b[j - 1];
                    t2[2] = b[j];
                    num2 = SignofDouble(cross(t2[1], t2[2], t2[0]));
                    if (num2 < 0)
                    {
                        GenericMethods.Swap<mPoint>(ref t2[1], ref t2[2]);
                    }
                    List<mPoint> convexIntersectPolygon = new List<mPoint>();
                    res += CPIA(t1.ToList(), t2.ToList(), out convexIntersectPolygon) * num1 * num2;
                    intersectPolygon.Add(convexIntersectPolygon);
                }
            }

            // merge all mergable convex polygons

            intersectPolygon = mergeAllConvexPolygons(intersectPolygon);
            
            return res;//res is the intersection area
        }


        static List<List<mPoint>> mergeAllConvexPolygons(List<List<mPoint>> polygons)
        {
            List<List<mPoint>> ret = new List<List<mPoint>>();
            ret = mergeNeighboringConvexPolygons(polygons);
            int curCount = ret.Count;
            int lastCount = 0;
            while (true)
            {
                ret= mergeNeighboringConvexPolygons(ret);
                // TODO: the second time they might not be convex anymore, tho there is one continuous edge but the check might happen twice....
                if (ret.Count == curCount) return ret;
                else
                {
                    lastCount = curCount;
                    curCount = ret.Count;
                }
            }
            
        }

        static List<List<mPoint>> mergeNeighboringConvexPolygons(List<List<mPoint>> polygons)
        {
            
            List<bool> merged= new List<bool>();
            for (int p = 0; p < polygons.Count; p++)
            {
                merged.Add(false);
            }
            List<List<mPoint>> ret = new List<List<mPoint>>();
            for (int p = 0; p < polygons.Count-1; p++)
            {
                if (merged[p]) continue;
                bool mergable = false;
                for (int q = p+1; q < polygons.Count; q++)
                {
                    if (merged[q]) continue;
                    List<mPoint> mergedpolygon = mergeTwoConvexPolygons(polygons[p], polygons[q]);
                    if (mergedpolygon.Count == 0) continue;

                    ret.Add(mergedpolygon);
                    mergable = true;
                    merged[p] = true;
                    merged[q] = true;
                }
                if (!mergable)
                {
                    
                }
            }

            for ( int i = 0; i < merged.Count; i++)
            {
                if (!merged[i])
                {
                    ret.Add(polygons[i]);
                }
            }

            return ret;
        }

        /// <summary>
        /// non-overlapping Convex polygons can have continuous common edges.
        /// 
        /// </summary>
        /// <param name="poly1">must be full close loop polygon</param>
        /// <param name="poly2">must be full close loop polygon</param>
        /// <returns></returns>
        static List<mPoint> mergeTwoConvexPolygons(List<mPoint> poly1, List<mPoint> poly2)
        {
            List<mPoint> ret = new List<mPoint>();
            bool found = false;
            for (int i = 0; i < poly1.Count - 1; i++)
            {
                LineSegment l1 = new LineSegment(poly1[i].x, poly1[i].y, poly1[i+1].x, poly1[i+1].y);
                for (int j = 0; j < poly2.Count - 1; j++)
                {
                    LineSegment l2 = new LineSegment(poly2[j].x, poly2[j].y, poly2[j + 1].x, poly2[j + 1].y);

                    bool sameDirection = true;
                    bool sameSegment = false;
                    if (l1.x1 == l2.x1 && l1.y1 == l2.y1 && l1.x2 == l2.x2 && l1.y2 == l2.y2)
                    {
                        sameSegment = true;
                        sameDirection = true;
                    }

                    if (l1.x1 == l2.x2 && l1.y1 == l2.y2 && l1.x2 == l2.x1 && l1.y2 == l2.y1)
                    {
                        sameSegment = true;
                        sameDirection = false;
                    }


                    if (sameSegment)
                    {
                        // add half chain of poly1, excluding i
                        for ( int p = 0; p < i; p++)
                        {
                            ret.Add(poly1[p]);
                        }

                        // add the entire poly2, starting from j or j+1 depending on direction
                        for (int q = 0; q < poly2.Count - 1; q++)
                        {
                            if (!sameDirection)
                            {
                                int idx = (q + j+1) % (poly2.Count - 1);
                                ret.Add(poly2[idx]);
                            }
                            else
                            {
                                int idx = (j -q + poly2.Count - 1) % (poly2.Count - 1);
                                ret.Add(poly2[idx]);
                            }
                        }
                        
                        // the other half poly1, excluding i+1
                        for ( int p=i+2;p< poly1.Count; p++)
                        {
                            ret.Add(poly1[p]);
                        }
                        found= true;break;
                    }
                    
                }
                if (found) break;
            }

            return ret;
        }
    }
}
