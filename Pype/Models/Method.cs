namespace Pype.Models
{
    /// <summary>
    /// Use an enum and convert to a string... or just use a static class with const strings
    /// and skip the conversion step
    /// </summary>
    public static class Method
    {
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Patch = "PATCH";
        public const string Delete = "DELETE";
    }
}
