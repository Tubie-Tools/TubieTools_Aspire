using ModelLayer;

namespace ServiceLayer
{
    public interface IEventService
    {
        Task<IEvent[]> Get(string userName);
        Task<PagedList<IEvent>> Get(ProductParameters param, string userName);
        Task<PagedList<IEvent>> GetWeightsOverTime(ProductParameters param, string userName);
        void Create(IEvent model, string userName);
        IList<IEvent> GroupedEvents(string userName);
    }
}