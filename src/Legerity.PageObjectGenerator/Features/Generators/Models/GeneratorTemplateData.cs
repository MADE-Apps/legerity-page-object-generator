namespace Legerity.PageObjectGenerator.Features.Generators.Models
{
    using System.Collections.Generic;

    public class GeneratorTemplateData
    {
        public GeneratorTemplateData(string page, string platform, string baseElementType)
        {
            this.Page = page;
            this.Platform = platform;
            this.Type = baseElementType;
        }

        public string Page { get; set; }

        public string Platform { get; set; }

        public string Type { get; set; }

        public UiElement Trait { get; set; }

        public List<UiElement> Elements { get; set; } = new();

        public override string ToString()
        {
            return $"[Page] {this.Page}; [Platform] {this.Platform};";
        }
    }
}