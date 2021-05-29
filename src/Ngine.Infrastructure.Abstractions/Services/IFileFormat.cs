namespace Ngine.Infrastructure.Abstractions.Services
{
    public interface IFileFormat
    {
        string FileExtension { get; }
        string FileFormatDescription { get; }
    }
}
