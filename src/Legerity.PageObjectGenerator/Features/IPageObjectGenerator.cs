namespace Legerity.PageObjectGenerator.Features
{
    using System.Threading.Tasks;

    public interface IPageObjectGenerator
    {
        Task GenerateAsync(string searchFolder, string outputFolder);
    }
}
