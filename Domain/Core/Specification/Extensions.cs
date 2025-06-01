using System.Linq.Expressions;
using System.Reflection;

namespace Domain.Core.Specification
{
    public static class Extensions
    {
        public static ISpecification<T> And<T>(
            this ISpecification<T> left,
            ISpecification<T> right)
        {
            return new And<T>(left, right);
        }

        public static ISpecification<T> Or<T>(
            this ISpecification<T> left,
            ISpecification<T> right)
        {
            return new Or<T>(left, right);
        }

        public static ISpecification<T> Negate<T>(this ISpecification<T> inner)
        {
            return new Negated<T>(inner);
        }

        public static void ApplySorting(this IRootSpecification gridSpec,
            string sort,
            string orderByDescendingMethodName,
            string groupByMethodName)
        {
            if (string.IsNullOrEmpty(sort)) return;

            const string DescendingSuffix = "Desc";

            var descending = sort.EndsWith(DescendingSuffix, StringComparison.Ordinal);
            var propertyName = sort.Substring(0, 1).ToUpperInvariant() +
                               sort.Substring(1, sort.Length - 1 - (descending ? DescendingSuffix.Length : 0));

            var specificationType = gridSpec.GetType().BaseType;
            var targetType = specificationType?.GenericTypeArguments[0];
            var property = targetType!.GetRuntimeProperty(propertyName) ??
                           throw new InvalidOperationException($"Because the property {propertyName} does not exist it cannot be sorted.");
            ArgumentNullException.ThrowIfNull(targetType);
            var lambdaParamX = Expression.Parameter(targetType, "x");

            var propertyReturningExpression = Expression.Lambda(
                Expression.Convert(
                    Expression.Property(lambdaParamX, property),
                    typeof(object)),
                lambdaParamX);

            // S3011: Make sure that this accessibility bypass is safe here.
            // Only allow public instance methods to be invoked via reflection.
            const BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (descending)
            {
                specificationType?.GetMethod(
                        orderByDescendingMethodName,
                        BindingFlags)
                    ?.Invoke(gridSpec, [propertyReturningExpression]);
            }
            else
            {
                specificationType?.GetMethod(
                        groupByMethodName,
                        BindingFlags)
                    ?.Invoke(gridSpec, [propertyReturningExpression]);
            }
        }
    }
}
