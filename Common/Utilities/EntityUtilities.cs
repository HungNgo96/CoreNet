namespace Common.Utilities
{
    public static class EntityUtilities
    {
        public static List<string> GetAllEntityNames<T>() where T : class
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface.");

            return GetAllEntities<T>() // List<Type>
                .Select(x => x.Name) // IEnumerable<string>
                .ToList(); // List<string>
        }

        public static List<Type> GetAllEntities<T>() where T : class
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface.");

            return AppDomain.CurrentDomain.GetAssemblies() // Assembly[]
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(T).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false }) // IEnumerable<Type>
                .ToList(); // List<Type>
        }
    }
}
