using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MAPSAI.Services;
using UraniumUI;
using SkiaSharp.Views.Maui.Controls.Hosting;
using LiveChartsCore.SkiaSharpView.Maui;
using MAPSAI.Models;
using MAPSAI.Services.Builders;
using MAPSAI.Services.AI;
using MAPSAI.Services.Files;

namespace MAPSAI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit(options =>
                {
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .UseSkiaSharp()
                .UseLiveCharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFontAwesomeIconFonts();
                });

            //DataStores
            builder.Services.AddSingleton<DataStore>();

            //Document processing
            builder.Services.AddTransient<ExcelService>();
            builder.Services.AddTransient<XmlService>();
            builder.Services.AddTransient<FileTextReader>();
            builder.Services.AddTransient<PdfService>();
            builder.Services.AddTransient<MSWordService>();

            //Builders
            builder.Services.AddTransient<UseCaseBuilder>();
            builder.Services.AddTransient<ProcessModelBuilder>();
            builder.Services.AddTransient<GanttBuilder>();
            builder.Services.AddTransient<PlantUMLBuilder>();
            builder.Services.AddTransient<PlantUMLProcessModelBuilder>();
            builder.Services.AddTransient<PlantUMLGanttBuilder>();

            //AI / ML
            builder.Services.AddSingleton<StoryPointService>();
            builder.Services.AddSingleton<ListEntryService>();

            //Random
            builder.Services.AddSingleton<CustomWindow>();
            

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
