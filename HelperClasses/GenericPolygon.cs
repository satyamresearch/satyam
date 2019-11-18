using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperClasses
{
    /// <summary>
    /// A segment is a part of the image, which consists of multiple polygons with holes.
    /// </summary>
    public class Segment
    {
        public List<GenericPolygon> polygons;

        public BoundingBox getBoundingBox()
        {
            int tlx = 1000000;
            int tly = 1000000;
            int brx = 0;
            int bry = 0;

            foreach (GenericPolygon poly in polygons)
            {
                BoundingBox bb = poly.getBoundingBox();
                if (bb.tlx < tlx) tlx = bb.tlx;
                if (bb.tly < tly) tly = bb.tly;
                if (bb.brx > brx) brx = bb.brx;
                if (bb.bry > bry) bry = bb.bry;
            }
            return new BoundingBox(tlx, tly, brx, bry);
        }


        public bool PointIsInSegment(int x, int y)
        {
            int count = 0;
            foreach(GenericPolygon poly in polygons)
            {
                List<int[]> allIntersects = poly.getAllIntersectionPointsOfLineSegment(x, y, 0, 0);
                count += allIntersects.Count;
            }
            return count % 2 == 1;
        }


        public static double computeIoU_PixelSweep(Segment seg1,Segment seg2)
        {

            BoundingBox b1 = seg1.getBoundingBox();
            BoundingBox b2 = seg2.getBoundingBox();
            int tlx = Math.Min(b1.tlx, b2.tlx);
            int tly = Math.Min(b1.tly, b2.tly);
            int brx = Math.Max(b1.brx, b2.brx);
            int bry = Math.Max(b1.bry, b2.bry);

            int seg1Area = 0;
            int seg2Area = 0;
            int overlappingArea = 0;
            for (int i = tlx; i < brx; i++)
            {
                for (int j = tly; j < bry; j++)
                {
                    bool InSeg1 = false;
                    bool InSeg2 = false;
                    if (seg1.PointIsInSegment(i, j))
                    {
                        seg1Area++;
                        InSeg1 = true;
                    }
                    if (seg2.PointIsInSegment(i, j))
                    {
                        seg2Area++;
                        InSeg2 = true;
                    }
                    if (InSeg1 && InSeg2)
                    {
                        overlappingArea++;
                    }
                }
            }

            double intersection = (double)overlappingArea;
            double union = (double)(seg1Area + seg2Area - overlappingArea);
            if (union == 0) return 0;

            return intersection / union;
        }
    }

    public class GenericPolygon
    {
        public List<int[]> vertices = new List<int[]>();

        public GenericPolygon() { }

        public GenericPolygon(List<int[]> _vertices)
        {
            vertices = _vertices;
        }


        public GenericPolygon(GenericPolygon p)
        {
            foreach(int[] vertex in p.vertices)
            {
                int[] copy_vertex = new int[2];
                copy_vertex[0] = vertex[0];
                copy_vertex[1] = vertex[1];
                vertices.Add(copy_vertex);
            }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for(int i =0;i< vertices.Count; i++ )
            {
                s.Append(vertices[i][0] + "\t" + vertices[i][1]);
                if(i!=vertices.Count-1)
                {
                    s.Append("\t");
                }
            }
            return s.ToString();
        }


        //determinant of |0 1;v1' 1;v2' 1|
        private double TriangleArea(int[] v1, int[] v2)
        {
            return 0.5 * (-v1[1] * v2[0] + v1[0] * v2[1]);
        }


        public double ComputeArea()
        {
            double area = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                area += -vertices[i][1] * vertices[i + 1][0] + vertices[i][0] * vertices[i + 1][1];
            }
            area += -vertices[vertices.Count - 1][1] * vertices[0][0] + vertices[vertices.Count - 1][0] * vertices[0][1];
            area = 0.5 * Math.Abs(area);
            return area;
        }

        public BoundingBox getBoundingBox()
        {
            int tlx = 1000000;
            int tly = 1000000;
            int brx = 0;
            int bry = 0;
            
            foreach (int[] xy in vertices)
            {
                if (xy[0] < tlx) tlx = xy[0];
                if (xy[1] < tly) tly = xy[1];
                if (xy[0] > brx) brx = xy[0];
                if (xy[1] > bry) bry = xy[1];
            }
            return new BoundingBox(tlx,tly,brx,bry);
        }

        /// <summary>
        /// draw a line from 0,0 to x,y. if odd number of intersections, point is in. otherwise out.
        /// applies only when the point is not on the polygon.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool PointIsInPolygon(int x,int y)
        {
            List<int[]> allIntersects = getAllIntersectionPointsOfLineSegment(x, y, 0, 0);
            return allIntersects.Count % 2 == 1;
        }


        public bool PointIsOnPolygon(int x, int y)
        {
            foreach (int []xy in vertices)
            {
                if (x==xy[0]&& y == xy[1])
                {
                    return true;
                }
            }
            return false;
        }

        public List<int[]> getAllIntersectionPointsOfLineSegment(int x1, int y1, int x2, int y2)
        {
            List<int[]> ret = new List<int[]>();
            for (int i = 0; i < vertices.Count; i++)
            {
                int xx1 = vertices[i][0];
                int yy1 = vertices[i][1];

                int j = (i + 1) % vertices.Count;
                int xx2 = vertices[j][0];
                int yy2 = vertices[j][1];

                double[] outCoordinates;
                bool intersect = LineSegment.Intersect(x1, y1, x2, y2, xx1, yy1, xx2, yy2, out outCoordinates);                
                if (intersect)
                {
                    ret.Add(new int[] { (int)outCoordinates[0], (int)outCoordinates[1] });
                }
            }
            return ret;
        }


        public List<int[]> getAllIntersectionPointsOfLineSegment(int x1, int y1, int x2, int y2, out List<int[]> IntersectingLineSegments)
        {
            List<int[]> ret = new List<int[]>();
            IntersectingLineSegments = new List<int[]>();
            for (int i = 0; i < vertices.Count; i++)
            {
                int xx1 = vertices[i][0];
                int yy1 = vertices[i][1];

                int j = (i + 1) % vertices.Count;
                int xx2 = vertices[j][0];
                int yy2 = vertices[j][1];

                double[] outCoordinates;
                bool intersect = LineSegment.Intersect(x1, y1, x2, y2, xx1, yy1, xx2, yy2, out outCoordinates);
                if (intersect)
                {
                    ret.Add(new int[] { (int)outCoordinates[0], (int)outCoordinates[1] });
                    IntersectingLineSegments.Add(new int[] {xx1,yy1,xx2,yy2 });
                }
            }
            return ret;
        }

        public int[] getClosestIntersectingPointsToX1Y1OfLineSegment(int x1, int y1, int x2,  int y2)
        {
            double minDistatnce = 999999999;
            int[] closestIntersect = new int[] { -1, -1 };
            List<int[]> allIntersectingPoints = getAllIntersectionPointsOfLineSegment(x1, y1, x2, y2);
            foreach(int[] xy in allIntersectingPoints)
            {
                double distance = LineSegment.getLength(x1, y1, xy[0], xy[1]);
                if (distance < minDistatnce)
                {
                    minDistatnce = distance;
                    closestIntersect[0] = xy[0];
                    closestIntersect[1] = xy[1];
                }
            }
            return closestIntersect;
        }


        public int[] getClosestPointsOnPolygonToAPoint(int x, int y, out LineSegment closestEdge, out int closestEdgeStartingPointIndex)
        {
            double closestDistance = double.MaxValue;
            int[] closestPointOnPolygonBoundary = new int[] { -1,-1};
            closestEdge = new LineSegment(-1, -1, -1, -1);
            closestEdgeStartingPointIndex = -1;

            for (int i = 0; i < vertices.Count; i++)
            {
                int xx1 = vertices[i][0];
                int yy1 = vertices[i][1];

                int j = (i + 1) % vertices.Count;
                int xx2 = vertices[j][0];
                int yy2 = vertices[j][1];
                LineSegment line = new LineSegment(xx1, yy1, xx2, yy2);
                double slope = line.slope();
                double perpendicularSlope = 0;
                if (!double.IsInfinity(slope))
                {
                    perpendicularSlope = -1 / slope;
                }

                // find the perpendicular line that cross (x,y) in image.
                // assume the line is y=ax+b, a= slope, b = y-ax, b=px0[1]
                //double xstart = 0;
                //double ystart = (y - perpendicularSlope * (double)x);
                //double xend = -(ystart / perpendicularSlope);
                //double yend = 0;

                double xstart = 0;
                double ystart = 0;
                double xend = 0;
                double yend = 0;

                double b = (y - perpendicularSlope * (double)x);
                double[] px0 = new double[] { 0,  b};
                double[] py0 = new double[] { -(b / perpendicularSlope), 0 };
                // assume width height less than 10000
                int max = 100000;
                double[] pxmax = new double[] { max, perpendicularSlope * max + b };
                double[] pymax = new double[] { -1,-1};
                if (perpendicularSlope == 0)
                {
                    pymax[0] = max-1;
                    pymax[1] = b;
                }
                else
                {
                    pymax[0] = (max - b) / perpendicularSlope;
                    pymax[1] = max;
                }

                // select the 2 far ends
                List<double[]> plist = new List<double[]>() { px0, py0, pxmax, pymax };
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                foreach (double[] p in plist)
                {
                    if (p[0] < minX)
                    {
                        minX = p[0];
                        xstart = minX;
                        ystart = p[1];
                    }
                    if (p[0] > maxX)
                    {
                        maxX = p[0];
                        xend = maxX;
                        yend = p[1];
                    }
                }


                // find out whether it intersect, if intersect, since perpendicular, the intersects is the closest distance to that edge
                double[] intersect = new double[] { -1, -1 };
                List<double[]> candidatePoints = new List<double[]>();
                if (LineSegment.Intersect(xstart,ystart,xend,yend, xx1, yy1, xx2, yy2,out intersect))
                {
                    candidatePoints.Add(intersect);
                }
                else
                {
                    //"Not Intersecting", then the closest point must be one of the 2 ends.
                    candidatePoints.Add(new double[] { xx1, yy1 });
                    candidatePoints.Add(new double[] { xx2, yy2 });
                }
                foreach (double[] p in candidatePoints)
                {
                    double distance = LineSegment.getLength(x, y, (int)p[0], (int)p[1]);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPointOnPolygonBoundary[0] = (int)p[0];
                        closestPointOnPolygonBoundary[1] = (int)p[1];
                        closestEdge = line;
                        closestEdgeStartingPointIndex = i;
                    }
                }
            }
            return closestPointOnPolygonBoundary;
        }

        

    }
}
