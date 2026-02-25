using System.Diagnostics;
namespace MAPSAI;

public partial class CustomWindow : Window
{
    private readonly PerformanceCounter _cpuCounter;
    private PerformanceCounter[] _gpuCounters;

    public CustomWindow()
	{
		InitializeComponent();
        BindingContext = this;
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        InitializeGpuCounters();
        UpdateCpuUsageLoop();
        UpdateGpuUsageLoop();
    }

    private async void UpdateCpuUsageLoop()
    {
        while (true)
        {
            _cpuCounter.NextValue();
            await Task.Delay(1000);

            var cpu = _cpuCounter.NextValue();

            Dispatcher.Dispatch(() =>
            {
                CPULabel.Text = $"CPU({cpu:F1}%)";
            });

            await Task.Delay(1000);
        }
    }

    private void InitializeGpuCounters()
    {
        var category = new PerformanceCounterCategory("GPU Engine");
        var instances = category.GetInstanceNames().Where(i => i.Contains("engtype_3D")).ToArray();

        _gpuCounters = instances.Select(inst =>
            new PerformanceCounter("GPU Engine", "Utilization Percentage", inst)
        ).ToArray();

        foreach (var counter in _gpuCounters)
            counter.NextValue();
    }

    private async void UpdateGpuUsageLoop()
    {
        while (true)
        {
            float gpuUsage = 0;

            foreach (var counter in _gpuCounters)
            {
                try
                {
                    gpuUsage += counter.NextValue();
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }

            Dispatcher.Dispatch(() =>
            {
                GPULabel.Text = $"GPU({gpuUsage:F1}%)";
            });

            await Task.Delay(1000);
        }
    }
}