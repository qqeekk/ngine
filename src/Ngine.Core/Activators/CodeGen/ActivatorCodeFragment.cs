namespace Ngine.Core.Activators.CodeGen
{
    public class ActivatorCodeFragment
    {
        public string FunctionName { get; }

        public string FunctionCode { get; }

        public ActivatorCodeFragment(string functionName, string functionCode)
        {
            FunctionName = functionName;
            FunctionCode = functionCode;
        }
    }
}
