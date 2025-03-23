using IdGen;

namespace Domain.Core
{
    public static class NumericIdGenerator
    {
        private static IdGenerator? s_generator;

        public static long Generate()
        {
            EnsureInitialized();
            return s_generator!.CreateId();
        }

        public static long[] Generate(int count)
        {
            EnsureInitialized();
            return s_generator!.Take(count).ToArray();
        }

        public static void Init(int? generatorId = null, DateTimeOffset? epoch = null)
        {
            if (s_generator != null) return;

            generatorId ??= new Random().Next(0, 1024);
            if (generatorId < 0 || generatorId > 1023)
            {
                throw new ArgumentOutOfRangeException(nameof(generatorId), "Generator ID must be between 0 and 1023.");
            }

            epoch ??= new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

            s_generator = new IdGenerator(generatorId.Value, new IdGeneratorOptions(timeSource: new DefaultTimeSource(epoch.Value)));
        }

        private static void EnsureInitialized()
        {
            if (s_generator == null)
            {
                throw new InvalidOperationException("Generator has not been initialized. Call Init() first.");
            }
        }
    }
}
