using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;
using HelperClasses;

namespace Utilities
{
    public class ColorSet
    {
        public static Dictionary<string, Color> colorDict = new Dictionary<string, Color>()
        {
            { "pedestrian", Color.Blue }, { "pedestrianWithStroller", Color.Blue },
            { "person", Color.Blue },
            { "bicycle", Color.Green },
            { "motorbike", Color.LightGoldenrodYellow },
            { "car", Color.Red },
            { "bus", Color.Purple },
            { "truck", Color.Orange },
            { "wheelchair", Color.Black },

            { "cat", Color.LightCoral },
            { "dog", Color.LightGreen },

            { "monitor", Color.Red },
            { "keyboard", Color.Green },

            { "table", Color.Gold},
            { "bottle", Color.LightBlue},
            { "chair" ,  Color.DarkViolet},
            { "aeroplane" ,  Color.Orange},
            
            { "bird", Color.Yellow },
            { "boat", Color.Navy },
        };

        public static Color getColorByObjectType(string type)
        {
            if (colorDict.ContainsKey(type.ToLower()))
            {
                return colorDict[type];
            }
            else
            {
                return Color.Gray;
            }
        }
    }

    public static class DrawingBoxesAndLinesOnImages
    {
        public static List<Color> Colors = new List<Color>() { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange, Color.Pink, Color.Brown, Color.Crimson, Color.DarkBlue };

       
           
            public static Image addOneRectangleToImage(Image im, Rectangle rectangle, Color color, string id, bool dashed = false)
            {
                int thickness = 5;
                float[] dashValues = { 2, 2, 2, 2 };
                Image outputImage = (Image)im.Clone();
                using (Graphics g = Graphics.FromImage(outputImage))
                {

                    Pen pen = new Pen(color, thickness);
                    if (dashed)
                    {
                        pen.DashPattern = dashValues;
                    }
                    Rectangle rect = rectangle;
                    g.DrawRectangle(pen, rect);
                    // draw the idx number for debugging
                    PointF loc = new PointF(rect.Left, rect.Top);
                    string idxString = id;
                    g.DrawString(idxString, new Font("Arial", 14), Brushes.White, loc);

                }
                return outputImage;
            }


            public static Image addRectanglesToImage(Image im, List<Rectangle> rectangles, List<Color> colors, List<string> ids, List<bool> dashed)
            {
                int thickness = 5;
                float[] dashValues = { 2, 2, 2, 2 };
                Image outputImage = (Image)im.Clone();
                using (Graphics g = Graphics.FromImage(outputImage))
                {
                    for (int i = 0; i < rectangles.Count; i++)
                    {
                        Pen pen = new Pen(colors[i], thickness);
                        if (dashed[i])
                        {
                            pen.DashPattern = dashValues;
                        }
                        Rectangle rect = rectangles[i];
                        g.DrawRectangle(pen, rect);
                        // draw the idx number for debugging
                        PointF loc = new PointF(rect.Left, rect.Top);
                        string idxString = ids[i];
                        g.DrawString(idxString, new Font("Arial", 14), Brushes.White, loc);
                    }
                }
                return outputImage;
            }


            public static Image addLinesToImage(Image im, List<LineSegment> lines, Color color, bool dashed=false)
            {
                int thickness = 5;
                float[] dashValues = { 2, 2, 2, 2 };
                Image outputImage = (Image)im.Clone();
                using (Graphics g = Graphics.FromImage(outputImage))
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        Color c = color;
                        Pen pen = new Pen(c, thickness);
                        if (dashed)
                        {
                            pen.DashPattern = dashValues;
                        }
                        Point pt1 = new Point((int)lines[i].x1, (int)lines[i].y1);
                        Point pt2 = new Point((int)lines[i].x2, (int)lines[i].y2);
                        g.DrawLine(pen, pt1, pt2);
                    }
                }
                return outputImage;

            }

        public static void DrawBoxAndSaveImage(string imageURI, string ValidationResultFolder, List<Rectangle> rectangles, List<Color> colors, List<string> ids, List<bool> dashed)
        {
            string fileName = URIUtilities.filenameFromURINoExtension(imageURI);
            Image originalImage = ImageUtilities.getImageFromURI(imageURI);
            Image imageWithBoxesAndBoundary = DrawingBoxesAndLinesOnImages.addRectanglesToImage(originalImage, rectangles, colors, ids, dashed);
            ImageUtilities.saveImage(imageWithBoxesAndBoundary, ValidationResultFolder, fileName);
        }

    }



}
