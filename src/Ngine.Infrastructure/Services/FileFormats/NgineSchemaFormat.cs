using Ngine.Infrastructure.Abstractions.Services;

namespace Ngine.Infrastructure.Services.FileFormats
{
    public class NgineSchemaFormat : IFileFormat
    {
        public string FileExtension => "ngs";

        public string FileFormatDescription => "Файл Ngine-schema";
    }
}
