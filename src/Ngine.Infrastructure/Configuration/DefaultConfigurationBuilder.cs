using Microsoft.Extensions.Configuration;
using System.IO;

namespace Ngine.Infrastructure.Configuration
{
    public class DefaultConfigurationBuilder
    {
        public static IConfigurationBuilder Create(string jsonFilePath)
        {
            return new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(jsonFilePath, false, true);
        }
    }
}
