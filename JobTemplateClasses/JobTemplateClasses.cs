using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperClasses;

namespace JobTemplateClasses
{

   

    public class SingleObjectLabelingSubmittedJob
    {        
        public List<string> Categories;
        public string Description;        
    }

    public class ObjectCountingSubmittedJob
    {
        public string ObjectName;
        public string Description;
    }

    public class MultiObjectLocalizationAndLabelingSubmittedJob
    {
        public List<string> Categories;
        public string Description;
        public List<LineSegment> BoundaryLines;
    }

    public class ImageSegmentationSubmittedJob
    {
        public List<string> Categories;
        public string Description;
        public List<LineSegment> BoundaryLines;
    }

    public class MultiObjectTrackingSubmittedJob
    {
        public Dictionary<string, List<String>> Categories;
        public string Description;
        public List<LineSegment> BoundaryLines;
        public string DataSrcFormat;
        public int ChunkDuration;//seconds
        public int FrameRate;
        public double ChunkOverlap;
    }



}
