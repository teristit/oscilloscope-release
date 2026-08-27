using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace oscilloscope.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<SineWaveModel> Waves { get; } = new();

        [ObservableProperty] private double timeRange = 1.0;
        [ObservableProperty] private bool autoScaleY = true;
        [ObservableProperty] private bool phaseInDegrees = true;
        [ObservableProperty] private bool isRunning = true;
        [ObservableProperty] private bool showSum = false;

        private int waveCounter;
        private int lastColorIndex;

        public double CurrentTime { get; set; }

        public RelayCommand AddWaveCommand { get; }
        public RelayCommand<SineWaveModel> RemoveWaveCommand { get; }
        public RelayCommand ResetTimeCommand { get; }

        public MainViewModel()
        {
            AddWaveCommand = new RelayCommand(() => AddWave());
            RemoveWaveCommand = new RelayCommand<SineWaveModel>(RemoveWave);
            ResetTimeCommand = new RelayCommand(ResetTime);

            AddWave(2.0, 2.0, 0.0);
            AddWave(1.0, 5.0, 45.0);
            AddWave(0.5, 11.0, 90.0);
        }

        private void AddWave(double amplitude = 1.0, double frequency = 1.0, double phase = 0.0)
        {
            var color = WavePalette.Items[lastColorIndex % WavePalette.Items.Length];
            lastColorIndex++;
            waveCounter++;
            Waves.Add(new SineWaveModel
            {
                Name = $"Синусоида {waveCounter}",
                Amplitude = amplitude,
                Frequency = frequency,
                Phase = phase,
                ColorHex = color.Hex,
                IsVisible = true,
            });
        }

        private void RemoveWave(SineWaveModel? wave)
        {
            if (wave != null)
                Waves.Remove(wave);
        }

        private void ResetTime()
        {
            CurrentTime = 0;
        }

        public double SumAt(double t)
        {
            double sum = 0;
            foreach (var w in Waves)
            {
                if (w.IsVisible)
                    sum = sum + w.ValueAt(t, PhaseInDegrees);
            }
            return sum;
        }

        public void SaveToFile(string path)
        {
            var session = new SessionDto();
            session.TimeRange = TimeRange;
            session.PhaseInDegrees = PhaseInDegrees;
            foreach (var w in Waves)
            {
                var dto = new WaveDto();
                dto.Name = w.Name;
                dto.Amplitude = w.Amplitude;
                dto.Frequency = w.Frequency;
                dto.Phase = w.Phase;
                dto.ColorHex = w.ColorHex;
                dto.IsVisible = w.IsVisible;
                session.Waves.Add(dto);
            }
            File.WriteAllText(path, JsonSerializer.Serialize(session));
        }

        public void LoadFromFile(string path)
        {
            var session = JsonSerializer.Deserialize<SessionDto>(File.ReadAllText(path));
            if (session == null)
                return;

            Waves.Clear();
            waveCounter = 0;
            lastColorIndex = 0;
            TimeRange = session.TimeRange;
            PhaseInDegrees = session.PhaseInDegrees;

            foreach (var dto in session.Waves)
            {
                lastColorIndex++;
                waveCounter++;
                Waves.Add(new SineWaveModel
                {
                    Name = dto.Name,
                    Amplitude = dto.Amplitude,
                    Frequency = dto.Frequency,
                    Phase = dto.Phase,
                    ColorHex = dto.ColorHex,
                    IsVisible = dto.IsVisible,
                });
            }
        }
    }

    public class WaveDto
    {
        public string Name { get; set; } = "";
        public double Amplitude { get; set; }
        public double Frequency { get; set; }
        public double Phase { get; set; }
        public string ColorHex { get; set; } = "";
        public bool IsVisible { get; set; }
    }

    public class SessionDto
    {
        public double TimeRange { get; set; } = 1.0;
        public bool PhaseInDegrees { get; set; } = true;
        public List<WaveDto> Waves { get; set; } = new List<WaveDto>();
    }
}
