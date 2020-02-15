using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.MailServices
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            FromAddresses = new List<MailboxAddress>();
            ToAddresses = new List<MailboxAddress>();
        }

        public List<MailboxAddress> FromAddresses { get; set; }
        public List<MailboxAddress> ToAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
