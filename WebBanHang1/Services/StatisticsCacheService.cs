namespace WebBanHang1.Services
{
    public class StatisticsCacheService
    {
        private DateTime _lastUpdated;
        private object _cachedData;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public object GetStatistics()
        {
            if (DateTime.Now - _lastUpdated > _cacheDuration)
            {
                return null;
            }
            return _cachedData;
        }

        public void UpdateStatistics(object data)
        {
            _cachedData = data;
            _lastUpdated = DateTime.Now;
        }
    }
}
