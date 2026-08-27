using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using oscilloscope.ViewModel;
using ScottPlot;

namespace oscilloscope
{
    public partial class MainWindow : Window
    {
        private MainViewModel vm = new MainViewModel();
        private DispatcherTimer? timer;
        private const int PointsCount = 500;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;

            foreach (var wave in vm.Waves)
                wave.PropertyChanged += Wave_PropertyChanged;

            vm.Waves.CollectionChanged += Waves_CollectionChanged;

            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.TimeRange) ||
                    e.PropertyName == nameof(vm.AutoScaleY) ||
                    e.PropertyName == nameof(vm.PhaseInDegrees) ||
                    e.PropertyName == nameof(vm.ShowSum) ||
                    e.PropertyName == nameof(vm.IsRunning))
                    UpdatePlot();
            };

            vm.CurrentTime = vm.TimeRange;
            UpdatePlot();
            StartAnimation();
        }

        private void Wave_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdatePlot();
        }

        private void Waves_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SineWaveModel w in e.NewItems)
                    w.PropertyChanged += Wave_PropertyChanged;
            if (e.OldItems != null)
                foreach (SineWaveModel w in e.OldItems)
                    w.PropertyChanged -= Wave_PropertyChanged;

            UpdatePlot();
        }

        private void StartAnimation()
        {
            if (timer != null)
                timer.Stop();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!vm.IsRunning)
                return;

            vm.CurrentTime += 0.05;
            UpdatePlot();
        }

        private void UpdatePlot()
        {
            var plot = PlotControl.Plot;
            plot.Clear();

            double timeEnd = Math.Max(vm.CurrentTime, vm.TimeRange);
            double timeStart = timeEnd - vm.TimeRange;

            foreach (var wave in vm.Waves)
            {
                if (!wave.IsVisible)
                    continue;

                double[] xs = new double[PointsCount];
                double[] ys = new double[PointsCount];
                for (int i = 0; i < PointsCount; i++)
                {
                    double t = timeStart + (timeEnd - timeStart) * i / (PointsCount - 1);
                    xs[i] = t;
                    ys[i] = wave.ValueAt(t, vm.PhaseInDegrees);
                }

                var signal = plot.Add.Scatter(xs, ys);
                signal.LegendText = wave.Name;
                signal.Color = ScottPlot.Color.FromHex(wave.ColorHex);
                signal.LineWidth = 2;
            }

            if (vm.ShowSum)
            {
                double[] xs = new double[PointsCount];
                double[] ys = new double[PointsCount];
                for (int i = 0; i < PointsCount; i++)
                {
                    double t = timeStart + (timeEnd - timeStart) * i / (PointsCount - 1);
                    xs[i] = t;
                    ys[i] = vm.SumAt(t);
                }

                var sumSignal = plot.Add.Scatter(xs, ys);
                sumSignal.LegendText = "Сумма";
                sumSignal.Color = ScottPlot.Color.FromHex("#000000");
                sumSignal.LineWidth = 2;
            }

            plot.Legend.IsVisible = true;
            plot.Legend.Alignment = Alignment.UpperRight;
            plot.Axes.SetLimitsX(timeStart, timeEnd);

            if (vm.AutoScaleY)
            {
                plot.Axes.AutoScaleY();
            }
            else
            {
                double maxAmp = 5;

                foreach (var wave in vm.Waves)
                {
                    if (!wave.IsVisible)
                        continue;

                    if (Math.Abs(wave.Amplitude) > maxAmp)
                        maxAmp = Math.Abs(wave.Amplitude);
                }

                if (vm.ShowSum)
                {
                    double sum = 0;
                    foreach (var wave in vm.Waves)
                        if (wave.IsVisible)
                            sum = sum + Math.Abs(wave.Amplitude);

                    if (sum > maxAmp)
                        maxAmp = sum;
                }

                maxAmp = maxAmp + maxAmp * 0.1;
                plot.Axes.SetLimitsY(-maxAmp, maxAmp);
            }

            PlotControl.Refresh();
        }


        private void OnPauseClicked(object sender, RoutedEventArgs e)
        {
            vm.IsRunning = !vm.IsRunning;
            if (vm.IsRunning)
                StartAnimation();
            else
                timer?.Stop();
        }

        private void OnRefreshClicked(object sender, RoutedEventArgs e)
        {
            UpdatePlot();
        }


        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
                vm.SaveToFile(dlg.FileName);
        }

        private void OnLoadClicked(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                vm.LoadFromFile(dlg.FileName);
                vm.CurrentTime = vm.TimeRange;
                UpdatePlot();
            }
        }
    }
}
