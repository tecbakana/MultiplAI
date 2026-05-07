using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace CMSXBLL
{
    public class Email
    {
        public string? Sender { get; set; }
        public string? To { get; set; }
        public string? Assunto { get; set; }
        public Attachment? Anexo { get; set; }
        public string? Corpo { get; set; }
        public string? smtpsrv { get; set; }
        public string? smtpusr { get; set; }
        public string? smtppass { get; set; }

        public static Email ObtemNovoEmail()
        {
            return new Email();
        }
    }
}
