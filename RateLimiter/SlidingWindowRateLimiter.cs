namespace RateLimiter;

public class SlidingWindowRateLimiter
{

    private Dictionary<int, List<DateTime>> _requestTimestamps { get; set; }
    
    private int _maxRequests { get; set; }
    private TimeSpan _timeWindow { get; set; }
    

    public SlidingWindowRateLimiter(int maxRequests, TimeSpan timeWindow)
    {
        if (maxRequests <= 0)
        {
            throw new ArgumentException(nameof(maxRequests));
        }
        
        if (timeWindow <= TimeSpan.Zero)
        {
            throw new ArgumentException(nameof(timeWindow));
        }
        
        _requestTimestamps = new Dictionary<int, List<DateTime>>();
        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
    }

    public bool RateLimit(int customerId)
    {
        var currentRequestTimestamp = DateTime.UtcNow;

        if (!_requestTimestamps.ContainsKey(customerId))
        {
            _requestTimestamps.Add(customerId, new List<DateTime>());
        }
        
        _requestTimestamps[customerId].RemoveAll(x => x.Add(_timeWindow) <= currentRequestTimestamp);

        if (_requestTimestamps[customerId].Count >= _maxRequests)
        {
            return false;
        }

        _requestTimestamps[customerId].Add(currentRequestTimestamp);
        return true;

    }
    
}