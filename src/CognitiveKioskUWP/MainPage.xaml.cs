﻿using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
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
using Windows.Storage;
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
        private int imageWidth = 0;
        private int imageHeight = 0;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if ((e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.CodeActivated ||
                  e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.PointerActivated) && mediaCapture != null && mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming)
            {
                await StartPreviewAsync();
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            settings = Settings.SingletonInstance;
            DisableUI();
            timerFace = new DispatcherTimer();
            timerFace.Tick += TimerFace_Tick;
            timerFace.Interval = new TimeSpan(0, 0, 1, 0);
            timerTakePicture = new DispatcherTimer();
            timerTakePicture.Tick += TimerTakePicture_Tick;
            timerTakePicture.Interval = new TimeSpan(0, 0, 0, 0, 60000/settings.FaceCVFPM);
            timerFailsafe = new DispatcherTimer();
            timerFailsafe.Tick += TimerFailsafe_Tick;
            timerFailsafe.Interval = new TimeSpan(0, 0, 10, 0, 0);
            timerFailsafe.Start();

            await StartPreviewAsync();
            InfoFadeOut.Begin();
            Window.Current.Activated += Current_Activated;
        }

        private async void TimerFailsafe_Tick(object sender, object e)
        {
            // Check for shutoff time
            if (DateTime.Now.Subtract(faceLastDate).TotalMinutes > 30)
            {
                faceLastDate = DateTime.Now;
                DisableUI();
                await StopMediaCapture();
                await StartPreviewAsync();

            }
        }

        private async Task StopMediaCapture()
        {
            try
            {
                await mediaCapture.StopPreviewAsync();

            }
            catch (Exception)
            {
            }
            try
            {
                await mediaCapture2.StopPreviewAsync();

            }
            catch (Exception)
            {
            }
            try
            {
                await mediaCapture3.StopPreviewAsync();

            }
            catch (Exception)
            {
            }
            try
            {
                await mediaCapture4.StopPreviewAsync();

            }
            catch (Exception)
            {
            }
        }

        DateTime proc = DateTime.Now;
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
                if (DateTime.Now.Subtract(faceLastDate).TotalSeconds > 30)
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
                var resolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).ToList();

                var availableResolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Cast<VideoEncodingProperties>().OrderByDescending(v => v.Width * v.Height * (v.FrameRate.Numerator / v.FrameRate.Denominator));
                //1080p or lower
                var reslution = availableResolutions.FirstOrDefault(v => v.Height <= 1080);

                // set used resolution
                await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, reslution);

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
                definition.DetectionMode = FaceDetectionMode.HighQuality;
                imageAnalysisRunning = false;

                // Add the effect to the preview stream
                _faceDetectionEffect = (FaceDetectionEffect)await mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);

                // Choose the shortest interval between detection events
                _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(300);

                // Start detecting faces
                _faceDetectionEffect.Enabled = true;

                // Register for face detection events
                _faceDetectionEffect.FaceDetected += _faceDetectionEffect_FaceDetectedAsync;
                timerFailsafe.Start();
            }
            catch (Exception)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                Console.Write("The app was denided access to the camera");
                faceLastDate = DateTime.Now.Subtract(new TimeSpan(1, 1, 1));
                return;
            }

            try
            {
                captionsControl.MainCapture.Source = mediaCapture;
                speechControl.MainCapture.Source = mediaCapture2;
                tagsControl.MainCapture.Source = mediaCapture3;
                //facesControl.MainCapture.Source = mediaCapture4;
               conversationControl.MainCapture.Source = mediaCapture4;
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
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,  async () =>
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
            tagsControl.Visibility = Visibility.Visible;
            if (await settings.HasKinect())
            {
                conversationControl.Visibility = Visibility.Visible;
            }
            else
            {
                facesControl.Visibility = Visibility.Visible;
            }
            captionsControl.Visibility = Visibility.Visible;
            speechControl.Visibility = Visibility.Visible;
            timerTakePicture.Start();
            UpdateTranslationUI($"Warming Up Translation", "");
            try
            {
                _ = Task.Factory.StartNew(StartSpeechTranslation);

                if (await settings.HasKinect())
                {
                    _ = Task.Factory.StartNew(StartSpeechConversation);
                }

            }
            catch (Exception)
            {
                // let it ride
            }
        }

        private void DisableUI()
        {
            tagsControl.Visibility = Visibility.Collapsed;
            facesControl.Visibility = Visibility.Collapsed;
            captionsControl.Visibility = Visibility.Collapsed;
            conversationControl.Visibility = Visibility.Collapsed;
            speechControl.Visibility = Visibility.Collapsed;
            speechControl.UpdateEvent(new CognitiveEvent() { ClearData = true });
            if (timerTakePicture != null)
                timerTakePicture.Stop();
            if (timerFace != null)
                timerFace.Stop();
            isFaceFound = false;
            StopSpeechTranslation();
            StopSpeechConversation();

        }

        #region Speech Translation

        bool isTranslationListening = false;
        TaskCompletionSource<int> translationStopRecognition;
        Dictionary<string, string> textLanguges = new Dictionary<string, string>() { { "es", "Spanish" }, { "zh-Hans", "Chinese" }, { "fr", "French" } };
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
                            //Debug.WriteLine($"Message received {e.Result.Text}");
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
                        //Debug.WriteLine($"Message received {e.Result.Text}");
                        if (e.Result.Translations.Count() > 0)
                        {
                            string languageLong = textLanguges[e.Result.Translations.FirstOrDefault().Key];
                            UpdateTranslationFinalUI($"{e.Result.Text}", $"{e.Result.Translations.First().Value}");
                        }
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
                speechControl.UpdateEvent(new CognitiveEvent() { PrimarySpeechMessage = messageOriginal, SecondarySpeechMessage = messageTranslation });
            }
            else
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    speechControl.UpdateEvent(new CognitiveEvent() { PrimarySpeechMessage = messageOriginal, SecondarySpeechMessage = messageTranslation });
                });
            }
        }

        private void UpdateTranslationFinalUI(string messageOriginal, string messageTranslation)
        {
            if (Dispatcher.HasThreadAccess)
            {
                speechControl.UpdateEvent(new CognitiveEvent() { PrimarySpeechMessageFinal = messageOriginal, SecondarySpeechMessageFinal = messageTranslation });
            }
            else
            {
                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    speechControl.UpdateEvent(new CognitiveEvent() { PrimarySpeechMessageFinal = messageOriginal, SecondarySpeechMessageFinal = messageTranslation });
                });
            }
        }

        #endregion



        #region Speech Conversation

        bool isConversationListening = false;
        TaskCompletionSource<int> conversationStopRecognition;

        private async Task StartSpeechConversation()
        {
            try
            {
                if (isConversationListening || string.IsNullOrEmpty(settings.SpeechKey))
                    return;

                isConversationListening = true;

                conversationStopRecognition = new TaskCompletionSource<int>();
                var subscriptionKey = settings.SpeechKey;
                var region = settings.SpeechRegion;

                var config = SpeechConfig.FromSubscription(subscriptionKey, region);
                config.SetProperty("ConversationTranscriptionInRoomAndOnline", "true");
                config.SetProperty("DifferentiateGuestSpeakers", "true");
                //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                //StorageFile logFile = await storageFolder.CreateFileAsync("logfile.txt", CreationCollisionOption.ReplaceExisting);
                //config.SetProperty(PropertyId.Speech_LogFilename, logFile.Path);

                // en-us by default. Adding this code to specify other languages, like zh-cn.
                // config.SpeechRecognitionLanguage = "zh-cn";
                config.SpeechRecognitionLanguage = "en-us";

                Debug.WriteLine($"Starting");
                MicrophoneCoordinates[] microphoneCoordinates = new MicrophoneCoordinates[7]
                {
                new MicrophoneCoordinates(0, 0, 0),
                new MicrophoneCoordinates(40, 0, 0),
                new MicrophoneCoordinates(20, -35, 0),
                new MicrophoneCoordinates(-20, -35, 0),
                new MicrophoneCoordinates(-40, 0, 0),
                new MicrophoneCoordinates(-20, 35, 0),
                new MicrophoneCoordinates(20, 35, 0)
                };
                var microphoneArrayGeometry = new MicrophoneArrayGeometry(MicrophoneArrayType.Planar, microphoneCoordinates);
                var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT, microphoneArrayGeometry);
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
                var name = devices.FirstOrDefault(x => x.Name.Contains("Kinect")).Properties.GetValueOrDefault("System.Devices.DeviceInstanceId").ToString();

                using (var audioInput = AudioConfig.FromMicrophoneInput(name.Substring(13), audioProcessingOptions))
                {
                    var meetingID = Guid.NewGuid().ToString();

                    using (var conversation = await Conversation.CreateConversationAsync(config, meetingID))
                    {
                        // create a conversation transcriber using audio stream input
                        using (var conversationTranscriber = new ConversationTranscriber(audioInput))
                        {
                            conversationTranscriber.Transcribing += (s, e) =>
                            {
                                //Debug.WriteLine($"TRANSCRIBING: Text={e.Result.Text} SpeakerId={e.Result.UserId}");
                            };

                            conversationTranscriber.Transcribed += (s, e) =>
                            {

                                try
                                {
                                    //Debug.WriteLine($"Message received {e.Result.Text}");
                                    UpdateConversationFinalUI(e.Result.Text, e.Result.UserId);

                                }
                                catch (Exception)
                                {
                                    // let it go
                                }

                                //if (e.Result.Text.ToLower().Contains("stop"))
                                //{
                                //    conversationStopRecognition.TrySetResult(0);
                                //}
                            };

                            conversationTranscriber.Canceled += (s, e) =>
                            {
                                Debug.WriteLine($"CANCELED: Reason={e.Reason}");

                                if (e.Reason == CancellationReason.Error)
                                {
                                    Debug.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                    Debug.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                    Debug.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                                    conversationStopRecognition.TrySetResult(0);
                                }
                            };

                            conversationTranscriber.SessionStarted += (s, e) =>
                            {
                                Debug.WriteLine($"\nSession started event. SessionId={e.SessionId}");
                            };

                            conversationTranscriber.SessionStopped += (s, e) =>
                            {
                                Debug.WriteLine($"\nSession stopped event. SessionId={e.SessionId}");
                                Debug.WriteLine("\nStop recognition.");
                                conversationStopRecognition.TrySetResult(0);
                            };

                            // Add participants to the conversation.
                            //var speaker1 = Participant.From("User1", "en-US", voiceSignatureStringUser1);
                            //var speaker2 = Participant.From("User2", "en-US", voiceSignatureStringUser2);
                            //await conversation.AddParticipantAsync(speaker1);
                            //await conversation.AddParticipantAsync(speaker2);

                            // Join to the conversation and start transcribing
                            await conversationTranscriber.JoinConversationAsync(conversation);
                            await conversationTranscriber.StartTranscribingAsync().ConfigureAwait(false);

                            // waits for completion, then stop transcription
                            Task.WaitAny(new[] { conversationStopRecognition.Task });
                            await conversationTranscriber.StopTranscribingAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception caught let it go!
            }


        }

        private void StopSpeechConversation()
        {
            if (conversationStopRecognition != null)
                conversationStopRecognition.TrySetResult(0);

        }

        private void UpdateConversationFinalUI(string messageOriginal, string guest)
        {
            if (Dispatcher.HasThreadAccess)
            {
                conversationControl.UpdateEvent(new CognitiveEvent() { PrimaryConversationMessageFinal = new ConversationMessage() { Message = messageOriginal, User = guest } });

            }
            else
            {

                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        conversationControl.UpdateEvent(new CognitiveEvent() { PrimaryConversationMessageFinal = new ConversationMessage() { Message = messageOriginal, User = guest } });

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
        bool imageAnalysisRunning = false;

        private async Task ProcessImage(SoftwareBitmap image)
        {
            try
            {
                Func<Task<Stream>> imageStreamCallback;

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetSoftwareBitmap(image);
                    await encoder.FlushAsync();

                    // Read the pixel bytes from the memory stream
                    using (var reader = new DataReader(stream.GetInputStreamAt(0)))
                    {
                        var bytes = new byte[stream.Size];
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(bytes);
                        imageStreamCallback = () => Task.FromResult<Stream>(new MemoryStream(bytes));
                    }
                }

            
                Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ComputerVisionClient visionClient = new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ComputerVisionClient(
                    new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(settings.ComputerVisionKey),
                    new System.Net.Http.DelegatingHandler[] { });

                // Create a prediction endpoint, passing in the obtained prediction key
                CustomVisionPredictionClient customVisionClient = null;

                if (!string.IsNullOrEmpty(settings.CustomVisionKey))
                {
                    customVisionClient = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(settings.CustomVisionKey), new System.Net.Http.DelegatingHandler[] { })
                    {
                        Endpoint = $"https://{settings.CustomVisionRegion}.api.cognitive.microsoft.com"
                    };
                }


                Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                    new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(settings.FaceKey),
                    new System.Net.Http.DelegatingHandler[] { });
                


                visionClient.Endpoint = settings.ComputerVisionEndpoint;
                faceClient.Endpoint = settings.FaceEndpoint;

                List<Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.VisualFeatureTypes?> features =
                        new List<VisualFeatureTypes?>()
                    {
                    VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                    VisualFeatureTypes.Tags, VisualFeatureTypes.Faces, VisualFeatureTypes.Brands
                    };
                // The list of Face attributes to return.
                IList<FaceAttributeType> faceAttributes =
                    new FaceAttributeType[]
                    {
            FaceAttributeType.HeadPose
                    };

                try
                {
                    if (!imageAnalysisRunning && DateTime.Now.Subtract(imageAnalysisLastDate).TotalMilliseconds > 1000)
                    {
                        imageAnalysisRunning = true;

                        _ = Task.Run(async () =>
                        {
                            ImageAnalysis analysis = await visionClient.AnalyzeImageInStreamAsync(await imageStreamCallback(), features);
                            ImagePrediction analysisCV = null;

                            try
                            {
                                if(customVisionClient != null)
                                    analysisCV = await customVisionClient.DetectImageWithNoStoreAsync(new Guid(settings.CustomVisionProjectId), settings.CustomVisionIterationName, await imageStreamCallback());

                            }
                            catch (Exception)
                            {
                                // Throw away error
                            }


                            UpdateWithAnalysis(analysis, analysisCV);

                            imageAnalysisLastDate = DateTime.Now;
                            imageAnalysisRunning = false;
                        });
                    }



                    var analysisFace = await faceClient.Face.DetectWithStreamWithHttpMessagesAsync(await imageStreamCallback(), returnFaceId: true, returnFaceAttributes: faceAttributes);
                    imageWidth = image.PixelWidth;
                    imageHeight = image.PixelHeight;
                    facesControl.UpdateEvent(new CognitiveEvent() { Faces = analysisFace.Body, ImageWidth = image.PixelWidth, ImageHeight = image.PixelHeight });

                    if (analysisFace.Body.Count() > 0 && settings.DoFaceDetection)
                    {
                        var groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                        var group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);
                        if (group != null)
                        {
                            var results = await faceClient.Face.IdentifyWithHttpMessagesAsync(analysisFace.Body.Select(x => x.FaceId.Value).ToArray(), group.PersonGroupId);
                            foreach (var identifyResult in results.Body)
                            {
                                var cand = identifyResult.Candidates.FirstOrDefault(x => x.Confidence > settings.FaceThreshold / 100d);
                                if (cand == null)
                                {
                                    Console.WriteLine("No one identified");
                                }
                                else
                                {
                                    // Get top 1 among all candidates returned
                                    var candidateId = cand.PersonId;
                                    var person = await faceClient.PersonGroupPerson.GetWithHttpMessagesAsync(group.PersonGroupId, candidateId);
                                        tagsControl.UpdateEvent(new CognitiveEvent() { IdentifiedPerson = person.Body, IdentifiedPersonPrediction = cand.Confidence });
                                        Console.WriteLine("Identified as {0}", person.Body.Name);
                                    }
                                }
                            }
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

        private void UpdateWithAnalysis(ImageAnalysis analysis, ImagePrediction analysisCV)
        {
            try
           {
                if (Dispatcher.HasThreadAccess)
                {
                    captionsControl.UpdateEvent(new CognitiveEvent() { ImageAnalysis = analysis, ImageAnalysisCV = analysisCV, ImageHeight = imageHeight, ImageWidth = imageWidth });
                    tagsControl.UpdateEvent(new CognitiveEvent() { ImageAnalysis = analysis, ImageAnalysisCV = analysisCV });
                }
                else
                {
                    var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {captionsControl.UpdateEvent(new CognitiveEvent() { ImageAnalysis = analysis, ImageAnalysisCV = analysisCV, ImageHeight = imageHeight, ImageWidth = imageWidth }); tagsControl.UpdateEvent(new CognitiveEvent() { ImageAnalysis = analysis, ImageAnalysisCV = analysisCV }); });
                }
            }
            catch (Exception)
            {

                // Eat this exception
            }
        }

        #endregion

        private async void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisableUI();
            if (timerFailsafe != null)
                timerFailsafe.Stop();
            await this.StopMediaCapture();
            Frame.Navigate(typeof(SettingsPage));
            Window.Current.Activated -= Current_Activated;

        }
    }
}
