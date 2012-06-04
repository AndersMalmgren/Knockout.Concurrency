using System.Linq.Expressions;
using System.Reflection;

namespace Knockout.Concurrency.Demo.Common.Extensions
{
    public static class ExpressionExtensions
    {
        public static MemberInfo GetMemberInfo(this Expression expression)
        {
            var lambdaExpression = (LambdaExpression)expression;
            return (!(lambdaExpression.Body is UnaryExpression) ? (MemberExpression)lambdaExpression.Body : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand).Member;
        }
    }
}
