namespace Common {
    public interface IDuplicable<in T> {
        /// <summary>
        /// Copies the values unto the specified copy
        /// </summary>
        /// <returns></returns>
        void DuplicateTo(T copy);
    }
}