using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common {
    /// <summary>
    /// A common interface for containers that can contain a named value
    /// Note here that it's not a generic class
    /// This is so we can use it without specifying a type
    /// </summary>
    public interface NamedValueContainer {

        /**
         * Clears the container of named values
         */
        void Clear();

        /**
		 * Adds a variable
		 */
        void Add(string name);

        /**
		 * Returns whether or not the containter contains the specified variable
		 */
        bool Contains(string name);

        /**
		 * Removes the specified variable
		 */
        void Remove(string name);

        /**
		 * Returns the variable instance
         * Note that the object returned here is the Named* instance. Not its value.
		 */
        object Get(string name);

        /// <summary>
        /// Returns a copy of the variable with the specified name
        /// Note that the object returned here is the Named* instance. Not its value.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetCopy(string name);

        /**
         * Returns the number of contents
         */
        int Count { get; }

        /**
         * Returns the entry at the specified index
         */
        object GetAt(int index);

        /// <summary>
        /// Returns the name of the named instance at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetNameAt(int index);

        /// <summary>
        /// Returns the NamedValueHolder at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        NamedValueHolder GetNamedValueHolderAt(int index);

    }
}
