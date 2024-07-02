using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCSTLKiosk
{
    public class CognitiveEvent
    {
        public ImageAnalysis ImageAnalysis { get; set; }
        public bool ClearData = false;

        public string PrimarySpeechMessage { get; set; }
        public string SecondarySpeechMessage { get; set; }

        public string PrimarySpeechMessageFinal { get; set; }
        public string SecondarySpeechMessageFinal { get; set; }

        public ConversationMessage PrimaryConversationMessageFinal { get; set; }

        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }

        public double IdentifiedPersonPrediction { get; set; }
    }
}
