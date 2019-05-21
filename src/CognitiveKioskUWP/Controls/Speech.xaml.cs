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
    public sealed partial class Speech : UserControl, IQuarterControl
    {
        public Speech()
        {
            this.InitializeComponent();
        }

        public CaptureElement MainCapture { get { return captureControl; } }

        public void UpdateEvent(CognitiveEvent mainEvent)
        {
            if(mainEvent.ClearData)
            {
                textTranslationOriginalRT.Text = "";
                textTranslationRT.Text = "";
                textTranslationOriginal.Text = "";
                textTranslation.Text = "";

            }
            if (!string.IsNullOrEmpty(mainEvent.PrimarySpeechMessage))
            {
                textTranslationOriginalRT.Text = mainEvent.PrimarySpeechMessage;
                textTranslationRT.Text = mainEvent.SecondarySpeechMessage;
            }

            if (!string.IsNullOrEmpty(mainEvent.PrimarySpeechMessageFinal))
            {
                textTranslationOriginal.Text = mainEvent.PrimarySpeechMessageFinal + "\n\n" + textTranslationOriginal.Text;
                textTranslation.Text = mainEvent.SecondarySpeechMessageFinal + "\n\n" + textTranslation.Text;
                textTranslationOriginalRT.Text = "";
                textTranslationRT.Text = "";
            }
        }
    }
}
