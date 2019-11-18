using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperClasses
{
    public class BoundingBox
    {
        public int tlx;
        public int tly;
        public int brx;
        public int bry;
        public int centerx;
        public int centery;

        public static BoundingBox NullBoundingBox = new BoundingBox(0, 0, 0, 0);
        public static bool IsNullBoundingBox(BoundingBox b)
        {
            if(b.tlx==0 && b.tly==0 && b.brx==0 && b.bry==0)
            {
                return true;
            }
            return false;
        }

        public BoundingBox() { }

        public BoundingBox(int l_tlx, int l_tly, int l_brx, int l_bry)
        {
            tlx = l_tlx;
            tly = l_tly;
            brx = l_brx;
            bry = l_bry;
            centerx = (tlx + brx) / 2;
            centery = (tly + bry) / 2;
        }

        public BoundingBox(BoundingBox b)
        {
            tlx = b.tlx;
            tly = b.tly;
            brx = b.brx;
            bry = b.bry;
            centerx = (tlx + brx) / 2;
            centery = (tly + bry) / 2;
        }

        public bool regularizeByImageSize(int width, int height)
        {
            if (tlx > width || tly > height || brx < 0 || bry < 0) return false; 
            if (tlx < 0) tlx = 0;
            if (tly < 0) tly = 0;
            if (brx > width) brx = width;
            if (bry > height) bry = height;
            centerx = (tlx + brx) / 2;
            centery = (tly + bry) / 2;
            return true;
        }

        public int getWidth()
        {
            return Math.Abs(brx - tlx);
        }

        public int getHeight()
        {
            return Math.Abs(bry - tly);
        }

        public double ComputeArea()
        {
            return Math.Abs((brx - tlx) * (bry - tly));
        }

        public override string ToString()
        {
            return tlx + "\t" + tly + "\t" + brx + "\t" + bry;
        }

        public double ComputeOverlapArea(BoundingBox b)
        {
            return ComputeOverlapArea(b.tlx, b.tly, b.brx, b.bry);
        }

        public static double ComputeOverlapArea(BoundingBox b1, BoundingBox b2)
        {
            return b1.ComputeOverlapArea(b2);
        }

        public double ComputeOverlapArea(double l_tlx, double l_tly, double l_brx, double l_bry)
        {


            if (tlx > l_brx || l_tlx > brx)
                return 0;
            if (tly > l_bry || l_tly > bry)
                return 0;


            //there is some over lap
            double min_tlx = Math.Max(tlx, l_tlx);
            double min_tly = Math.Max(tly, l_tly);
            double min_brx = Math.Min(brx, l_brx);
            double min_bry = Math.Min(bry, l_bry);
            double overlapArea = (min_brx - min_tlx) * (min_bry - min_tly);
            return overlapArea;
        }

        public double ComputeOverlapAreaFraction(BoundingBox b)
        {
            return ComputeOverlapArea(b.tlx, b.tly, b.brx, b.bry) / ComputeArea();
        }

        public double ComputeOverlapAreaFraction(double l_tlx, double l_tly, double l_brx, double l_bry)
        {
            return ComputeOverlapArea(l_tlx, l_tly, l_brx, l_bry) / ComputeArea();
        }

        //this is a symmetric measure
        public static double ComputeOverlapAreaFraction(BoundingBox b1, BoundingBox b2)
        {
            double d1= b1.ComputeOverlapAreaFraction(b2);
            double d2 = b2.ComputeOverlapAreaFraction(b1);
            return Math.Min(d1,d2);
        }

        public static double ComputeIntersectionOverUnion(BoundingBox b1, BoundingBox b2)
        {
            double overlapArea = b1.ComputeOverlapArea(b2);
            double area1 = b1.ComputeArea();
            double area2 = b2.ComputeArea();
            double unionArea = area1 + area2 - overlapArea;
            double intersectionOverUnion = overlapArea / (unionArea);
            return intersectionOverUnion;
        }

        public double ComputeMaxDeviationMetric(BoundingBox b)
        {
            return ComputeMaxDeviationMetric(b.tlx, b.tly, b.brx, b.bry);
        }

        public double ComputeMaxDeviationMetric(double l_tlx, double l_tly, double l_brx, double l_bry)
        {
            double deviation = Math.Max(Math.Abs(tlx - l_tlx), Math.Abs(tly - l_tly));
            deviation = Math.Max(deviation, Math.Abs(bry - l_bry));
            deviation = Math.Max(deviation, Math.Abs(brx - l_brx));
            // deviation = Math.Max(deviation, Math.Abs(brx - tlx - l_brx + l_tlx));
            // deviation = Math.Max(deviation, Math.Abs(bry - tly - l_bry + l_tly));
            return deviation;
        }

        public double ComputeNormalizedMaxDeviationMetric(double l_tlx, double l_tly, double l_brx, double l_bry, double thresholdX, double thresholdY)
        {
            double deviation = Math.Max(Math.Abs(tlx - l_tlx)/thresholdX, Math.Abs(tly - l_tly)/thresholdY);
            deviation = Math.Max(deviation, Math.Abs(bry - l_bry)/thresholdY);
            deviation = Math.Max(deviation, Math.Abs(brx - l_brx)/thresholdX);
            // deviation = Math.Max(deviation, Math.Abs(brx - tlx - l_brx + l_tlx));
            // deviation = Math.Max(deviation, Math.Abs(bry - tly - l_bry + l_tly));
            return deviation;
        }

        public double ComputeNormalizedMaxDeviationMetric(BoundingBox b, double thresholdX, double thresholdY)
        {
            return ComputeNormalizedMaxDeviationMetric(b.tlx, b.tly, b.brx, b.bry, thresholdX, thresholdY);
        }

        public static bool AreAllignedWithinToleranceBounds(BoundingBox b1, BoundingBox b2, int thresholdX, int thresholdY)
        {
            if(Math.Abs(b1.tlx-b2.tlx) <= thresholdX && Math.Abs(b1.tly - b2.tly) <= thresholdY && Math.Abs(b1.brx - b2.brx) <= thresholdX && Math.Abs(b1.bry - b2.bry) <= thresholdY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool intersectsLineSegment(double x1, double y1, double x2, double y2)
        {
            bool ret = false;
            double[] dummy;
            ret = LineSegment.Intersect(tlx, tly, brx, tly, x1, y1, x2, y2, out dummy);
            if (ret == true)
            {
                return ret;
            }
            ret = LineSegment.Intersect(brx, tly, brx, bry, x1, y1, x2, y2, out dummy);
            if (ret == true)
            {
                return ret;
            }
            ret = LineSegment.Intersect(brx, bry, tlx, bry, x1, y1, x2, y2, out dummy);
            if (ret == true)
            {
                return ret;
            }
            ret = LineSegment.Intersect(tlx, bry, tlx, tly, x1, y1, x2, y2, out dummy);
            return ret;
        }

        public bool intersectsLineSegments(List<double[,]> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                bool ret = intersectsLineSegment(lines[i][0, 0], lines[i][0, 1], lines[i][1, 0], lines[i][1, 1]);
                if (ret)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
