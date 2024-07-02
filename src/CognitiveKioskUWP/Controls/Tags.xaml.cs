using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MTCSTLKiosk.Controls
{
    public sealed partial class Tags : UserControl, IQuarterControl
    {
        DateTime lastRefresh = DateTime.Now.AddDays(-1);
        private Settings settings = Settings.SingletonInstance;

        public Tags()
        {
            this.InitializeComponent();
        }

        public CaptureElement MainCapture { get { return captureControl; } }

        public void UpdateEvent(CognitiveEvent mainEvent)
        {
            if (lastRefresh < DateTime.Now.AddMinutes(-1))
            {
                lastRefresh = DateTime.Now;
                borderTop.Visibility = Visibility.Collapsed;
            }
            try
            {
                if (mainEvent.ImageAnalysis != null)
                {
                    textTags.Text = "Pretrained: ";
                    textTags.Text += string.Join(",", mainEvent.ImageAnalysis.Tags.Select(x => x.Name));

                    if (mainEvent.ImageAnalysis.Brands != null && mainEvent.ImageAnalysis.Brands.Count > 0)
                    {
                        textTags.Text += "\n\n" + mainEvent.ImageAnalysis.Brands.First().Name + " Logo";
                    }
                }

            }
            catch (Exception)
            {

                textTags.Text += "\n Image analysis failed";
            }
        }
    }
}
