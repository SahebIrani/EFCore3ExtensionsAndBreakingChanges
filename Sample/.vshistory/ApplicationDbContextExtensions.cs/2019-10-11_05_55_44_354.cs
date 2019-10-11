namespace Sample
{
    public static class ApplicationDbContextExtensions
    {
        public static DbSet<TEntityType> DbSet<TEntityType>(this context)
            where TEntityType : class
        {
            return context.Set<TEntityType>();
        }
    }
}
