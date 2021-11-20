
namespace ApiCalc.Middleware
{
    
    public class MaxConcurrentRequestsOptions
    {
        public const Int32 ConcurrentRequestsUnlimited = -1;

        private Int32 _limit;
       
        public Int32 Limit
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