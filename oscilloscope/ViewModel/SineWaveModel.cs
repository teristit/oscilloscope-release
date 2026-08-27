using CommunityToolkit.Mvvm.ComponentModel;

namespace oscilloscope.ViewModel
{
    public partial class SineWaveModel : ObservableObject
    {
        [ObservableProperty] private string name = "";
        [ObservableProperty] private double amplitude = 1.0;
        [ObservableProperty] private double frequency = 1.0;
        [ObservableProperty] private double phase = 0.0;
        [ObservableProperty] private string colorHex = "#1f77b4";
        [ObservableProperty] private bool isVisible = true;

        public double ValueAt(double t, bool phaseInDegrees)
        {
            double phi = Phase;
            if (phaseInDegrees)
                phi = phi * Math.PI / 180.0;
            return Amplitude * Math.Sin(2 * Math.PI * Frequency * t + phi);
        }
    }

    public class PaletteItem
    {
        public string Name;
        public string Hex;

        public PaletteItem(string name, string hex)
        {
            Name = name;
            Hex = hex;
        }
    }

    public static class WavePalette
    {
        public static readonly PaletteItem[] Items =
        {
            new PaletteItem("синий", "#1f77b4"),
            new PaletteItem("красный", "#d62728"),
            new PaletteItem("зелёный", "#2ca02c"),
            new PaletteItem("оранжевый", "#ff7f0e"),
            new PaletteItem("фиолетовый", "#9467bd"),
            new PaletteItem("бирюзовый", "#17becf"),
            new PaletteItem("розовый", "#e377c2"),
            new PaletteItem("коричневый", "#8c564b"),
        };
    }
}
