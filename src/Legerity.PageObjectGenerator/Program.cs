namespace Legerity.PageObjectGenerator
{
    using System.IO;
    using System.Threading.Tasks;
    using CommandLine;
    using Features;
    using Features.Generators.Axml;
    using Features.Generators.Xaml;
    using Infrastructure.Configuration;
    using Infrastructure.Logging;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<GeneratorOptions>(args)
                .WithNotParsed(errors =>
                {
                    foreach (Error error in errors)
                    {
                        if (error.Tag == ErrorType.MissingRequiredOptionError)
                        {
                            ConsoleEventLogger.Current.WriteError("A required parameter was not provided");
                        }
                    }
                })
                .WithParsedAsync(async options =>
                {
                    ConsoleEventLogger.Current.WriteInfo($"Locating {options.PlatformType:G} page files in {options.Path}...");

                    IPageObjectGenerator pageObjectGenerator = default;

                    switch (options.PlatformType)
                    {
                        case PlatformType.Windows:
                            pageObjectGenerator = new XamlPageObjectGenerator();
                            break;
                        case PlatformType.Android:
                            pageObjectGenerator = new AxmlPageObjectGenerator();
                            break;
                        case PlatformType.Web:
                            break;
                        case PlatformType.IOS:
                            break;
                        case PlatformType.Uno:
                            break;
                        default:
                            ConsoleEventLogger.Current.WriteWarning("Cannot generate Legerity page objects for an unsupported project type!");
                            return;
                    }

                    if (pageObjectGenerator != default)
                    {
                        if (!Directory.Exists(options.Output))
                        {
                            Directory.CreateDirectory(options.Output);
                        }

                        await pageObjectGenerator.GenerateAsync(options.Path, options.Output);
                    }
                    else
                    {
                        ConsoleEventLogger.Current.WriteError("Cannot generate Legerity page objects as a suitable generator could not be found!");
                        return;
                    }

                    ConsoleEventLogger.Current.WriteInfo("Completed generating Legerity page objects!");
                });
        }
    }
}
