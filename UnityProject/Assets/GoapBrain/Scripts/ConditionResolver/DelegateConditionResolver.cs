namespace GoapBrain {
    class DelegateConditionResolver : ConditionResolver {

        public delegate bool ResolverDelegate(GoapAgent agent);

        private readonly ResolverDelegate resolverAction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resolverAction"></param>
        public DelegateConditionResolver(ResolverDelegate resolverAction) {
            this.resolverAction = resolverAction;
        }

        protected override bool Resolve(GoapAgent agent) {
            if(this.resolverAction == null) {
                // Nothing specified 
                return false;
            }

            return this.resolverAction(agent); // Invoke
        }

    }
}
