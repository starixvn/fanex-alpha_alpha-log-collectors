namespace Alpha.LogCollector.Services
{
    using System;

    public interface ILogCollector
    {
        void Collect(DateTime currentTime);
    }
}