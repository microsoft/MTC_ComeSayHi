using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Settings settings = Settings.SingletonInstance;

        Brush brush1 = new SolidColorBrush(Windows.UI.Color.FromArgb(125, 242, 80, 34));
        Brush brush2 = new SolidColorBrush(Windows.UI.Color.FromArgb(125, 0, 164, 239));
        Brush brush3 = new SolidColorBrush(Windows.UI.Color.FromArgb(125, 127, 186, 0));
        Brush brush4 = new SolidColorBrush(Windows.UI.Color.FromArgb(125, 255, 185, 0));
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

        }

        private void UpdateBox(ObjectBox uc, int left, int top, int width, int height, string text, Brush brush)
        {
            uc.Margin = new Thickness(left, top, 0, 0);
            uc.Width = width;
            uc.Height = height;
            uc.SetDisplay(text, brush);

        }
    }
}
