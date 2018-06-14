namespace Alpha.LogCollector.WebService.Services
{
    using System;
    using System.Collections.Generic;

    public interface ILogCollector<T>
    {
        IEnumerable<T> Collect(DateTime date, long logId);
    }
}