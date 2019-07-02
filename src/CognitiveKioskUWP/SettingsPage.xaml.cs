using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MTCSTLKiosk
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Settings settings = Settings.SingletonInstance;

        bool groupsLoading = false;
        public SettingsPage()
        {
            this.InitializeComponent();
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            dropdownRegion.ItemsSource = settings.Regions.ToArray();
            dropdownFaceRegion.ItemsSource = settings.Regions.ToArray();
            dropdownVisionRegion.ItemsSource = settings.Regions.ToArray();
            dropdownCustomVisionRegion.ItemsSource = settings.Regions.ToArray();

            textFaceAPIKey.Text = settings.FaceKey;
            textVisionAPIKey.Text = settings.VisionKey;
            textSpeechAPIKey.Text = settings.SpeechKey;
            textCustomVisionAPIKey.Text = settings.CustomVisionKey;

            textCustomVisionProjectID.Text = settings.CustomVisionProjectId;
            textCustomVisionIterationName.Text = settings.CustomVisionIterationName;
            sliderCustomVision.Value = settings.CustomVisionThreshold;
            sliderFaceDetect.Value = settings.FaceThreshold;

            dropdownRegion.SelectedValue = settings.SpeechRegion;
            dropdownVisionRegion.SelectedValue = settings.VisionRegion;
            dropdownFaceRegion.SelectedValue = settings.FaceRegion;
            dropdownCustomVisionRegion.SelectedValue = settings.CustomVisionRegion;

            toggleShowAge.IsOn = settings.ShowAgeAndGender;
            toggleFaceDetect.IsOn = settings.DoFaceDetection;


            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            dropdownCamera.ItemsSource = devices.Select(x => x.Name).ToArray();

            dropdownCamera.SelectedValue = settings.CameraKey;

            textSettings.Text = "Settings v." + GetAppVersion();
            await BindFaces();
        }

        private async System.Threading.Tasks.Task BindFaces()
        {
            if (groupsLoading)
                return;
            try
            {
                groupsLoading = true;
                Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                    new ApiKeyServiceClientCredentials(settings.FaceKey),
                    new System.Net.Http.DelegatingHandler[] { });

                faceClient.Endpoint = $"https://{settings.FaceRegion}.api.cognitive.microsoft.com";

                var groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                var group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);

                if (group == null)
                {
                   var groupRet = await faceClient.PersonGroup.CreateWithHttpMessagesAsync(Guid.NewGuid().ToString(), settings.GroupName);
                     groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                     group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);
                }

                var perople = await faceClient.PersonGroupPerson.ListWithHttpMessagesAsync(group.PersonGroupId);

                dropdownPerson.ItemsSource = perople.Body.Select(x => x.Name).ToArray();
            }
            catch (Exception)
            {

            }
            groupsLoading = false;
        }

        private async System.Threading.Tasks.Task AddPerson(string person)
        {
            try
            {

                Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                    new ApiKeyServiceClientCredentials(settings.FaceKey),
                    new System.Net.Http.DelegatingHandler[] { });

                faceClient.Endpoint = $"https://{settings.FaceRegion}.api.cognitive.microsoft.com";

                var groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                var group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);

                var perople = await faceClient.PersonGroupPerson.ListWithHttpMessagesAsync(group.PersonGroupId);

                if (perople.Body.Count(x=>x.Name == person) == 0)
                {
                    await faceClient.PersonGroupPerson.CreateWithHttpMessagesAsync(group.PersonGroupId, person);
                }
                await BindFaces();
                dropdownPerson.SelectedValue = person;
            }
            catch (Exception)
            {

            }
        }

        private async System.Threading.Tasks.Task DeletePerson(string person)
        {
            try
            {

                Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                    new ApiKeyServiceClientCredentials(settings.FaceKey),
                    new System.Net.Http.DelegatingHandler[] { });

                faceClient.Endpoint = $"https://{settings.FaceRegion}.api.cognitive.microsoft.com";

                var groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                var group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);

                var people = await faceClient.PersonGroupPerson.ListWithHttpMessagesAsync(group.PersonGroupId);

                var personObject = people.Body.First(x => x.Name == person);
                await faceClient.PersonGroupPerson.DeleteWithHttpMessagesAsync(group.PersonGroupId, personObject.PersonId);
                await BindFaces();
            }
            catch (Exception)
            {

            }
        }

        private async System.Threading.Tasks.Task UpdatePerson(string person)
        {
            try
            {

                Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                    new ApiKeyServiceClientCredentials(settings.FaceKey),
                    new System.Net.Http.DelegatingHandler[] { });

                faceClient.Endpoint = $"https://{settings.FaceRegion}.api.cognitive.microsoft.com";

                var groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                var group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);

                var people = await faceClient.PersonGroupPerson.ListWithHttpMessagesAsync(group.PersonGroupId);
                var personObject = people.Body.First(x => x.Name == person);

                var trainingStatus = await faceClient.PersonGroup.GetTrainingStatusWithHttpMessagesAsync(group.PersonGroupId);
                txtPersonFaceInfo.Text = $"{personObject.Name} has {personObject.PersistedFaceIds.Count()} faces : training status is {trainingStatus.Body.Status.ToString()}";
            }
            catch (Exception)
            {

            }
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void textSpeechAPIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.SpeechKey = textSpeechAPIKey.Text;
        }

        private async void textVisionAPIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.VisionKey = textVisionAPIKey.Text;
            await BindFaces();
        }

        private void dropdownRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.SpeechRegion = (string)dropdownRegion.SelectedValue;
        }

        private async void dropdownFaceRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.FaceRegion = (string)dropdownFaceRegion.SelectedValue;
            await BindFaces();

        }

        private void dropdownVisionRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.VisionRegion = (string)dropdownVisionRegion.SelectedValue;

        }

        private void textFaceAPIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.FaceKey = textFaceAPIKey.Text;
        }

        private void dropdownCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.CameraKey = (string)dropdownCamera.SelectedValue;
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

            // Encode subject and body of the email so that it at least largely 
            // corresponds to the mailto protocol (that expects a percent encoding 
            // for certain special characters)
            string encodedSubject = WebUtility.UrlEncode("Come say hi input").Replace("+", " ");
            string encodedBody = WebUtility.UrlEncode("All the good stuff").Replace("+", " ");

            // Create a mailto URI
            Uri mailtoUri = new Uri("mailto:" + "brrobe@microsoft.com" + "?subject=" +
               encodedSubject +
               "&body=" + encodedBody);

            // Execute the default application for the mailto protocol
            await Launcher.LaunchUriAsync(mailtoUri);
        }

        private void ToggleShowAge_Toggled(object sender, RoutedEventArgs e)
        {

            settings.ShowAgeAndGender = toggleShowAge.IsOn;
        }

        private async void ButtonAddPerson_Click(object sender, RoutedEventArgs e)
        {
            await AddPerson(textNewPerson.Text);
            textNewPerson.Text = "";
        }

        private async void DropdownPerson_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dropdownPerson.SelectedItem != null)
                await UpdatePerson(dropdownPerson.SelectedItem.ToString());
        }

        private async void ButtonAddImages_Click(object sender, RoutedEventArgs e)
        {
            await UploadPics();
        }

        private async System.Threading.Tasks.Task UploadPics()
        {
            try
            {
                txtPersonFaceInfo.Text = "Uploading";
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                var files = await picker.PickMultipleFilesAsync();
                if (files != null)
                {

                    Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient faceClient = new Microsoft.Azure.CognitiveServices.Vision.Face.FaceClient(
                        new ApiKeyServiceClientCredentials(settings.FaceKey),
                        new System.Net.Http.DelegatingHandler[] { });

                    faceClient.Endpoint = $"https://{settings.FaceRegion}.api.cognitive.microsoft.com";

                    var groups = await faceClient.PersonGroup.ListWithHttpMessagesAsync();
                    var group = groups.Body.FirstOrDefault(x => x.Name == settings.GroupName);

                    var people = await faceClient.PersonGroupPerson.ListWithHttpMessagesAsync(group.PersonGroupId);
                    var personObject = people.Body.First(x => x.Name == dropdownPerson.SelectedItem.ToString());

                    foreach (var file in files)
                    {
                        var s = await file.OpenReadAsync();

                        // Detect faces in the image and add to Anna
                        await faceClient.PersonGroupPerson.AddFaceFromStreamWithHttpMessagesAsync(group.PersonGroupId, personObject.PersonId, s.AsStream());
                    }
                    await UpdatePerson(dropdownPerson.SelectedItem.ToString());
                    
                    await faceClient.PersonGroup.TrainWithHttpMessagesAsync(group.PersonGroupId);
                }
                else
                {
                    txtPersonFaceInfo.Text = "No Files";
                }

            }
            catch (Exception ex)
            {
                txtPersonFaceInfo.Text = "Upload Error: " + ex.Message;
            }
        }
        private async void ButtonDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Are you sure you want to delete?", "DELETE");

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(
                "Yes (Delete)",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(
                "No",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();

        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            if(command.Label != "No")
            {
                await DeletePerson(dropdownPerson.SelectedItem.ToString());
            }
        }

        private void TextCustomVisionAPIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
          settings.CustomVisionKey =  textCustomVisionAPIKey.Text;
        }

        private void DropdownCustomVisionRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.CustomVisionRegion = (string)dropdownCustomVisionRegion.SelectedValue;

        }

        private void TextCustomVisionProjectID_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.CustomVisionProjectId = textCustomVisionProjectID.Text;
        }

        private void TextCustomVisionIterationName_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.CustomVisionIterationName = textCustomVisionIterationName.Text;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.CustomVisionThreshold = (int)sliderCustomVision.Value;
        }

        private void ToggleFaceDetect_Toggled(object sender, RoutedEventArgs e)
        {
            settings.DoFaceDetection = toggleFaceDetect.IsOn;
        }

        private void SliderFaceDetect_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            settings.FaceThreshold = (int)sliderFaceDetect.Value;
        }
    }
}
