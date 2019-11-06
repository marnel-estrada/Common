namespace Common {
    public interface IOptionMatcher<T> where T : class {
        /// <summary>
        /// Handling when there's a value
        /// </summary>
        /// <param name="value"></param>
        void OnSome(T value);

        /// <summary>
        /// Handling when there's no value
        /// </summary>
        void OnNone();
    }
}