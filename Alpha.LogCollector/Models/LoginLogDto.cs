namespace Alpha.LogCollector.Models
{
    using System;

    public class LoginLogDto
    {
        public long LogId { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }

        public int SubAccId { get; set; }

        public int ServerId { get; set; }

        public string IP { get; set; }

        public int ActionId { get; set; }

        public string DomainName { get; set; }

        public string BrowserName { get; set; }

        public string Platform { get; set; }

        public string CountryCode { get; set; }

        public string Username2 { get; set; }

        public DateTime LogDate { get; set; }

        public string Doing { get; set; }
    }
}