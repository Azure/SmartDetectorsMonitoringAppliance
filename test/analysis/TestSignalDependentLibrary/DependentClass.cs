namespace TestSignalDependentLibrary
{
    using Newtonsoft.Json;

    public class DependentClass
    {
        public string GetString()
        {
            return "with dependency";
        }

        public string ObjectToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
