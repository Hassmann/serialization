using System.Text;

namespace SLD.Serialization
{
    public static class HashExtensions
    {
        public static Hash GetHash(this string text)
            => Hash.From(Encoding.Unicode.GetBytes(text));
    }
}