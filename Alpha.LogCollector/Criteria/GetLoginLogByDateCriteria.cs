namespace Alpha.LogCollector.Criteria
{
    using Fanex.Data.Repository;
    using System;

    public class GetLoginLogByDateCriteria : CriteriaBase
    {
        public GetLoginLogByDateCriteria(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; }

        public override string GetSettingKey() => "Age_UserLog_LoginSelByDate";

        public override bool IsValid() 
            => Date > DateTime.MinValue;
    }
}