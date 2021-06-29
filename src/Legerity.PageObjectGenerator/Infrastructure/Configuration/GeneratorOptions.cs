namespace Legerity.PageObjectGenerator.Infrastructure.Configuration
{
    using System;
    using CommandLine;

    public class GeneratorOptions
    {
        [Option('o', HelpText = "The path to the folder where the generated class files should be stored. Default to the 'PageObjects' folder in the executing folder.")]
        public string Output { get; set; } = System.IO.Path.Combine(Environment.CurrentDirectory, "PageObjects");

        [Option('p', HelpText = "The path to the folder where platform pages exist that will be generating page objects for. Default to the executing folder.")]
        public string Path { get; set; } = Environment.CurrentDirectory;

        [Option('t', Required = true, HelpText = "The type of platform that will be generating page objects for.")]
        public PlatformType PlatformType { get; set; }
    }
}