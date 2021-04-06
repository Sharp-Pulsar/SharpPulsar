
namespace SharpPulsar.User.Events
{
    public interface ISourceBuilder<T>
    {
       ISourceMethodBuilder<T> SourceMethod();
    }
}
