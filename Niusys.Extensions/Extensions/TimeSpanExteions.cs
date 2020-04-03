using System;
using System.Text;

namespace Niusys
{
    public static class TimeSpanExteions
    {
        public static string FriendlyFormat(this TimeSpan timeSpan)
        {
            StringBuilder sbFormatedMessage = new StringBuilder();
            if (timeSpan.Days > 1)
            {
                sbFormatedMessage.Append($"{timeSpan.Days}天");
            }

            if (timeSpan.Days == 1)
            {
                sbFormatedMessage.Append($"{24 + timeSpan.Hours}小时");
            }
            else if (timeSpan.Days == 0 && timeSpan.Hours > 1)
            {
                sbFormatedMessage.Append($"{timeSpan.Hours}小时");
            }

            if (timeSpan.Days == 0 && timeSpan.Hours == 1)
            {
                sbFormatedMessage.Append($"{60 + timeSpan.Minutes}分钟");
            }
            else if (timeSpan.Hours == 0 && timeSpan.Minutes > 0)
            {
                sbFormatedMessage.Append($"{timeSpan.Minutes}分钟");
            }

            return sbFormatedMessage.ToString();
        }
    }
}
