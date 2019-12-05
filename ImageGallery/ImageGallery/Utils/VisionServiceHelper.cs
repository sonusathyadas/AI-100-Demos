using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.ProjectOxford.Vision;

using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ImageGallery.Utils
{
    public class VisionServiceHelper
    {
        public static int RetryCountOnQuotaLimitError = 6;
        public static int RetryDelayOnQuotaLimitError = 500;
        private static VisionServiceClient visionClient { get; set; }
        public static Action Throttled;
        private static string apiKey;
        private static string endPoint { get; set; }

        static VisionServiceHelper()
        {
            InitializeVisionService();
        }

        public static string ApiKey
        {
            get
            {
                return apiKey;
            }

            set
            {
                var changed = apiKey != value;
                apiKey = value;
                if (changed)
                {
                    InitializeVisionService();
                }
            }
        }

        public static string EndPoint
        {
            get { return endPoint; }
            set
            {
                var changed = endPoint != value;
                endPoint = value;
                if (changed)
                {
                    InitializeVisionService();
                }
            }
        }

        private static void InitializeVisionService()
        {
            visionClient = new VisionServiceClient(apiKey, endPoint);
        }

        // handle throttling issues
        private static async Task<TResponse> RunTaskWithAutoRetryOnQuotaLimitExceededError<TResponse>(Func<Task<TResponse>> action)
        {
            int retriesLeft = VisionServiceHelper.RetryCountOnQuotaLimitError;
            int delay = VisionServiceHelper.RetryDelayOnQuotaLimitError;

            TResponse response = default(TResponse);

            while (true)
            {
                try
                {
                    response = await action();
                    break;
                }
                catch (Microsoft.ProjectOxford.Vision.ClientException exception) when (exception.HttpStatus == (System.Net.HttpStatusCode)429 && retriesLeft > 0)
                {
                    ErrorTrackingHelper.TrackException(exception, "Vision API throttling error");
                    if (retriesLeft == 1 && Throttled != null)
                    {
                        Throttled();
                    }

                    await Task.Delay(delay);
                    retriesLeft--;
                    delay *= 2;
                    continue;
                }
            }

            return response;
        }

        public static async Task<AnalysisResult> AnalyzeImageAsync(string imageUrl, IEnumerable<VisualFeature> visualFeatures = null, IEnumerable<string> details = null)
        {
            return await RunTaskWithAutoRetryOnQuotaLimitExceededError<AnalysisResult>(() => visionClient.AnalyzeImageAsync(imageUrl, visualFeatures, details));
        }
        public static async Task<AnalysisResult> AnalyzeImageAsync(Func<Task<Stream>> imageStreamCallback, IEnumerable<VisualFeature> visualFeatures = null, IEnumerable<string> details = null)
        {
            return await RunTaskWithAutoRetryOnQuotaLimitExceededError<AnalysisResult>(async () => await visionClient.AnalyzeImageAsync(await imageStreamCallback(), visualFeatures, details));
        }
    }
}