using Ngine.Backend;

namespace Ngine.CommandLine.Options
{
    internal class AppSettings
    {
        public string PathToYamlStorage { get; set; }

        public KerasExecutionOptions ExecutionOptions { get; set; }
    }
}
