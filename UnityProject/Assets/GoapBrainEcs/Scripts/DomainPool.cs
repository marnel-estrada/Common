using System.Collections.Generic;

namespace GoapBrainEcs {
    public class DomainPool {
        private readonly Dictionary<ushort, GoapDomain> domainMap = new Dictionary<ushort, GoapDomain>(1);

        /// <summary>
        /// Adds a domain
        /// </summary>
        /// <param name="domain"></param>
        public void Add(GoapDomain domain) {
            this.domainMap[domain.Id] = domain;
        }

        /// <summary>
        /// Returns the GoapDomain with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GoapDomain Get(ushort id) {
            return this.domainMap[id];
        }
    }
}