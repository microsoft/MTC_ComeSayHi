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

        public string PrimarySpeechMessage { get; set; }
        public string SecondarySpeechMessage { get; set; }
    }
}
