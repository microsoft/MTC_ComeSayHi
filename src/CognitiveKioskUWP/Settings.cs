using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace MTCSTLKiosk
{
    public class Settings
    {
        private static Settings instance = null;
        private Windows.Storage.ApplicationDataContainer localSettings;
        private List<string> regions;

        private Settings()
        {
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            regions = new List<string>() { "eastasia", "southeastasia", "australiaeast", "northeurope", "westeurope", "eastus", "eastus2", "southcentralus", "westcentralus",
                                            "westus", "westus2", "brazilsouth"};
        }


        public static Settings SingletonInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                }
                return instance;
            }
        }

        public string CameraKey
        {
            get
            {
                if (localSettings.Values.ContainsKey("CameraKey"))
                    return (string)localSettings.Values["CameraKey"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["CameraKey"] = value; }
        }



        public bool DoConversations
        {
            get
            {
                if (localSettings.Values.ContainsKey("DoConversations"))
                    return (bool)localSettings.Values["DoConversations"];
                else
                    return false;
            }
            set { localSettings.Values["DoConversations"] = value; }
        }

        public string SpeechKey
        {
            get {
                if (localSettings.Values.ContainsKey("SpeechKey"))
                    return (string)localSettings.Values["SpeechKey"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["SpeechKey"] = value; }
        }

        public string GroupName
        {
            get
            {
                return "MTC CSH FACES";
            }
        }

        public string ComputerVisionKey
        {
            get
            {
                if (localSettings.Values.ContainsKey("VisionKey"))
                    return (string)localSettings.Values["VisionKey"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["VisionKey"] = value; }
        }

        public string CustomVisionKey
        {
            get
            {
                if (localSettings.Values.ContainsKey("CustomVisionKey"))
                    return (string)localSettings.Values["CustomVisionKey"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["CustomVisionKey"] = value; }
        }

        public string CustomVisionProjectId
        {
            get
            {
                if (localSettings.Values.ContainsKey("CustomVisionProjectId"))
                    return (string)localSettings.Values["CustomVisionProjectId"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["CustomVisionProjectId"] = value; }
        }

        public string CustomVisionIterationName
        {
            get
            {
                if (localSettings.Values.ContainsKey("CustomVisionIterationName"))
                    return (string)localSettings.Values["CustomVisionIterationName"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["CustomVisionIterationName"] = value; }
        }

        public int CustomVisionThreshold
        {
            get
            {
                if (localSettings.Values.ContainsKey("CustomVisionThreshold"))
                    return (int)localSettings.Values["CustomVisionThreshold"];
                else
                    return 80;
            }
            set { localSettings.Values["CustomVisionThreshold"] = value; }
        }


        private string ComputerVisionRegion
        {
            get
            {
                if (localSettings.Values.ContainsKey("VisionRegion"))
                    return (string)localSettings.Values["VisionRegion"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["VisionRegion"] = value; }
        }

        public int FaceCVFPM
        {
            get
            {
                if (localSettings.Values.ContainsKey("FaceCVFPM"))
                    return (int)localSettings.Values["FaceCVFPM"];
                else
                    return 60;
            }
            set { localSettings.Values["FaceCVFPM"] = value; }
        }


        public string ComputerVisionEndpoint
        {
            get
            {
                if(!string.IsNullOrEmpty(ComputerVisionRegion) && ComputerVisionRegion.Length < 20)
                {
                    localSettings.Values["ComputerVisionEndpoint"] = $"https://{ComputerVisionRegion}.api.cognitive.microsoft.com";
                    ComputerVisionRegion = "";
                }
                if (localSettings.Values.ContainsKey("ComputerVisionEndpoint"))
                    return (string)localSettings.Values["ComputerVisionEndpoint"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["ComputerVisionEndpoint"] = value; }
        }


        public string CustomVisionRegion
        {
            get
            {
                if (localSettings.Values.ContainsKey("CustomVisionRegion"))
                    return (string)localSettings.Values["CustomVisionRegion"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["CustomVisionRegion"] = value; }
        }

        public string SpeechRegion
        {
            get
            {
                if (localSettings.Values.ContainsKey("SpeechRegion"))
                    return (string)localSettings.Values["SpeechRegion"];
                else
                    return string.Empty;
            }
            set { localSettings.Values["SpeechRegion"] = value; }
        }

        public List<string> Regions
        {
            get
            {
                return regions;
            }
        }

    }
}
