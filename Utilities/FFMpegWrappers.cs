using Constants;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class FFMpegWrappers
    {
        // m3u8 doesn't support timedid naming. so just record one video at a time
        public static void StreamRecord_m3u8(String ffmpegPath, String inputfilename, 
            String cameraName, DateTime startTime, int durationSec, string outputDirectory = "\\.")
        {
            string arg = "";
            DateTime fileStartTime = startTime;
            DateTime fileEndTime = startTime.AddSeconds(durationSec);
            String startTime_string = fileStartTime.Year + "-" + fileStartTime.Month + "-" + fileStartTime.Day + "-" + fileStartTime.Hour + "-" + fileStartTime.Minute + "-" + fileStartTime.Second + "-000";
            String endTime_string = fileEndTime.Year + "-" + fileEndTime.Month + "-" + fileEndTime.Day + "-" + fileEndTime.Hour + "-" + fileEndTime.Minute + "-" + fileEndTime.Second + "-000";
            String out_file_name = "";

            out_file_name = outputDirectory + "\\" + cameraName + "_" + startTime_string + "_" + endTime_string + ".mp4";

            arg = "-i " + "\"" + inputfilename + "\"" + " -c copy -t " + durationSec + " " + out_file_name;
            

            Console.WriteLine(ffmpegPath + " " + arg);
            SystemCommands.runCommand(ffmpegPath, arg);
        }

        public static void VideoFastForwardSegement(String ffmpegPath, String inputfilename, String cameraName, DateTime startTime, DateTime endTime, int speedup, int chunkDuration, double duration = 0, string outputDirectory = "\\.")
        {
            String arg = "";

            if (speedup != 1)
            {
                //first create a speedup video
                //"ffmpeg - i input.mkv - filter:v "setpts=0.1*PTS" output.mkv"
                double speedup_param = 1 / (double)speedup;
                if (duration == 0)
                {
                    arg = "-i " + "\"" + inputfilename + "\"" + " -filter:v \"setpts=" + speedup_param + "*PTS\" " + outputDirectory + "\\temp_speeded_video_file.mp4";
                }
                else
                {
                    arg = "-i " + "\"" + inputfilename + "\"" + " -t " + duration + " -filter:v \"setpts=" + speedup_param + "*PTS\" " + outputDirectory + "\\temp_speeded_video_file.mp4";
                }

                SystemCommands.runCommand(ffmpegPath, arg);
            }

            //now take the speeded file and segment it
            TimeSpan dt = endTime - startTime;
            int numSeconds = (int)Math.Floor(dt.TotalSeconds);
            Console.WriteLine("There are " + numSeconds + " seconds");
            StringBuilder ffmpeg_segment = new StringBuilder();
            for (int t = chunkDuration / speedup; t < numSeconds / speedup; t += chunkDuration / speedup)
            {
                ffmpeg_segment.Append(t);
                if (numSeconds / speedup - t > chunkDuration / speedup)
                {
                    ffmpeg_segment.Append(",");
                }
            }

            //now segment the speeded up file
            if (speedup != 1)
            {
                if (duration == 0)
                {
                    arg = "-i " + "temp_speeded_video_file.mp4 -c copy -f segment -segment_times " + ffmpeg_segment + " -c copy -map 0  " + outputDirectory + "\\temp_output%d.mp4";
                }
                else
                {
                    arg = "-i " + "temp_speeded_video_file.mp4 -c copy -f segment -segment_times " + ffmpeg_segment + " -t " + duration + " -c copy -map 0  " + outputDirectory + "\\temp_output%d.mp4";
                }

            }
            else
            {
                if (duration == 0)
                {
                    arg = "-i " + "\"" + inputfilename + "\"" + " -c copy -f segment -segment_times " + ffmpeg_segment + " -c copy -map 0  " + outputDirectory + "\\temp_output%d.mp4";
                }
                else
                {
                    arg = "-i " + "\"" + inputfilename + "\"" + " -c copy -f segment -segment_times " + ffmpeg_segment + " -t " + duration + " -c copy -map 0  " + outputDirectory + "\\temp_output%d.mp4";
                }
            }
            Console.WriteLine(ffmpegPath + " " + arg);
            SystemCommands.runCommand(ffmpegPath, arg);
            int fileNo = 0;
            for (int t = 0; t < numSeconds; t += chunkDuration)
            {
                TimeSpan start_dt = TimeSpan.FromSeconds(t);
                TimeSpan end_dt = TimeSpan.FromSeconds(t + chunkDuration);
                DateTime fileStartTime = startTime + start_dt;
                DateTime fileEndTime = startTime + end_dt;
                String startTime_string = fileStartTime.Year + "-" + fileStartTime.Month + "-" + fileStartTime.Day + "-" + fileStartTime.Hour + "-" + fileStartTime.Minute + "-" + fileStartTime.Second + "-000";
                String endTime_string = fileEndTime.Year + "-" + fileEndTime.Month + "-" + fileEndTime.Day + "-" + fileEndTime.Hour + "-" + fileEndTime.Minute + "-" + fileEndTime.Second + "-000";
                String out_file_name = "";
                if (speedup != 1)
                {
                    out_file_name = outputDirectory + "\\" + cameraName + "_" + startTime_string + "_" + endTime_string + "_" + speedup + "X.mp4";
                }
                else
                {
                    out_file_name = outputDirectory + "\\" + cameraName + "_" + startTime_string + "_" + endTime_string + ".mp4";
                }
                String input_file_name = outputDirectory + "\\temp_output" + fileNo + ".mp4";
                arg = input_file_name + " " + out_file_name;
                fileNo++;
                System.IO.File.Move(input_file_name, out_file_name);
            }
        }

        public static void VideoFastForwardFromStream(String ffmpegPath, String inputfilename, String cameraName, DateTime startTime, int speedup, double duration, string outputDirectory = "\\.")
        {
            //String command = "ffmpeg";
            //String command = GlobalVariables.ffmpeg;
            String arg = "";
            DateTime endTime = startTime.AddSeconds(duration);

            String startTime_string = startTime.Year + "-" + startTime.Month + "-" + startTime.Day + "-" + startTime.Hour + "-" + startTime.Minute + "-" + startTime.Second + "-000";
            String endTime_string = endTime.Year + "-" + endTime.Month + "-" + endTime.Day + "-" + endTime.Hour + "-" + endTime.Minute + "-" + endTime.Second + "-000";
            String out_file_name = "";
            if (speedup != 1)
            {
                out_file_name = outputDirectory + "\\" + cameraName + "_" + startTime_string + "_" + endTime_string + "_" + speedup + "X.mp4";
            }
            else
            {
                out_file_name = outputDirectory + "\\" + cameraName + "_" + startTime_string + "_" + endTime_string + ".mp4";
            }

            if (speedup != 1)
            {
                //first create a speedup video
                //"ffmpeg - i input.mkv - filter:v "setpts=0.1*PTS" output.mkv"
                double speedup_param = 1 / (double)speedup;
                arg = "-i " + "\"" + inputfilename + "\"" + " -t " + duration + " -filter:v \"setpts=" + speedup_param + "*PTS\" " + out_file_name;
            }
            else
            {
                arg = "-i " + "\"" + inputfilename + "\"" + " -c copy " + " -t " + duration + " " + out_file_name;
            }

            Console.WriteLine(ffmpegPath + " " + arg);
            SystemCommands.runCommand(ffmpegPath, arg);
        }

        //ffmpeg -i myvideo.avi -vf fps=1/60 img%03d.jpg
        //for limited duration for live feeds
        //ffmpeg -i "rtmp://streaming.seattle.gov:1935/live?evmtestoken/1_Stewart_NS.stream" -t 1 -vf fps = 4 "D:\Bellevue\Bellevue_Video_Frames\live\tmpimg%06d.jpg"
        public static void ExtractFrames(String ffmpegpath, String inputFileName, String outputDirectory, String cameraName, DateTime startTime, double fps=-1, double duration = 0)
        {
            


            String arg = "";
            if (duration == 0)
            {
                arg = "-i " + "\"" + inputFileName + "\"" + " -vf fps=" + fps + " " + "\"" + outputDirectory + "\\" + cameraName + "-%06d.jpg\"";
                //arg = "-i " + "\"" + inputFileName + "\"" + " -vf fps=" + fps + " " +  "tmpimg%06d.jpg";
            }
            else
            {
                arg = "-i " + "\"" + inputFileName + "\"" + " -t " + duration + " -vf fps=" + fps + " " + "\"" + outputDirectory + "\\" + cameraName + "-%06d.jpg\"";
            }


            String wholeCommand = ffmpegpath + " " + arg;
            Console.WriteLine(wholeCommand);
            SystemCommands.ExecuteCommandSync(wholeCommand);
            
        }

        //ffmpeg -i myvideo.avi -vf fps=1/60 img%03d.jpg
        //for limited duration for live feeds
        //ffmpeg -i "rtmp://streaming.seattle.gov:1935/live?evmtestoken/1_Stewart_NS.stream" -t 1 -vf fps = 4 "D:\Bellevue\Bellevue_Video_Frames\live\tmpimg%06d.jpg"
        public static void ExtractFramesFromStream(String ffmpegpath, String inputFileName, String outputDirectory, String cameraName, DateTime startTime, double fps, double duration)
        {
            //String command = GlobalVariables.ffmpeg;
            String arg = "";

            arg = "-i " + "\"" + inputFileName + "\"" + " -t " + duration + " -vf fps=" + fps + " " + "\"" + outputDirectory + "\\tmpimg%06d.jpg\"";

            String wholeCommand = ffmpegpath + " " + arg;
            Console.WriteLine(wholeCommand);
            SystemCommands.ExecuteCommandSync(wholeCommand);

            //now change the names of the files
            bool done = false;
            int no = 1;
            double samplingInterval = 1000 * (1.0 / (double)fps); //in ms


            do
            {
                string input_file_name = "\"" + outputDirectory + "\\tmpimg" + no.ToString("000000") + ".jpg\"";
                //string input_file_name = "tmpimg" + no.ToString("000000") + ".jpg";
                Console.WriteLine("Moving filename " + input_file_name);

                double nomilliseconds = (no - 1) * samplingInterval;
                int noHrs = (int)Math.Floor(nomilliseconds / (3600 * 1000));
                double timeSoFar = noHrs * 3600 * 1000;
                int noMins = (int)Math.Floor((nomilliseconds - timeSoFar) / (1000 * 60));
                timeSoFar += noMins * 60 * 1000;
                int noSecs = (int)Math.Floor((nomilliseconds - timeSoFar) / 1000);
                timeSoFar += noSecs * 1000;
                int noms = (int)Math.Floor(nomilliseconds - timeSoFar);
                Console.WriteLine("computer timespan = " + noHrs + ":" + noMins + ":" + noSecs + ":" + noms);
                TimeSpan dt = new TimeSpan(0, noHrs, noMins, noSecs, noms);
                Console.WriteLine(dt.ToString());
                DateTime frameTime = startTime + dt;
                Console.WriteLine(frameTime);
                //String frameTimeString = FileNameUtilities.convertDateTimeToString(frameTime);
                String frameTimeString = frameTime.ToString();
                //String out_file_name = "\"" + outputDirectory +  "\\" + cameraName + "_" + frameTimeString + ".jpg\"";
                String out_file_name = cameraName + "_" + frameTimeString + ".jpg";
                //Console.WriteLine("Moving filename " + input_file_name + "--->" + @out_file_name);
                //System.IO.File.Move(input_file_name, out_file_name);   

                String command = "rename " + input_file_name + " " + out_file_name;
                Console.WriteLine(command);
                SystemCommands.ExecuteCommandSync(command);

                no++;
                if (no > duration * fps)
                {
                    break;
                }
            } while (!done);
        }

        //this is temporary solution needs to be fixed
        //ffmpeg -framerate 1/5 -i img%03d.png -c:v libx264 -r 30 -pix_fmt yuv420p out.mp4
        public static void generateVideoFromFrames(List<Image> images, String videoName, string directory)
        {
            for (int i = 0; i < images.Count; i++)
            {
                images[i].Save("img" + i.ToString("000") + ".jpg");
            }
            string command = DirectoryConstants.ffmpeg + " -i img%03d.jpg " + directory + "\\" + videoName + ".mp4";
            Console.WriteLine(command);
            SystemCommands.ExecuteCommandSync(command);
            command = "del img*.jpg";
            SystemCommands.ExecuteCommandSync(command);
            Console.WriteLine("done");
        }


        public static void generateVideoFromFolderofFrames(string videoName, string directory)
        {
            
            string command = DirectoryConstants.ffmpeg + " -i " + directory + "img%03d.jpg " + directory + "\\" + videoName + ".mp4";
            Console.WriteLine(command);
            SystemCommands.ExecuteCommandSync(command);
            Console.WriteLine("done");
        }

        public static void ConvertVideoURLToMP4(String ffmpegPath, string inputFile, string outputFolderPath)
        {
            string fileName = URIUtilities.filenameFromURINoExtension(inputFile);
            string command = ffmpegPath + " -i " + inputFile + " " + outputFolderPath + "\\" + fileName + ".mp4";
            Console.WriteLine(command);
            SystemCommands.ExecuteCommandSync(command);
        }

        public static void ConvertLocalVideoToMP4(String ffmpegPath, string inputFile, string outputFolderPath)
        {
            string fileName = URIUtilities.filenameFromDirectoryNoExtension(inputFile);
            string command = ffmpegPath + " -i " + inputFile + " " + outputFolderPath + "\\" + fileName + ".mp4";
            SystemCommands.ExecuteCommandSync(command);
        }

        public static void ConvertLocalVideoFolderToMP4(String ffmpegPath, string inputFolder, string outputFolder)
        {
            System.IO.Directory.CreateDirectory(outputFolder);
            string[] files = System.IO.Directory.GetFiles(inputFolder);
            foreach (string file in files)
            {   
                ConvertLocalVideoToMP4(ffmpegPath, file, outputFolder);
            }
        }
    }
}
