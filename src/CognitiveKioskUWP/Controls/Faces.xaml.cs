using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class Faces : UserControl, IQuarterControl
    {
        private Settings settings;
        private System.Collections.Generic.IList<Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectedFace> lastFaces = null;
        public Faces()
        {
            this.InitializeComponent();
            settings = Settings.SingletonInstance;
        }

        public CaptureElement MainCapture { get { return captureControl; } }

        public void UpdateFaceLocation(IReadOnlyList<Windows.Media.FaceAnalysis.DetectedFace> detectedFaces, int imageHeight, int imageWidth)
        {
            if (lastFaces == null)
                return; 
            var localFaces = detectedFaces.OrderByDescending(x => x.FaceBox.Width).Take(3).OrderBy(x => x.FaceBox.X);
            int margin = (int)(imageWidth * .05) ;
            for (int i = 0; i < localFaces.Count(); i++)
            {
                var localFace = localFaces.ElementAt(i);
                for (int j = 0; j < lastFaces.Count; j++)
                {
                    var lastFace = lastFaces[j];
                    if ((lastFace.FaceRectangle.Top + margin > localFace.FaceBox.Y && localFace.FaceBox.Y > lastFace.FaceRectangle.Top - margin)
                        && (lastFace.FaceRectangle.Left + margin > localFace.FaceBox.X && localFace.FaceBox.X > lastFace.FaceRectangle.Left - margin))
                    {
                        lastFace.FaceRectangle.Top = (int)localFace.FaceBox.Y;
                        lastFace.FaceRectangle.Left = (int)localFace.FaceBox.X;
                        int convertedTop = 0;
                        int convertedLeft = 0;

                        if (localFaces.Count() > i && imageHeight > 0 && imageWidth > 0)
                        {
                            convertedTop = (((int)localFace.FaceBox.Y * (int)captureControl.ActualHeight) / imageHeight) - ((int)captureControl.Margin.Top) - ((int)ageControl1.ActualHeight);
                            convertedLeft = ((((int)localFace.FaceBox.X + ((int)localFace.FaceBox.Width / 3)) * (int)captureControl.ActualWidth) / imageWidth);
                        }
                        AgeControl ageControl = null;

                        switch (j)
                        {
                            case 0:
                                ageControl = ageControl1;
                                break;
                            case 1:
                                ageControl = ageControl2;
                                break;
                            case 2:
                                ageControl = ageControl3;
                                break;
                        }

                        if (ageControl == null)
                            break;
                        ageControl.Margin = new Thickness(convertedLeft, convertedTop, 0, 0);
                    }
                }
            }
        }
        public void UpdateEvent(CognitiveEvent mainEvent)
        {
            lastFaces = mainEvent.Faces.Take(3).OrderBy(x => x.FaceRectangle.Left).ToList();
            var faces = lastFaces;

            try
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        if(faces.Count() == 0)
                        {
                            textFaceTotal.Text = $"I don't see anyone!  Smile!";

                        }
                        else
                        {
                            textFaceTotal.Text = $"{faces.Count.ToString()} faces with {faces.Average(x => x.FaceAttributes.Emotion.Happiness).ToString("0.0%")} happiness";

                        }

                    }
                    catch (Exception)
                    {
                        textFaceTotal.Text = "";
                    }

                    textFaces1.Text = "";
                    textFaces2.Text = "";
                    textFaces3.Text = "";

                    if (faces.Count() >= 3)
                    {
                        ageControl1.Visibility = Visibility.Visible;
                        ageControl2.Visibility = Visibility.Visible;
                        ageControl3.Visibility = Visibility.Visible;
                    }
                    else if (faces.Count() >= 2)
                    {
                        ageControl1.Visibility = Visibility.Visible;
                        ageControl2.Visibility = Visibility.Visible;
                        ageControl3.Visibility = Visibility.Collapsed;

                    }
                    else if (faces.Count() >= 1)
                    {
                        ageControl1.Visibility = Visibility.Visible;
                        ageControl2.Visibility = Visibility.Collapsed;
                        ageControl3.Visibility = Visibility.Collapsed;

                    }
                    else if (faces.Count() >= 0)
                    {
                        ageControl1.Visibility = Visibility.Collapsed;
                        ageControl2.Visibility = Visibility.Collapsed;
                        ageControl3.Visibility = Visibility.Collapsed;

                    }
                    for (int i = 0; i < faces.Count(); i++)
                    {
                        var face = faces[i];
                        
                        int convertedTop = ((face.FaceRectangle.Top * (int)captureControl.ActualHeight) / mainEvent.ImageHeight) - ((int)captureControl.Margin.Top) - ((int)ageControl1.ActualHeight);
                        int convertedLeft = (((face.FaceRectangle.Left + (face.FaceRectangle.Width/3)) * (int)captureControl.ActualWidth) / mainEvent.ImageWidth);

                      

                        double maxEmotion = face.FaceAttributes.Emotion.Anger;
                        string choosenEmotion = "Anger";

                        if (face.FaceAttributes.Emotion.Contempt > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Contempt;
                            choosenEmotion = "Contempt";
                        }

                        if (face.FaceAttributes.Emotion.Disgust > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Disgust;
                            choosenEmotion = "Disgust";
                        }

                        if (face.FaceAttributes.Emotion.Fear > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Fear;
                            choosenEmotion = "Fear";
                        }

                        if (face.FaceAttributes.Emotion.Happiness > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Happiness;
                            choosenEmotion = "Happiness";
                        }

                        if (face.FaceAttributes.Emotion.Neutral > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Neutral;
                            choosenEmotion = "Neutral";
                        }

                        if (face.FaceAttributes.Emotion.Sadness > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Sadness;
                            choosenEmotion = "Sadness";
                        }

                        if (face.FaceAttributes.Emotion.Surprise > maxEmotion)
                        {
                            maxEmotion = face.FaceAttributes.Emotion.Surprise;
                            choosenEmotion = "Surprise";
                        }
                        string hair = "";
                        if (face.FaceAttributes.Hair.Bald > .6)
                            hair = "bald";
                        else
                        {
                            if (face.FaceAttributes.Hair.HairColor.Count > 0)
                                hair = face.FaceAttributes.Hair.HairColor.First().Color.ToString();
                        }
                        string userInfo = $"Emotion: {choosenEmotion} \nGlasses: {face.FaceAttributes.Glasses.ToString()} \nSmile: {face.FaceAttributes.Smile.ToString()} \nHair: {hair} ";

                        AgeControl ageControl = null;

                        switch (i)
                        {
                            case 0:
                                ageControl = ageControl1;
                                textFaces1.Text = userInfo;
                                break;
                            case 1:
                                ageControl = ageControl2;
                                textFaces2.Text = userInfo;
                                break;
                            case 2:
                                ageControl = ageControl3;
                                textFaces3.Text = userInfo;
                                break;
                        }

                        if (ageControl == null)
                            break;

                        //ageControl.Margin = new Thickness(convertedLeft, convertedTop, 0, 0);
                        ageControl.Visibility = Visibility;
                        ageControl.SetUserInfo(i + 1, face.FaceAttributes.Gender.ToString(), face.FaceAttributes.Age, choosenEmotion, face.FaceId);

                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //Eat excepiton
            }
        }

    }
}
