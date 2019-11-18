using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Grpc.Core;
using Tensorflow.Serving;
using System.Net;
using scoring_client;
using SatyamTaskResultClasses;
using HelperClasses;
using Tensorflow;
using Constants;

namespace TFServingClient
{
    public static class TensorflowServingClient
    {
        
        static string scoringServer = ConfigConstants.MODEL_SERVER_ADDRESS + ":8500";

        public static MultiObjectLocalizationAndLabelingResult GetImageDetectionResult(string image_url, List<string> Categories)
        {
            MultiObjectLocalizationAndLabelingResult ret = new MultiObjectLocalizationAndLabelingResult();
            int height = 0;
            int width = 0;
            PredictResponse res = ImageDetectionRequest(image_url, out height, out width);


            //float score_threshold = 
            int class_counts = res.Outputs["detection_classes"].FloatVal.Count;
            float[] classes = new float[class_counts];
            res.Outputs["detection_classes"].FloatVal.CopyTo(classes, 0);
            int box_counts = res.Outputs["detection_boxes"].FloatVal.Count;
            float[] boxes = new float[box_counts];
            res.Outputs["detection_boxes"].FloatVal.CopyTo(boxes, 0);
            int score_counts = res.Outputs["detection_scores"].FloatVal.Count;
            float[] scores = new float[score_counts];
            res.Outputs["detection_scores"].FloatVal.CopyTo(scores, 0);

            double score_threshold = 0.5;
            for (int i = 0; i < score_counts; i++)
            {
                float s = scores[i];

                if (s == 0) break;
                if (s < score_threshold) continue;

                int c = (int) classes[i]-1;
                // comes in as ymin, xmin, ymax, xmax
                int tlx = (int)(boxes[4 * i + 1] * width);
                int tly = (int)(boxes[4 * i] * height);
                int brx = (int)(boxes[4 * i + 3] * width);
                int bry = (int)(boxes[4 * i + 2] * height);
                
                MultiObjectLocalizationAndLabelingResultSingleEntry e = new MultiObjectLocalizationAndLabelingResultSingleEntry();
                e.boundingBox = new BoundingBox(tlx, tly, brx, bry);
                e.Category = Categories[c];

                ret.objects.Add(e);
            }

            ret.imageHeight = height;
            ret.imageWidth = width;
            

            return ret;
        }

        public static PredictResponse ImageDetectionRequest(string image_url, out int height, out int width)
        {
            //Create gRPC Channel
            var channel = new Channel(scoringServer, ChannelCredentials.Insecure,
                new List<Grpc.Core.ChannelOption> {
                    new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                    new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue)
                });
            var client = new PredictionService.PredictionServiceClient(channel);

            //Create prediction request
            var request = new PredictRequest()
            {
                ModelSpec = new ModelSpec() { Name = "ssd", SignatureName = "serving_default" }
            };

            //Add image tensor
            WebClient wc = new WebClient();
            byte[] data = wc.DownloadData(image_url);
            Stream stream = new MemoryStream(wc.DownloadData(image_url));
            var dimArray = ImageUtils.ConvertImageStreamToDimArrays(stream);
            height = dimArray.Length;
            width = dimArray[0].Length;
            var channels = dimArray[0][0].Length;

            var imageTensorBuilder = new TensorProto();
            var imageFeatureShape = new TensorShapeProto();

            imageFeatureShape.Dim.Add(new TensorShapeProto.Types.Dim() { Size = 1 });
            imageFeatureShape.Dim.Add(new TensorShapeProto.Types.Dim() { Size = height });
            imageFeatureShape.Dim.Add(new TensorShapeProto.Types.Dim() { Size = width });
            imageFeatureShape.Dim.Add(new TensorShapeProto.Types.Dim() { Size = channels });

            imageTensorBuilder.Dtype = DataType.DtUint8;
            imageTensorBuilder.TensorShape = imageFeatureShape;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        //imageTensorBuilder.FloatVal.Add(dimArray[i][j][c] / revertsBits);
                        imageTensorBuilder.IntVal.Add(dimArray[i][j][c]);
                    }
                }
            }
            request.Inputs.Add("inputs", imageTensorBuilder);

            //using (Stream stream = new MemoryStream(wc.DownloadData(image_url)))
            //{
            //    request.Inputs.Add("inputs", TensorBuilder.CreateTensorFromImage(stream, 1.0f));
            //}

            // Run the prediction
            var predictResponse = client.Predict(request);

            //Console.WriteLine(predictResponse.Outputs["detection_classes"]);
            //Console.WriteLine(predictResponse.Outputs["detection_boxes"]);
            //Console.WriteLine(predictResponse.Outputs["detection_scores"]);
            return predictResponse;
        }
    }
}
