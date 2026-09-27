using System.Text.RegularExpressions;

namespace SilkSoarDash.Logging
{
    public sealed class SilkLogEvent
    {
        private static readonly Regex Hole = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);
        public string Template { get; }

        public object[] Args { get; }

        public SilkLogEvent(string template, object[] args)
        {
            Template = template;
            Args = args;
        }

        public override string ToString()
        {
            var index = 0;

            return Hole.Replace(Template, match => index >= Args.Length ? match.Value : Render(Args[index++]));
        }

        private static string Render(object value)
        {
            if (value == null)
            {
                return "null";
            }

            var text = value.ToString();

            return text.IndexOf(' ') < 0 ? text : "\"" + text + "\"";
        }
    }
}
