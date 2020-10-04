﻿using Ngine.Infrastructure.Abstractions.Services;

namespace Ngine.Infrastructure.Services.FileFormats
{
    public class NgineSchemaAmbiguitiesFormat : IFileFormat
    {
        public string FileExtension => "ngsca";

        public string FileFormatDescription => "Файл неопределенностей Ngine-schema";
    }
}
