namespace Common {
    public interface DataPoolItemRenderer<T> where T : Identifiable, IntIdentifiable, new() {
        /// <summary>
        /// Renders the item
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="item"></param>
        void Render(DataPool<T> pool, T item);
    }
}