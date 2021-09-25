using System;

namespace ApiCalc.Middleware
{
    public enum MaxConcurrentRequestsLimitExceededPolicy
    {
        Drop
    }
    public class MaxConcurrentRequestsOptions
    {
        public const int ConcurrentRequestsUnlimited = -1;

        private int _limit;
       
        public int Limit
        {
            get => _limit;

            set => _limit = (value < ConcurrentRequestsUnlimited) ? ConcurrentRequestsUnlimited : value; 
        }
        public MaxConcurrentRequestsOptions()
        {
            _limit = ConcurrentRequestsUnlimited;
        }
    }
  
}