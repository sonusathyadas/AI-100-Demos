using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Security.Permissions;

namespace SpeechToText
{
    public partial class Form1 : Form
    {
        private string subscriptionKey = "233946ac44724c8783e1109e30f266be";
        private string speechRegion = "eastus";
        private SpeechConfig speechConfig;
        private SpeechRecognizer recognizer;
        TaskCompletionSource<int> stopRecognition;

        public Form1()
        {
            InitializeComponent();
            speechConfig = SpeechConfig.FromSubscription(subscriptionKey, speechRegion);
            recognizer = new SpeechRecognizer(speechConfig);
            stopRecognition = new TaskCompletionSource<int>();
        }

        private async void btnName_Click(object sender, EventArgs e)
        {
            if (btnName.Text == "Start")
            {
                btnName.Text = "Stop";                                              
                recognizer.Recognizing += (s, e) =>
                {                    
                    //Log($"{e.Result.Text}{Environment.NewLine}");
                };

                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {                        
                        Log($"{e.Result.Text}{Environment.NewLine}");
                        
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        Log($"NOMATCH: Speech could not be recognized.{Environment.NewLine}");
                    }
                };

                recognizer.Canceled += (s, e) =>
                {                    
                    Log( $"CANCELED: Reason={e.Reason}{Environment.NewLine}");

                    if (e.Reason == CancellationReason.Error)
                    {
                        Log( $"CANCELED: ErrorCode={e.ErrorCode}{Environment.NewLine}");
                        Log($"CANCELED: ErrorDetails={e.ErrorDetails}{Environment.NewLine}");
                        Log($"CANCELED: Did you update the subscription info?{Environment.NewLine}");
                    }

                    stopRecognition.TrySetResult(0);
                };

                recognizer.SessionStopped += (s, e) =>
                {
                    Log($"Session stopped event.{Environment.NewLine}");                    
                    stopRecognition.TrySetResult(0);
                };
                await recognizer.StartContinuousRecognitionAsync();                
                await Task.WhenAny(stopRecognition.Task);
            }
            else
            {
                btnName.Text = "Start";
                
                await recognizer.StopContinuousRecognitionAsync();                
            }
        }

        private void Log(string msg)
        {            
            txtResult.BeginInvoke((MethodInvoker)delegate ()
            {
                txtResult.AppendText(msg);
                txtResult.ScrollToCaret();
            });
        }

    }
}
