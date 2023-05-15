using System.Globalization;
using WavePlayer.GUI.Properties;

namespace WavePlayer.GUI
{
    internal class ResourceService
         : ViewModel
    {
        static ResourceService() => Current = new ResourceService();

        public ResourceService() => Resources = new Resources();

        public static ResourceService Current { get; }
        public Resources Resources { get; }

        public void ChangeCulture(string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                cultureName = CultureInfo.CurrentCulture.Name;
            Resources.Culture = CultureInfo.GetCultureInfo(cultureName);
            RaisePropertyChangedEvent(nameof(Resources));
        }
    }
}
