using System;

using UnityEngine;

namespace Common {
	/**
	 * A generic named variable
	 */
	[Serializable]
	public class NamedValue<T> : NamedValueHolder {
		[SerializeField]
		private string name;
		
		[SerializeField]
		private T varValue;

        [SerializeField]
        private bool useOtherHolder;

        [SerializeField]
        private string otherHolderName;

        /**
		 * Default constructor
		 */
        public NamedValue() {
		}
		
		/**
		 * Constructor with specified name and variable value
		 */
		public NamedValue(string name, T varValue) {
			this.name = name;
			this.varValue = varValue;
		}
		
		public string Name {
			get => this.name;
			set => this.name = value;
		}

        public bool UseOtherHolder {
            get => this.useOtherHolder;
            set => this.useOtherHolder = value;
        }

        public string OtherHolderName {
            get => this.otherHolderName;
            set => this.otherHolderName = value;
        }

        public virtual T Value {
			get => this.varValue;
			set => this.varValue = value;
        }

        protected T VariableInstance {
            get => this.varValue;
            set => this.varValue = value;
        }

        public void ClearName() {
            this.name = null;
        }

        public void ClearOtherHolderName() {
            this.otherHolderName = null;
        }

        public object Get() {
            return this.Value;
        }

        public void Set(object value) {
            this.Value = (T)value;
        }
    }
}
