using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MTCSTLKiosk
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Settings settings;

        public MainPage()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            settings = Settings.SingletonInstance;
            DisableUI();
            timerFace = new DispatcherTimer();
            timerFace.Tick += TimerFace_Tick;
            timerFace.Interval = new TimeSpan(0, 0, 2);
            timerTakePicture = new DispatcherTimer();
            timerTakePicture.Tick += TimerTakePicture_Tick;
            timerTakePicture.Interval = new TimeSpan(0, 0, 0, 1, 0);
            timerFailsafe = new DispatcherTimer();
            timerFailsafe.Tick += TimerFailsafe_Tick;
            timerFailsafe.Interval = new TimeSpan(0, 0, 10, 0, 0);
            timerFailsafe.Start();

            await StartPreviewAsync();
            InfoFadeOut.Begin();
        }

        private async void TimerFailsafe_Tick(object sender, object e)
        {
            // Check for shutoff time
            if (DateTime.Now.Subtract(faceLastDate).TotalMinutes > 5)
            {
                DisableUI();
                await StartPreviewAsync();
                
            }
        }

        private async void TimerTakePicture_Tick(object sender, object e)
        {
            try
            {
                if (isProcessingImage)
                    return;
                isProcessingImage = true;
                var image = await TakeImage();
                if (image != null)
                    await ProcessImage(image);


            }
            catch (Exception ex)
            {
                // Eat this error
                Analytics.TrackEvent(Microsoft.AppCenter.Crashes.Crashes.LogTag, new Dictionary<string, string> {
                { "Extended", ex.ToString() }
            });
            }
            finally
            {
                isProcessingImage = false;

            }

        }

        private void TimerFace_Tick(object sender, object e)
        {
            try
            {
                // Check for shutoff time
                if (DateTime.Now.Subtract(faceLastDate).TotalSeconds > 60)
                {
                    DisableUI();
                    isFaceFound = false;
                    timerFace.Stop();
                    timerTakePicture.Stop();
                }

            }
            catch (Exception)
            {
                isFaceFound = false;
            }

        }

        MediaCapture mediaCapture;
        MediaCapture mediaCapture2;
        MediaCapture mediaCapture3;
        MediaCapture mediaCapture4;
        DisplayRequest displayRequest = new DisplayRequest();
        FaceDetectionEffect _faceDetectionEffect;
        DispatcherTimer timerFace;
        DispatcherTimer timerTakePicture;
        DispatcherTimer timerFailsafe;
        DateTime faceLastDate = DateTime.Now;
        DateTime imageAnalysisLastDate = DateTime.Now;
        bool isFaceFound = false;
        bool isProcessingImage = false;

        private async Task StartPreviewAsync()
        {
            try
            {
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                var deviceList = devices.ToList();
                var device = devices.FirstOrDefault(x => x.Name.Contains(settings.CameraKey));
                string deviceId = device == null ? "" : device.Id;


                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings() { SharingMode = MediaCaptureSharingMode.ExclusiveControl, VideoDeviceId = deviceId });
                var resolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).ToList();

                Windows.Media.MediaProperties.VideoEncodingProperties reslution = (Windows.Media.MediaProperties.VideoEncodingProperties)resolutions.Where(x => x.Type == "Video").OrderByDescending(x => ((Windows.Media.MediaProperties.VideoEncodingProperties)x).Width).FirstOrDefault();


                // set used resolution
                await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, reslution);

                mediaCapture2 = new MediaCapture();
                await mediaCapture2.InitializeAsync(new MediaCaptureInitializationSettings() { SharingMode = MediaCaptureSharingMode.SharedReadOnly, VideoDeviceId = deviceId });

                mediaCapture3 = new MediaCapture();
                await mediaCapture3.InitializeAsync(new MediaCaptureInitializationSettings() { SharingMode = MediaCaptureSharingMode.SharedReadOnly, VideoDeviceId = deviceId });

                mediaCapture4 = new MediaCapture();
                await mediaCapture4.InitializeAsync(new MediaCaptureInitializationSettings() { SharingMode = MediaCaptureSharingMode.SharedReadOnly, VideoDeviceId = deviceId });

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

                // Create the definition, which will contain some initialization settings
                var definition = new FaceDetectionEffectDefinition();

                // To ensure preview smoothness, do not delay incoming samples
                definition.SynchronousDetectionEnabled = false;

                // In this scenario, choose detection speed over accuracy
                definition.DetectionMode = FaceDetectionMode.HighPerformance;

                // Add the effect to the preview stream
                _faceDetectionEffect = (FaceDetectionEffect)await mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);

                // Choose the shortest interval between detection events
                _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(300);

                // Start detecting faces
                _faceDetectionEffect.Enabled = true;

                // Register for face detection events
                _faceDetectionEffect.FaceDetected += _faceDetectionEffect_FaceDetectedAsync; ;
            }
            catch (Exception)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                Console.Write("The app was denided access to the camera");
                return;
            }

            try
            {
                captureTopLeft.Source = mediaCapture;
                captureTopRight.Source = mediaCapture2;
                captureBottomLeft.Source = mediaCapture3;
                captureBottomRight.Source = mediaCapture4;
                await mediaCapture.StartPreviewAsync();
                await mediaCapture2.StartPreviewAsync();
                await mediaCapture3.StartPreviewAsync();
                await mediaCapture4.StartPreviewAsync();
            }
            catch (Exception)
            {
                //mediaCapture.CaptureDeviceExclusiveControlStatusChanged += MediaCapture_CaptureDeviceExclusiveControlStatusChanged; ;
            }

        }

        private async void _faceDetectionEffect_FaceDetectedAsync(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            if (args.ResultFrame.DetectedFaces.Count > 0)
            {
                try
                {
                    if (!isFaceFound || DateTime.Now.Subtract(faceLastDate).TotalMinutes > 5)
                    {
                        Analytics.TrackEvent("Faces found, starting capture");
                        isFaceFound = true;
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            timerFace.Stop();
                            timerFace.Start();
                            await ActivateUI();
                        });
                    }
                    faceLastDate = DateTime.Now;

                }
                catch (Exception)
                {
                    // eat error
                }
                //await ContCapture();

            }
        }

        private async Task ActivateUI()
        {
            gridCaptureBottomLeft.Visibility = Visibility.Visible;
            gridCaptureBottomRight.Visibility = Visibility.Visible;
            gridCaptureTopLeft.Visibility = Visibility.Visible;
            gridCaptureTopRight.Visibility = Visibility.Visible;
            borderTranslation.Visibility = Visibility.Visible;
            timerTakePicture.Start();
            UpdateTranslationUI($"Warming Up Translation", "");
            try
            {
                await StartSpeechTranslation();

            }
            catch (Exception)
            {
                // let it ride
            }
        }

        private void DisableUI()
        {
            gridCaptureBottomLeft.Visibility = Visibility.Collapsed;
            gridCaptureBottomRight.Visibility = Visibility.Collapsed;
            gridCaptureTopLeft.Visibility = Visibility.Collapsed;
            gridCaptureTopRight.Visibility = Visibility.Collapsed;
            if (timerTakePicture != null)
                timerTakePicture.Stop();
            isFaceFound = false;
            StopSpeechTranslation();
        }

        #region Speech Translation

        bool isTranslationListening = false;
        TaskCompletionSource<int> translationStopRecognition;
        Dictionary<string, string> textLanguges = new Dictionary<string, string>() { { "es", "Spanish" }, { "zh-Hans", "Chinese" }, { "fr", "French" }, { "tlh", "Klingon" } };
        private async Task StartSpeechTranslation()
        {
            try
            {
                if (isTranslationListening || string.IsNullOrEmpty(settings.SpeechKey))
                    return;

                isTranslationListening = true;
                // Creates an instance of a speech factory with specified subscription key and service region.
                // Replace with your own subscription key and service region (e.g., "westus").
                var config = SpeechTranslationConfig.FromSubscription(settings.SpeechKey, settings.SpeechRegion);
                config.SpeechRecognitionLanguage = "en-US";

                translationStopRecognition = new TaskCompletionSource<int>();

                Random rand = new Random();
                string language = textLanguges.ElementAt(rand.Next(textLanguges.Keys.Count())).Key;
                
                config.AddTargetLanguage(language);
                using (var recognizer = new TranslationRecognizer(config))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        try
                        {
                            Debug.WriteLine($"Message received {e.Result.Text}");
                            string languageLong = textLanguges[e.Result.Translations.First().Key];
                            UpdateTranslationUI($"English: {e.Result.Text}", $"{languageLong}: {e.Result.Translations.First().Value}");

                        }
                        catch (Exception)
                        {
                            // let it go
                        }
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        var result = e.Result;
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        //NotifyUser($"An error occurred. Please step in front of camera to reactivate.");
                        isTranslationListening = false;
                        translationStopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        //NotifyUser($"\n    Session event. Event: {e.EventType.ToString()}.");
                        // Stops recognition when session stop is detected.

                        //NotifyUser($"\nStop recognition.");
                        isTranslationListening = false;
                        translationStopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    UpdateTranslationUI($"Warming Up Translation", "");
                    await Task.Delay(3500);
                    UpdateTranslationUI($"Say Hi!", "");

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { translationStopRecognition.Task });
                    //NotifyUser($"Stopped listenint");

                    isTranslationListening = false;

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Exception caught let it go!
            }


        }

        private void StopSpeechTranslation()
        {
            if (translationStopRecognition != null)
                translationStopRecognition.TrySetResult(0);

        }

        private void UpdateTranslationUI(string messageOriginal, string messageTranslation)
        {
            if (Dispatcher.HasThreadAccess)
            {
                textTranslation.Text = messageTranslation;
                textTranslationOriginal.Text = messageOriginal;
            }
            else
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    textTranslation.Text = messageTranslation;
                    textTranslationOriginal.Text = messageOriginal;
                });
            }
        }

        #endregion

        #region Computer Vision

        private async Task<SoftwareBitmap> TakeImage()
        {
            try
            {
                SoftwareBitmap savedImage;
                // Get information about the preview
                var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                // Create a video frame in the desired format for the preview frame
                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

                VideoFrame previewFrame = await mediaCapture.GetPreviewFrameAsync(videoFrame);

                savedImage = previewFrame.SoftwareBitmap;

                previewFrame.Dispose();
                previewFrame = null;

                return savedImage;

            }
            catch (Exception)
            {
                // eat error
            }
            return null;
        }

        private async Task ProcessImage(SoftwareBitmap image)
        {
            try
            {
                Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ComputerVisionClient visionClient = new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ComputerVisionClient(
                    new ApiKeyServiceClientCredentials(settings.VisionKey),
                    new System.Net.Http.DelegatingHandler[] { });

                Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                    new ApiKeyServiceClientCredentials(settings.FaceKey),
                    new System.Net.Http.DelegatingHandler[] { });

                visionClient.Endpoint = $"https://{settings.VisionRegion}.api.cognitive.microsoft.com";
                faceClient.Endpoint = $"https://{settings.FaceRegion}.api.cognitive.microsoft.com";

                List<VisualFeatureTypes> features =
                        new List<VisualFeatureTypes>()
                    {
                    VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                    VisualFeatureTypes.Tags, VisualFeatureTypes.Faces
                    };
                // The list of Face attributes to return.
                IList<FaceAttributeType> faceAttributes =
                    new FaceAttributeType[]
                    {
            FaceAttributeType.Gender, FaceAttributeType.Age,
            FaceAttributeType.Smile, FaceAttributeType.Emotion,
            FaceAttributeType.Glasses, FaceAttributeType.Hair
                    };

                try
                {
                    if (DateTime.Now.Subtract(imageAnalysisLastDate).TotalSeconds > 1)
                    {
                        using (var ms = new InMemoryRandomAccessStream())
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                            encoder.SetSoftwareBitmap(image);
                            await encoder.FlushAsync();

                            var analysis = await visionClient.AnalyzeImageInStreamAsync(ms.AsStream(), features);
                            UpdateWithAnalysis(analysis);
                        }
                        imageAnalysisLastDate = DateTime.Now;
                    }


                    using (var ms = new InMemoryRandomAccessStream())
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
                        encoder.SetSoftwareBitmap(image);
                        await encoder.FlushAsync();

                        var analysisFace = await faceClient.Face.DetectWithStreamWithHttpMessagesAsync(ms.AsStream(), returnFaceAttributes: faceAttributes);
                        UpdateFaces(analysisFace, image.PixelHeight, image.PixelWidth);
                    }

                }
                catch (Exception)
                {
                    // Eat exception
                }

            }
            catch (Exception)
            {
                // eat this exception too
            }
        }

        private void UpdateWithAnalysis(ImageAnalysis analysis)
        {
            try
            {
                if (analysis.Description.Captions.Count > 0)
                {
                    UpdateDescription(analysis.Description.Captions.FirstOrDefault().Text);
                }

                UpdateTags(string.Join(",", analysis.Tags.Select(x => x.Name)));
            }
            catch (Exception)
            {

                // Eat this exception
            }
        }

        private void UpdateDescription(string message)
        {

            if (Dispatcher.HasThreadAccess)
            {
                textDescription.Text = message;
            }
            else
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => textDescription.Text = message);
            }
        }
        private void UpdateFaces(Microsoft.Rest.HttpOperationResponse<System.Collections.Generic.IList<Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectedFace>> message, int imageHeight, int imageWidth)
        {
            try
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {

                    textFaces1.Text = "";
                    textFaces2.Text = "";
                    textFaces3.Text = "";

                    if (message.Body.Count() >= 3)
                    {
                        ageControl1.Visibility = Visibility.Visible;
                        ageControl2.Visibility = Visibility.Visible;
                        ageControl3.Visibility = Visibility.Visible;
                    }
                    else if (message.Body.Count() >= 2)
                    {
                        ageControl1.Visibility = Visibility.Visible;
                        ageControl2.Visibility = Visibility.Visible;
                        ageControl3.Visibility = Visibility.Collapsed;

                    }
                    else if (message.Body.Count() >= 1)
                    {
                        ageControl1.Visibility = Visibility.Visible;
                        ageControl2.Visibility = Visibility.Collapsed;
                        ageControl3.Visibility = Visibility.Collapsed;

                    }
                    else if (message.Body.Count() >= 0)
                    {
                        ageControl1.Visibility = Visibility.Collapsed;
                        ageControl2.Visibility = Visibility.Collapsed;
                        ageControl3.Visibility = Visibility.Collapsed;

                    }
                    var facesSorted = message.Body.OrderBy(x => x.FaceRectangle.Left);
                    for (int i = 0; i < facesSorted.Count(); i++)
                    {
                        var face = message.Body[i];

                        int convertedTop = ((face.FaceRectangle.Top * (int)captureBottomRight.ActualHeight) / imageHeight) - ((int)captureBottomRight.Margin.Top) - ((int)ageControl1.ActualHeight * 2);
                        int convertedLeft = ((face.FaceRectangle.Left * (int)captureBottomRight.ActualWidth) / imageWidth);

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
                            if(face.FaceAttributes.Hair.HairColor.Count > 0)
                             hair = face.FaceAttributes.Hair.HairColor.First().Color.ToString();
                        }
                        string userInfo = $"{i + 1}: {face.FaceAttributes.Gender.ToString()} ({face.FaceAttributes.Age.ToString()})\nEmotion: {choosenEmotion} \nGlasses: {face.FaceAttributes.Glasses.ToString()} \nSmile: {face.FaceAttributes.Smile.ToString()} \nHair: {hair} ";

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
        private void UpdateTags(string message)
        {

            if (Dispatcher.HasThreadAccess)
            {
                textTags.Text = message;
            }
            else
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => textTags.Text = message);
            }
        }
        #endregion

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));

        }
    }
}
