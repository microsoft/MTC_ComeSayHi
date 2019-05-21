using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class Captions : UserControl, IQuarterControl
    {
        public Captions()
        {
            this.InitializeComponent();
        }

        public CaptureElement MainCapture { get { return captureControl; } }

        public void UpdateEvent(CognitiveEvent mainEvent)
        {
          
            if (mainEvent.ImageAnalysis.Description.Captions.Count > 0)
            {
                textDescription.Text = mainEvent.ImageAnalysis.Description.Captions.FirstOrDefault().Text;
            }
            if (mainEvent.ImageAnalysis.Brands != null && mainEvent.ImageAnalysis.Brands.Count > 0)
            {
                textDescription.Text += "\n\n" + mainEvent.ImageAnalysis.Brands.First().Name + " Logo";
            }
        }
    }
}
