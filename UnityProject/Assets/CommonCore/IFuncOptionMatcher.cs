namespace Common {
    public interface IFuncOptionMatcher<in TWrappedType, out TReturnType> where TWrappedType : class {
        /// <summary>
        /// Handling when there's a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TReturnType OnSome(TWrappedType value);

        /// <summary>
        /// Handling when there's no value
        /// </summary>
        /// <returns></returns>
        TReturnType OnNone();
    }
}