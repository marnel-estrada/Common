namespace Common {
    public interface IDuplicable<out T> {
        /// <summary>
        /// Creates the duplicate
        /// </summary>
        /// <returns></returns>
        T Duplicate();
    }
}