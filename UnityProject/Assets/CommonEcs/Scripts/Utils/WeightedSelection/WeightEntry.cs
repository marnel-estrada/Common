namespace CommonEcs {
    internal struct WeightEntry {
        public uint weight;

        public uint start;
        public uint end;

        public WeightEntry(uint weight, uint start, uint end) : this() {
            this.weight = weight;
            this.start = start;
            this.end = end;
        }

        public bool IsWithinRange(uint value) {
            return this.start <= value && value <= this.end;
        }
    }
}