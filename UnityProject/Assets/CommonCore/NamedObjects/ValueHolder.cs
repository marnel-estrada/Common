namespace Common {
    /// <summary>
    /// An interface for classes that holds a certain value
    /// </summary>
    public interface ValueHolder {
        /// <summary>
        /// Returns the value
        /// </summary>
        /// <returns></returns>
        object Get();

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value"></param>
        void Set(object value);

        /// <summary>
        /// Whether or not to use other ValueHolder instance to refer to its value
        /// </summary>
        bool UseOtherHolder { get; set; }

        /// <summary>
        /// Named of the other ValueHolder instance
        /// </summary>
        string OtherHolderName { get; set; }

        /// <summary>
        /// Clears the other holder name
        /// </summary>
        void ClearOtherHolderName();
    }
}
