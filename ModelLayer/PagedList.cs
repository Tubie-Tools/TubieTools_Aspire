namespace ModelLayer
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; set; }
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            MetaData = new MetaData
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };
            AddRange(items);
        }
        public static PagedList<T> ToPagedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        internal static PagedList<IEvent> ToPagedList(IEnumerable<IGrouping<object, IEvent>> enumerable, int pageNumber, int pageSize)
        {
            var count = enumerable.Count();
            var items = enumerable.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToList();

            List<IEvent> list = new List<IEvent>();
            foreach (var item in items)
            {
                list.Add(item.First());
            }

            list.Reverse();

            return new PagedList<IEvent>(list, count, pageNumber, pageSize);

            //throw new NotImplementedException();
        }
    }
}
