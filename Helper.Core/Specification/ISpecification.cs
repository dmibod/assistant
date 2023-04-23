namespace Helper.Core.Specification;

public interface ISpecification<in T>
{
    bool IsSatisfied(T obj);
}