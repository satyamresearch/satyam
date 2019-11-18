using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperClasses
{
    public class LineSegment
    {
        public double x1;
        public double y1;
        public double x2;
        public double y2;

        public LineSegment() { }

        public LineSegment(double l_x1, double l_y1, double l_x2, double l_y2)
        {
            x1 = l_x1;
            y1 = l_y1;
            x2 = l_x2;
            y2 = l_y2;
        }

        public bool IsSameLineSegment(LineSegment l)
        {
            if (x1 ==l.x1 && y1 == l.y1 && x2==l.x2&& y2 == l.y2)
            {
                return true;
            }

            if (x1 == l.x2 && y1 == l.y2 && x2 == l.x1 && y2 == l.y1)
            {
                return true;
            }
            return false;
        }


        public double slope()
        {
            if (x2 == x1)
            {
                if (y2 > y1)
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    return double.NegativeInfinity;
                }

            }
            else
            {
                return (y2 - y1) / (x2 - x1);
            }
        }

        public static double getLength(int x1,int y1, int x2, int y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        static string intersectionPointLambdas(double x11, double y11, double x12, double y12, double x21, double y21, double x22, double y22, out double[] lambdas)
        {
            string ret = "intersect";
            lambdas = new double[2];
            lambdas[0] = 0;
            lambdas[1] = 0;

            if (x22 - x21 == 0 && x12 - x11 != 0 && y22 - y21 != 0)
            {
                lambdas[0] = (x21 - x11) / (x12 - x11);
                lambdas[1] = (lambdas[0] * (y12 - y11) - (y21 - y11)) / (y22 - y21);
            }
            else if (x22 - x21 == 0 && (x12 - x11 == 0 || y22 - y21 == 0))
            {
                ret = "nointersect";
            }
            else if (y22 - y21 == 0 && y12 - y11 != 0 && x22 - x21 != 0)
            {
                lambdas[0] = (y21 - y11) / (y12 - y11);
                lambdas[1] = (lambdas[0] * (x12 - x11) - (x21 - x11)) / (x22 - x21);
            }
            else if (y22 - y21 == 0 && (y12 - y11 == 0 || x22 - x21 == 0))
            {
                ret = "nointersect";
            }
            else if (x12 - x11 == 0 && x22 - x21 != 0 && y12 - y11 != 0)
            {
                lambdas[1] = -1.0 * (x21 - x11) / (x22 - x21);
                lambdas[0] = ((y21 - y11) + lambdas[1] * (y22 - y21)) / (y12 - y11);
            }
            else if (x12 - x11 == 0 && (x22 - x21 == 0 || y12 - y11 == 0))
            {
                ret = "nointersect";
            }
            else if (y12 - y11 == 0 && y22 - y21 != 0 && x12 - x11 != 0)
            {
                lambdas[1] = -1.0 * (y21 - y11) / (y22 - y21);
                lambdas[0] = ((x21 - x11) + lambdas[1] * (x22 - x21)) / (x12 - x11);
            }
            else if (y12 - y11 == 0 && (y22 - y21 == 0 || x12 - x11 == 0))
            {
                ret = "nointersect";
            }
            else if (((x22 - x21) / (x12 - x11)) - ((y22 - y21) / (y12 - y11)) == 0 && x12 - x11 != 0 && y12 - y11 != 0) //identical slopes
            {
                if ((x21 - x11) / (x12 - x11) == (y21 - y11) / (y12 - y11))
                {
                    ret = "overlap";
                }
                else
                {
                    ret = "nointersect";
                }
            }
            else
            {
                lambdas[1] = (((y21 - y11) / (y12 - y11)) - ((x21 - x11) / (x12 - x11))) / (((x22 - x21) / (x12 - x11)) - ((y22 - y21) / (y12 - y11)));
                lambdas[0] = (((x21 - x11) / (x22 - x21)) - ((y21 - y11) / (y22 - y21))) / (((x12 - x11) / (x22 - x21)) - ((y12 - y11) / (y22 - y21)));
            }
            return ret;
        }

        public static bool Intersect(double x11, double y11, double x12, double y12, double x21, double y21, double x22, double y22, out double[] intersectCoords)
        {
            double[] lambdas;
            intersectCoords = new double[] { -1, -1 };
            string status = intersectionPointLambdas(x11, y11, x12, y12, x21, y21, x22, y22, out lambdas);
            if (status == "intersect" && lambdas[0] >= 0 && lambdas[0] <= 1 && lambdas[1] >= 0 && lambdas[1] <= 1)
            {
                intersectCoords[0] = x11 + (x12 - x11) * lambdas[0];
                intersectCoords[1] = y11 + (y12 - y11) * lambdas[0];
                return true;
            }
            else
            {
                return false;
            }
        }

        
    }
}
