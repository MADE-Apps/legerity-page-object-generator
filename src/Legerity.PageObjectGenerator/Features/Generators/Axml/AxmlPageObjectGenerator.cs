namespace Legerity.PageObjectGenerator.Features.Generators.Axml
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

    public class AxmlPageObjectGenerator : IPageObjectGenerator
    {
        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

        private const string BaseElementType = "AndroidElement";

        private static readonly GenericEqualityComparer<string> SimpleStringComparer = new(s => s.ToLower());

        public static IEnumerable<string> SupportedCoreAndroidElements => new List<string>
        {
            "Button",
            "CheckBox",
            "DatePicker",
            "EditText",
            "RadioButton",
            "Spinner",
            "Switch",
            "TextView",
            "ToggleButton"
        };

        public async Task GenerateAsync(string searchFolder, string outputFolder)
        {
            IEnumerable<string> filePaths = GetAxmlFilePaths(searchFolder);

            foreach (string filePath in filePaths)
            {
                ConsoleEventLogger.Current.WriteInfo($"Processing {filePath}");

                await using FileStream fileStream = File.Open(filePath, FileMode.Open);
                var axml = XDocument.Load(fileStream);

                if (axml.Root != null)
                {
                    var templateData = new GeneratorTemplateData(Path.GetFileNameWithoutExtension(filePath), "Android", BaseElementType);

                    ConsoleEventLogger.Current.WriteInfo($"Generating template for {templateData}");

                    IEnumerable<XElement> elements = this.FlattenElements(axml.Root.Elements()).Where(element => element != null);

                    foreach (XElement element in elements)
                    {
                        string id = RemoveAndroidIdReference(element.Attribute(XName.Get("id", AndroidNamespace))?.Value);
                        string contentDesc = element.Attribute(XName.Get("contentDescription", AndroidNamespace))?.Value;

                        string byQueryType = GetByQueryType(id, contentDesc);

                        if (byQueryType.IsNullOrWhiteSpace())
                        {
                            continue;
                        }

                        string byQueryValue = id ?? contentDesc;

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

        private static string RemoveAndroidIdReference(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Replace("+", string.Empty).Replace("@id/", string.Empty);
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

        private static string GetByQueryType(string id, string contentDesc)
        {
            if (!id.IsNullOrWhiteSpace())
            {
                return "Id";
            }

            return !contentDesc.IsNullOrWhiteSpace() ? "AndroidContentDesc" : null;
        }

        private static IEnumerable<string> GetAxmlFilePaths(string searchFolder)
        {
            string[] filePaths = default;

            try
            {
                filePaths = Directory.GetFiles(searchFolder, "*.axml", SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException uae)
            {
                ConsoleEventLogger.Current.WriteError(
                    "An error occurred while retrieving AXML files for processing",
                    uae);
            }

            if (filePaths is { Length: <= 0 })
            {
                ConsoleEventLogger.Current.WriteWarning("No AXML files were found for processing");
            }

            return filePaths;
        }

        private string GetElementWrapperType(string elementName)
        {
            return SupportedCoreAndroidElements.Contains(elementName, SimpleStringComparer) ? elementName : BaseElementType;
        }

        private IEnumerable<XElement> FlattenElements(IEnumerable<XElement> elements)
        {
            return elements.SelectMany(c => this.FlattenElements(c.Elements())).Concat(elements);
        }
    }
}