using Ngine.Infrastructure.Abstractions.Services;

namespace Ngine.Infrastructure.Services.FileFormats
{
    public class NgineMappingsFormat : IFileFormat
    {
        public string FileExtension => "ngm";

        public string FileFormatDescription => "Файл отображений Ngine";
    }
}
