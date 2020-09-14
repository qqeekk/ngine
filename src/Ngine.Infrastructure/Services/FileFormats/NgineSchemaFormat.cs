using Ngine.Infrastructure.Abstractions.Services;

namespace Ngine.Infrastructure.Services.FileFormats
{
    public class NgineSchemaFormat : IFileFormat
    {
        public string FileExtension => "ngsc";

        public string FileFormatDescription => "Ngine-schema file";
    }
}
