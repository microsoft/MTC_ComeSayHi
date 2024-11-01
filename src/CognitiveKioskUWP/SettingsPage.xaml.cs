﻿using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
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

            textVisionAPIKey.Text = settings.ComputerVisionKey;
            textSpeechAPIKey.Text = settings.SpeechKey;
            textFramesPerMinute.Text = settings.FaceCVFPM.ToString();
            textComputerVisionEndpoint.Text = settings.ComputerVisionEndpoint;
            textOpenAIKey.Text = settings.OpenAIKey;
            textOpenAIModel.Text = settings.OpenAIModel;
            textOpenAIUrl.Text = settings.OpenAIUrl;
            textOpenAIPrompt.Text = settings.OpenAIImagePrompt;




            dropdownRegion.SelectedValue = settings.SpeechRegion;



            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            dropdownCamera.ItemsSource = devices.Select(x => x.Name).ToArray();

            dropdownCamera.SelectedValue = settings.CameraKey;

            textSettings.Text = "Settings v." + GetAppVersion();
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
            settings.ComputerVisionKey = textVisionAPIKey.Text;
        }

        private void dropdownRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.SpeechRegion = (string)dropdownRegion.SelectedValue;
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



        private void TextFramesPerMinute_TextChanged(object sender, TextChangedEventArgs e)
        {
            int fpm = 60;
            int.TryParse(textFramesPerMinute.Text, out fpm);
            settings.FaceCVFPM = fpm;

        }


        private void TextComputerVisionEndpoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.ComputerVisionEndpoint = textComputerVisionEndpoint.Text;
        }

        private void textOpenAIUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.OpenAIUrl = textOpenAIUrl.Text;
        }

        private void textOpenAIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.OpenAIKey = textOpenAIKey.Text;
        }

        private void textOpenAIModel_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.OpenAIModel = textOpenAIModel.Text;
        }

        private void textOpenAIPrompt_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.OpenAIImagePrompt = textOpenAIPrompt.Text;
        }
    }
}
