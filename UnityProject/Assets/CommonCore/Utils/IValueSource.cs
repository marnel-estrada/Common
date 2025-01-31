namespace Common {
    /// <summary>
    /// A common interface to get a certain value from.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueSource<out T> {
        T Value { get; }
    }
}