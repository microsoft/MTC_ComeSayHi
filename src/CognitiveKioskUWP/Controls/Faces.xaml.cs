﻿using System;
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
        public Faces()
        {
            this.InitializeComponent();
            settings = Settings.SingletonInstance;
        }

        public CaptureElement MainCapture { get { return captureControl; } }
        

        public void UpdateEvent(CognitiveEvent mainEvent)
        {
            var faces = mainEvent.Faces;

            try
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    try
                    {
                        if (faces.Count() == 0)
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
                    var facesSorted = faces.Take(3).OrderBy(x => x.FaceRectangle.Left).ToList();
                    for (int i = 0; i < facesSorted.Count(); i++)
                    {
                        var face = facesSorted[i];

                        int convertedTop = ((face.FaceRectangle.Top * (int)captureControl.ActualHeight) / mainEvent.ImageHeight) - ((int)captureControl.Margin.Top) - ((int)ageControl1.ActualHeight);
                        int convertedLeft = (((face.FaceRectangle.Left + (face.FaceRectangle.Width / 4)) * (int)captureControl.ActualWidth) / mainEvent.ImageWidth);

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
                        string mask = "n/a";
                        if(face.FaceAttributes.Accessories != null)
                        {
                            mask = (face.FaceAttributes.Accessories.Count(x=>x.Type == Microsoft.Azure.CognitiveServices.Vision.Face.Models.AccessoryType.Mask) > 0).ToString();
                        }
                        string userInfo = $"Emotion: {choosenEmotion} \nGlasses: {face.FaceAttributes.Glasses.ToString()} \nSmile: {face.FaceAttributes.Smile.ToString()} \nHair: {hair} \nMask: {mask} ";

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

                        ageControl.Margin = new Thickness(convertedLeft, convertedTop, 0, 0);
                        ageControl.Visibility = Visibility;
                        ageControl.SetUserInfo(i + 1, face.FaceAttributes.Gender.ToString(), face.FaceAttributes.Age, choosenEmotion);

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
