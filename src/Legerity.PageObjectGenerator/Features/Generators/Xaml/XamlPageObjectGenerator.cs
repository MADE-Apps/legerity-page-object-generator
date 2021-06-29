namespace Legerity.PageObjectGenerator.Features.Generators.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Infrastructure.Extensions;
    using Infrastructure.Logging;
    using MADE.Collections.Compare;
    using MADE.Data.Validation.Extensions;
    using Models;
    using Scriban;

    public class XamlPageObjectGenerator : IPageObjectGenerator
    {
        private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        private const string BaseElementType = "WindowsElement";

        private static readonly GenericEqualityComparer<string> SimpleStringComparer = new(s => s.ToLower());

        public static IEnumerable<string> SupportedCoreWindowsElements => new List<string>
        {
            "AppBarButton",
            "AppBarToggleButton",
            "AutoSuggestBox",
            "Button",
            "CalendarDatePicker",
            "CalendarView",
            "CheckBox",
            "ComboBox",
            "CommandBar",
            "DatePicker",
            "FlipView",
            "GridView",
            "Hub",
            "HyperlinkButton",
            "InkToolbar",
            "ListBox",
            "ListView",
            "MenuFlyoutItem",
            "MenuFlyoutSubItem",
            "PasswordBox",
            "Pivot",
            "ProgressBar",
            "ProgressRing",
            "RadioButton",
            "Slider",
            "TextBlock",
            "TextBox",
            "TimePicker",
            "ToggleButton",
            "ToggleSwitch"
        };

        public async Task GenerateAsync(string searchFolder, string outputFolder)
        {
            IEnumerable<string> filePaths = GetXamlFilePaths(searchFolder);

            foreach (string filePath in filePaths)
            {
                ConsoleEventLogger.Current.WriteInfo($"Processing {filePath}");

                await using FileStream fileStream = File.Open(filePath, FileMode.Open);
                var xaml = XDocument.Load(fileStream);

                if (xaml.Root != null && xaml.Root.Name.ToString().Contains("Page"))
                {
                    var templateData = new GeneratorTemplateData(Path.GetFileNameWithoutExtension(filePath), "Windows", BaseElementType);

                    ConsoleEventLogger.Current.WriteInfo($"Generating template for {templateData}");

                    IEnumerable<XElement> elements = this.FlattenElements(xaml.Root.Elements());
                    foreach (XElement element in elements)
                    {
                        string automationId = element.Attribute("AutomationProperties.AutomationId")?.Value;
                        string uid = element.Attribute(XName.Get("Uid", XamlNamespace))?.Value;
                        string name = element.Attribute(XName.Get("Name", XamlNamespace))?.Value;

                        string byQueryType = GetByQueryType(uid, automationId, name);

                        if (byQueryType.IsNullOrWhiteSpace())
                        {
                            continue;
                        }

                        string wrapperAutomationId = uid ?? automationId;
                        string byQueryValue = wrapperAutomationId ?? name;

                        var uiElement = new UiElement(this.GetElementWrapperType(element.Name.LocalName),
                            byQueryValue.Capitalize(),
                            byQueryType,
                            byQueryValue);

                        ConsoleEventLogger.Current.WriteInfo($"Element found: {uiElement}");

                        templateData.Trait ??= uiElement;
                        templateData.Elements.Add(uiElement);
                    }

                    await GeneratePageObjectClassFileAsync(templateData, outputFolder);
                }
                else
                {
                    ConsoleEventLogger.Current.WriteWarning($"Skipping {filePath} as a page was not detected");
                }
            }
        }

        private static async Task GeneratePageObjectClassFileAsync(
            GeneratorTemplateData templateData,
            string outputFolder)
        {
            var pageObjectTemplate = Template.Parse(await File.ReadAllTextAsync("Templates/PageObject.template"));

            string result = await pageObjectTemplate.RenderAsync(templateData);

            FileStream output = File.Create(Path.Combine(outputFolder, $"{templateData.Page}.cs"));
            var outputWriter = new StreamWriter(output, Encoding.UTF8);

            await using (outputWriter)
            {
                await outputWriter.WriteAsync(result);
            }
        }

        private static string GetByQueryType(string uid, string automationId, string name)
        {
            if (!uid.IsNullOrWhiteSpace() || !automationId.IsNullOrWhiteSpace())
            {
                return "AutomationId";
            }

            return !name.IsNullOrWhiteSpace() ? "Name" : null;
        }

        private static IEnumerable<string> GetXamlFilePaths(string searchFolder)
        {
            string[] filePaths = default;

            try
            {
                filePaths = Directory.GetFiles(searchFolder, "*.xaml", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException uae)
            {
                ConsoleEventLogger.Current.WriteError(
                    "An error occurred while retrieving XAML files for processing",
                    uae);
            }

            if (filePaths is { Length: <= 0 })
            {
                ConsoleEventLogger.Current.WriteWarning("No XAML files were found for processing");
            }

            return filePaths;
        }

        private string GetElementWrapperType(string elementName)
        {
            return SupportedCoreWindowsElements.Contains(elementName, SimpleStringComparer) ? elementName : BaseElementType;
        }

        private IEnumerable<XElement> FlattenElements(IEnumerable<XElement> elements)
        {
            return elements.SelectMany(c => this.FlattenElements(c.Elements())).Concat(elements);
        }
    }
}