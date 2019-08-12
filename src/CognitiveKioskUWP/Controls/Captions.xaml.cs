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

            if (mainEvent.ImageAnalysisCV == null || mainEvent.ImageAnalysisCV.Predictions == null)
            {
                if (mainEvent.ImageAnalysis.Description.Captions.Count > 0)
                {
                    textDescription.Text = mainEvent.ImageAnalysis.Description.Captions.FirstOrDefault().Text;
                }
            }
            if (mainEvent.ImageAnalysisCV != null && mainEvent.ImageAnalysisCV.Predictions != null)
            {
                var preds = mainEvent.ImageAnalysisCV.Predictions.Where(x => x.Probability >= settings.CustomVisionThreshold / 100d).ToList();
                if (preds.Count() > 0)
                {
                    textDescription.Text = "Custom Vision: " + string.Join(",", preds.Select(x => x.TagName).Distinct());

                    for (int i = 0; i < preds.Count(); i++)
                    {
                        var pred = preds[i];

                        //int convertedTop = (int)((pred.BoundingBox.Top * (int)captureControl.ActualHeight) / mainEvent.ImageHeight) - ((int)captureControl.Margin.Top);
                        //int convertedLeft = (int)(((pred.BoundingBox.Left) * (int)captureControl.ActualWidth) / mainEvent.ImageWidth) - ((int)captureControl.Margin.Left);

                        int convertedTop = (int)((pred.BoundingBox.Top * captureControl.ActualHeight)) ;
                        int convertedLeft = (int)((pred.BoundingBox.Left * captureControl.ActualWidth));
                        int width = (int)(pred.BoundingBox.Width * captureControl.ActualWidth);
                        int height = (int)(pred.BoundingBox.Height * captureControl.ActualHeight);

                        ObjectBox uc = null;
                        Brush brush = null;

                        switch (i)
                        {
                            case 0:
                                uc = box1;
                                brush = brush1;
                                box1.Visibility = Visibility.Visible;
                                break;
                            case 1:
                                uc = box2;
                                brush = brush2;
                                box2.Visibility = Visibility.Visible;
                                break;
                            case 2:
                                uc = box3;
                                brush = brush3;
                                box3.Visibility = Visibility.Visible;
                                break;
                            case 3:
                                uc = box4;
                                brush = brush4;
                                box4.Visibility = Visibility.Visible;
                                break;
                        }
                        if(uc != null)
                            UpdateBox(uc, convertedLeft, convertedTop, width, height, pred.TagName + " " + pred.Probability.ToString("P"), brush);
                    }
                }
                else if (mainEvent.ImageAnalysis.Description.Captions.Count > 0)
                {
                    textDescription.Text = mainEvent.ImageAnalysis.Description.Captions.FirstOrDefault().Text;
                }
                else
                    textDescription.Text = "";

                for (int i = preds.Count(); i < 4; i++)
                {
                    switch (i)
                    {
                        case 0:
                            box1.Visibility = Visibility.Collapsed;
                            break;
                        case 1:
                            box2.Visibility = Visibility.Collapsed;
                            break;
                        case 2:
                            box3.Visibility = Visibility.Collapsed;
                            break;
                        case 3:
                            box4.Visibility = Visibility.Collapsed;
                            break;
                    }

                }
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
