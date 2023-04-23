namespace Helper.Core.Specification;

using System.Linq.Expressions;

public class ExpressionSpecification<T> : ISpecification<T>
{
    private readonly Lazy<Func<T, bool>> expressionFunc;

    public ExpressionSpecification(Expression<Func<T, bool>> expression)
    {
        this.Expression = expression;
        this.expressionFunc = new Lazy<Func<T, bool>>(expression.Compile);
    }

    public Expression<Func<T, bool>> Expression { get; }

    public bool IsSatisfied(T obj)
    {
        return this.expressionFunc.Value(obj);
    }
}