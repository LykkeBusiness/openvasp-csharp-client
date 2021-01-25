using Newtonsoft.Json.Converters;

namespace OpenVASP.CSharpClient.Applications.TravelRule
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}