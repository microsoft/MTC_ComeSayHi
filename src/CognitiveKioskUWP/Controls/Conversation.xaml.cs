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
        struct PersonStruct
        {
             public string name;
            public Color color;
        }
        DateTime lastClear = DateTime.Now;
        string lastConvo = "";

        List<ConversationMessage> conversationMessages = new List<ConversationMessage>();
        Dictionary<string, PersonStruct> mappingName = new Dictionary<string, PersonStruct>();
        public Conversation()
        {
            this.InitializeComponent();
        }

        public CaptureElement MainCapture { get { return captureControl; } }

        public void UpdateEvent(CognitiveEvent mainEvent)
        {
            if(mainEvent == null || string.IsNullOrEmpty(mainEvent.PrimaryConversationMessageFinal.Message.Trim()))
                return;

            var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if(lastConvo != mainEvent.PrimaryConversationMessageFinal.ConvoId)
                {
                    lastConvo = mainEvent.PrimaryConversationMessageFinal.ConvoId;
                    textStack.Children.Clear();
                    conversationMessages.Clear();

                }
                conversationMessages.Add(mainEvent.PrimaryConversationMessageFinal);
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

                    mappingName.Add(mainEvent.PrimaryConversationMessageFinal.User, new PersonStruct() { name = name, color=GetRandomColor()});
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
        private Color GetRandomColor()
        {
            Random rand = new Random();

            return Color.FromArgb(255, (byte)rand.Next(190, 255), (byte)rand.Next(190, 255), (byte)rand.Next(190, 255));
        }

        private void WriteNewMessage(ConversationMessage message)
        {
            TextBlock tb = new TextBlock();
            tb.Text = MakeMessage(message);
            tb.FontSize = 22;
            tb.FontWeight = FontWeights.Bold;
            tb.Foreground = GetColor(message.User);
            tb.Padding = new Thickness(0, 0, 0, 10); 
            tb.TextWrapping = TextWrapping.Wrap;
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            textStack.Children.Insert(0, tb);
        }

        private Brush GetColor(string name)
        {

            if (mappingName.ContainsKey(name) && mappingName[name].name.Length > 0)
            {

                return new SolidColorBrush(mappingName[name].color);
            }
            else
                return new SolidColorBrush(Colors.White);
        }
                

        private string MakeMessage (ConversationMessage message)
        {
            if (mappingName.ContainsKey(message.User))
                return $"{mappingName[message.User].name} - {message.Message}";
            else
                return $"{message.User} - {message.Message}";
        }
    }
}
