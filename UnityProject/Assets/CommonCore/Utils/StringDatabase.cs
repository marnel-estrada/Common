using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common {
    /// <summary>
    /// A data structure that maps strings to an int and vice versa
    /// Note that there's no removal. This is important so that IDs reference to only one string.
    /// This will prevent bugs like referencing a certain string but it changed because the original
    /// entry was removed and now references another string.
    /// </summary>
    public class StringDatabase {

        private QuickMap<string> intToStringMap = new QuickMap<string>();

        private Dictionary<string, int> stringToIntMap = new Dictionary<string, int>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buffer"></param>
        public StringDatabase(int buffer) {
            intToStringMap = new QuickMap<string>(buffer);
        }

        /// <summary>
        /// Returns whether or not the database contains the specified string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool Contains(string str) {
            return this.stringToIntMap.ContainsKey(str);
        }

        /// <summary>
        /// Adds a string to the database
        /// Returns the id to such string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int Add(string str) {
            if(Contains(str)) {
                // Database already has the specified string
                return this.stringToIntMap[str];
            }

            // Add a new entry
            int id = this.intToStringMap.Add(str);
            this.stringToIntMap[str] = id;
            Assertion.IsTrue(this.intToStringMap.Get(this.stringToIntMap[str]) == str);

            return id;
        }

        /// <summary>
        /// Returns the ID for the specified string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int GetId(string str) {
            // Throws error if string is not in database
            return this.stringToIntMap[str];
        }

        /// <summary>
        /// Returns the string associated with the specified int ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetString(int id) {
            return this.intToStringMap.Get(id);
        }

    }
}
