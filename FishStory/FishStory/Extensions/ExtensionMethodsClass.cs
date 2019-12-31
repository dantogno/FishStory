using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory
{
    public static class ExtensionMethodsClass
    {
        public static bool IsNullOrWhitespace(this string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue);
        }
    }
}
