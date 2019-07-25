using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
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

namespace MTCSTLKiosk
{
    public sealed partial class AgeControl : UserControl
    {
        private Settings settings;
        public AgeControl()
        {
            this.InitializeComponent();
            settings = Settings.SingletonInstance;
        }

        private string emojiSad = char.ConvertFromUtf32(0x1f62d);
        private string emojiAnger = char.ConvertFromUtf32(0x1f92c);
        private string emojiContempt = char.ConvertFromUtf32(0x1f612);
        private string emojiDisgust = char.ConvertFromUtf32(0x1f922);
        private string emojiFear = char.ConvertFromUtf32(0x1f631);
        private string emojiHappiness = char.ConvertFromUtf32(0x1f600);
        private string emojiNeutral = char.ConvertFromUtf32(0x1f610);
        private string emojiSuprise = char.ConvertFromUtf32(0x1f632);

        public void SetUserInfo(int number, string sex, double? age, string  emotion)
        {

            if (settings.ShowAgeAndGender)
            {
                gridEmoji.Visibility = Visibility.Collapsed;
                gridAgeIncluded.Visibility = Visibility.Visible;

                this.Height = 25;
                this.Width = 120;

                textUser.Text = sex + " (" + age.ToString() + ")";
            }
            else
            {
                gridAgeIncluded.Visibility = Visibility.Collapsed;
                gridEmoji.Visibility = Visibility.Visible;

                textUser.Text = emotion;
            }

            if (emotion == "Anger")
            {
                textSymbol.Text = emojiAnger;
            }
            if (emotion == "Contempt")
            {
                textSymbol.Text = emojiContempt;
            }
            if (emotion == "Disgust")
            {
                textSymbol.Text = emojiDisgust;
            }
            if (emotion == "Fear")
            {
                textSymbol.Text = emojiFear;
            }
            if (emotion == "Happiness")
            {
                textSymbol.Text = emojiHappiness;
            }
            if (emotion == "Neutral")
            {
                textSymbol.Text = emojiNeutral;
            }
            if (emotion == "Sadness")
            {
                textSymbol.Text = emojiSad;
            }
            if (emotion == "Surprise")
            {
                textSymbol.Text = emojiSuprise;
            }

            textSymbol2.Text = textSymbol.Text;

        }
    }
}
