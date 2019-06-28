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

        public System.Collections.Generic.IList<Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectedFace> Faces = new List<Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectedFace>();
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }

        public Microsoft.Azure.CognitiveServices.Vision.Face.Models.Person IdentifiedPerson { get; set; }
    }
}
