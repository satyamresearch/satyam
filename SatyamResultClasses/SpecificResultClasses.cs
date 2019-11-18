using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperClasses;

namespace SatyamTaskResultClasses
{
    public class SingleObjectLabelingResult
    {
        public string Category;
    }

    public class ObjectCountingResult
    {
        public int Count;
    }


    public class MultiObjectLocalizationAndLabelingResultSingleEntry
    {
        public BoundingBox boundingBox;
        public string Category;
    }

    public class MultiObjectLocalizationAndLabelingResult
    {
        public List<MultiObjectLocalizationAndLabelingResultSingleEntry> objects;
        public double displayScaleReductionX; //the scale at which the image was displayed to the user
        public double displayScaleReductionY; //the scale at which the image was displayed to the user
        public int imageWidth; //the original image width
        public int imageHeight;//the original imageheight

        public MultiObjectLocalizationAndLabelingResult()
        {
            objects = new List<MultiObjectLocalizationAndLabelingResultSingleEntry>();
        }
    }


    public class ImageSegmentationResultSingleEntry_NoHoles
    {
        public GenericPolygon polygon;
        public string Category;
    }

    public class ImageSegmentationResult_NoHoles
    {
        public List<ImageSegmentationResultSingleEntry_NoHoles> objects;
        public double displayScaleReductionX; //the scale at which the image was displayed to the user
        public double displayScaleReductionY; //the scale at which the image was displayed to the user
        public int imageWidth; //the original image width
        public int imageHeight;//the original imageheight
    }


    public class ImageSegmentationResultSingleEntry
    {
        public Segment segment;
        public string Category;
    }

    public class ImageSegmentationResult
    {
        public List<ImageSegmentationResultSingleEntry> objects;
        public double displayScaleReductionX; //the scale at which the image was displayed to the user
        public double displayScaleReductionY; //the scale at which the image was displayed to the user
        public int imageWidth; //the original image width
        public int imageHeight;//the original imageheight

        public static byte[] PolygonResult2Bitmap(ImageSegmentationResult res)
        {
            int ImageWidth = res.imageWidth;
            int ImageHeight = res.imageHeight;


            //auto padding data to make the stride a multiple of 4. required by bmpdata for output
            if (ImageWidth % 4 != 0)
            {
                ImageWidth = (ImageWidth / 4+1) * 4;
            }

            byte[] pixels = new byte[ImageWidth * ImageHeight];
            for (int i = 0; i < ImageWidth; i++)
            {
                for (int j = 0; j < ImageHeight; j++)
                {
                    bool In = false;
                    for (int k = 0; k < res.objects.Count; k++)
                    {
                        Segment seg = res.objects[k].segment;
                        if (seg.PointIsInSegment(i, j))
                        {
                            pixels[j * ImageWidth + i] = (byte)(k + 1);
                            In = true;
                            break;
                        }
                    }
                    if (!In)
                    {
                        pixels[j * ImageWidth + i] = (byte)(0);
                    }
                }
            }
            return pixels;
        }
    }


    public class OpenEndedQuestionResult
    {
        public string Q1Answer;
        public string Q2Answer;
        public string Q3Answer;
        
        public bool Online=false;
        public bool InPerson=false;

        public List<string> SharedTo= new List<string>();
    }

    /// <summary>
    /// CameraPoseAnnotationResult
    /// </summary>
    public class CameraPoseAnnotationResult
    {
        public CamLocResultsSingleEntry objects;
        public double displayScaleReductionX; //the scale at which the image was displayed to the user
        public double displayScaleReductionY; //the scale at which the image was displayed to the user
        public int imageWidth; //the original image width
        public int imageHeight;//the original imageheight
    }

    public class CamLocResultsSingleEntry
    {
        public VP_point_list caxis;
        public VP_point_list xvppoints;
        public VP_point_list yvppoints;
        public VP_point_list zvppoints;
    }

    public class VP_point_list
    {
        public List<int[]> vertices = new List<int[]>();
        public VP_point_list() { }
        public VP_point_list(List<int[]> _vertices){vertices = _vertices;}
    }


}
