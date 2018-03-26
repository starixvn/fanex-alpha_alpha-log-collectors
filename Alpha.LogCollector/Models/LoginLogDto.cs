namespace Alpha.LogCollector.Models
{
    using System;

    public class LoginLogDto
    {
        public int LogId { get; set; }

        public int UserId { get; }

        public int RoleId { get; }

        public int SubAccId { get; }

        public int ServerId { get; }

        public string IP { get; }

        public int ActionId { get; }

        public string DomainName { get; }

        public string BrowserName { get; }

        public string Platform { get; }

        public string CountryCode { get; }

        public string Username2 { get; }

        public DateTime LogDate { get; set; }

        public string Doing { get; }
    }
}