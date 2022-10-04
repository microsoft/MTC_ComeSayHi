using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.ApplicationModel.Email.DataProvider;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
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
    public sealed partial class Conversation : UserControl, IQuarterControl
    {
        Color[] myColors = { Colors.Aqua, Colors.SeaGreen, Colors.DarkMagenta, Colors.Khaki, Colors.Tomato };

        List<ConversationMessage> conversationMessages = new List<ConversationMessage>();
        Dictionary<string, string> mappingName = new Dictionary<string, string>();
        public Conversation()
        {
            this.InitializeComponent();
        }

        public CaptureElement MainCapture { get { return captureControl; } }

        public void UpdateEvent(CognitiveEvent mainEvent)
        {
            if(mainEvent == null || string.IsNullOrEmpty(mainEvent.PrimaryConversationMessageFinal.Message.Trim()))
                return;

            conversationMessages.Add(mainEvent.PrimaryConversationMessageFinal);
            var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (mainEvent.PrimaryConversationMessageFinal.User != "Unidentified" && mainEvent.PrimaryConversationMessageFinal.Message.ToLower().Contains("my name is"))
                {
                    //textTranscript.Text = "";
                    textStack.Children.Clear();
                    var name = mainEvent.PrimaryConversationMessageFinal.Message.Substring(mainEvent.PrimaryConversationMessageFinal.Message.ToLower().IndexOf("my name is") + 10);
                    name = name.Trim().Split(' ')[0];

                    name = name.Replace(".", "");
                    if (string.IsNullOrEmpty(name))
                    {
                        return;
                    }
                    if (mappingName.ContainsKey(mainEvent.PrimaryConversationMessageFinal.User))
                        mappingName.Remove(mainEvent.PrimaryConversationMessageFinal.User);

                    mappingName.Add(mainEvent.PrimaryConversationMessageFinal.User, name);
                    foreach (var message in conversationMessages)
                    {
                        WriteNewMessage(message);
                    }
                }
                else
                {
                    WriteNewMessage(mainEvent.PrimaryConversationMessageFinal);
                }
            });
        }

        private void WriteNewMessage(ConversationMessage message)
        {
            TextBlock tb = new TextBlock();
            tb.Text = MakeMessage(message);
            tb.FontSize = 22;
            tb.FontWeight = FontWeights.Bold;
            tb.Foreground = GetColor(message.User);
            tb.Padding = new Thickness(0, 0, 0, 10); 
            textStack.Children.Insert(0, tb);
        }

        private Brush GetColor(string name)
        {

            if (mappingName.ContainsKey(name))
            {
                var firstnum = (int)mappingName[name].First();
                firstnum = firstnum % myColors.Length;
                return new SolidColorBrush(myColors[firstnum]);
            }
            else
                return new SolidColorBrush(Colors.White);
        }

        private string MakeMessage (ConversationMessage message)
        {
            if (mappingName.ContainsKey(message.User))
                return $"{mappingName[message.User]} - {message.Message}";
            else
                return $"{message.User} - {message.Message}";
        }
    }
}
