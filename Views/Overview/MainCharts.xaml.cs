using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Extensions;

namespace MAPSAI.Views.ProjectDataFiles;

public partial class MainCharts : ContentView
{
	public MainCharts()
	{
		InitializeComponent();
        BindingContext = this;
	}

    public ISeries[] Series { get; set; } = [
        new LineSeries<double>
        {
            Values = [2, 1, 3, 5, 3, 4, 6],
            Fill = null,
            GeometrySize = 20
        },
        new LineSeries<int, StarGeometry>
        {
            Values = [4, 2, 5, 2, 4, 5, 3],
            Fill = null,
            GeometrySize = 20,
        }
    ];

    public LabelVisual Title { get; set; } =
        new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15)
        };

    public IEnumerable<ISeries> Series2 { get; set; } =
    new[] { 2, 4, 1, 4, 3 }.AsPieSeries();
}