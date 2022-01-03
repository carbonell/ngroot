using System.Linq.Expressions;

namespace NGroot
{
    public static class ExpressionExtensions
    {
        public static string? GetMemberName(this Expression expression)
        {
            // Expression is Reference Type
            if (expression is MemberExpression)
            {
                var memberExpr = (MemberExpression)expression;
                return GetMemberName(memberExpr);
                // Expression is Primitive Type
            }
            else if (expression is UnaryExpression)
            {
                var unaryExpr = (UnaryExpression)expression;
                return GetMemberName(unaryExpr);
            }
            else if (expression is MethodCallExpression)
            {
                var methodCallExpr = (MethodCallExpression)expression;
                return GetMemberName(methodCallExpr);
            }
            else
            {
                return string.Empty;
            }
        }

        private static string? GetMemberName(MemberExpression expression)
            => expression?.Member?.Name;

        private static string? GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression?.Operand is MethodCallExpression)
            {
                var methodExpression = unaryExpression?.Operand as MethodCallExpression;
                return methodExpression?.Method?.Name;
            }
            var memberExpression = unaryExpression?.Operand as MemberExpression;
            return memberExpression?.Member?.Name;
        }

        private static string? GetMemberName(MethodCallExpression expr)
        {
            var methodCallExpression = (MethodCallExpression)expr;
            return methodCallExpression?.Method?.Name;
        }
    }
}