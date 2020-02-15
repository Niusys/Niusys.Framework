using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.MailServices
{
    public class EmailSetting
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
    }
}
