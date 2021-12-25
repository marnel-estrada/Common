namespace Common {
    public interface IDataPoolSidebarView<T> where T : class, IDataPoolItem, IDuplicable<T>, new() {
        void Render(DataPool<T> pool, int width = 250);

        bool IsValidSelection(DataPool<T> pool);

        T GetSelectedItem(DataPool<T> pool);

        bool ShouldSortItems { get; set; }

        void OnRepaint(DataPool<T> pool);

        void AddFilterStrategy(DataPoolFilterStrategy<T> strategy);

        void SelectItem(string itemId);
    }
}