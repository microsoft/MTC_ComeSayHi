using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace MTCSTLKiosk.Controls
{
    public interface IQuarterControl
    {
        CaptureElement MainCapture { get;  }

        void UpdateEvent(CognitiveEvent mainEvent);

    }
}
