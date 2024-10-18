using System;

using Unity.Mathematics;

using UnityEngine;

namespace Common {
    /// <summary>
    /// An enum class for supported named value types
    /// This is an immutable class
    /// </summary>
    public class NamedValueType {
        public static readonly NamedValueType STRING = new(typeof(NamedString), typeof(string), "NamedString", "String");
        public static readonly NamedValueType INT = new(typeof(NamedInt), typeof(int), "NamedInt", "int");
        public static readonly NamedValueType FLOAT = new(typeof(NamedFloat), typeof(float), "NamedFloat", "float");
        public static readonly NamedValueType BOOL = new(typeof(NamedBool), typeof(bool), "NamedBool", "bool");
        public static readonly NamedValueType VECTOR3 = new(typeof(NamedVector3), typeof(Vector3), "NamedVector3", "Vector3");
        public static readonly NamedValueType INT2 = new(typeof(NamedInt2), typeof(int2), "NamedInt2", "int2");
        public static readonly NamedValueType INT3 = new(typeof(NamedInt3), typeof(int3), "NamedInt3", "int3");

        public static readonly NamedValueType[] ALL_TYPES = new NamedValueType[] {
            STRING,
            INT,
            FLOAT,
            VECTOR3,
            BOOL,
            INT2,
            INT3
        };

        /**
		 * Converts the specified value Type to an equivalent VariableType.
		 */
        public static NamedValueType ConvertFromValueType(Type type) {
            foreach (NamedValueType varType in ALL_TYPES) {
                if (type == varType.ValueType) {
                    return varType;
                }
            }

            throw new Exception("Can't resolve the specified value Type: " + type.Name);
        }

        /**
		 * Converts the specified property type to a variable type
		 */
        public static NamedValueType ConvertFromPropertyType(Type type) {
            foreach (NamedValueType varType in ALL_TYPES) {
                if (type == varType.PropertyType) {
                    return varType;
                }
            }

            throw new Exception("Can't resolve the specified property Type: " + type.Name);
        }

        /**
		 * Returns whether or not the specified property type is a supported variable property
		 */
        public static bool IsSupportedNamedType(Type propertyType) {
            foreach (NamedValueType varType in ALL_TYPES) {
                if (propertyType == varType.PropertyType) {
                    return true;
                }
            }

            return false;
        }

        /**
		 * Returns whether or not the specified value type is a supported variable property
		 */
        public static bool IsSupportedValueType(Type type) {
            foreach (NamedValueType varType in ALL_TYPES) {
                if (type == varType.ValueType) {
                    return true;
                }
            }

            return false;
        }

        private readonly Type propertyType; // the type to map when it is a property in an action
        private readonly Type valueType; // the type of the value that is assigned to the variable
        private readonly string label;
        private readonly string valueTypeLabel;

        /**
		 * Constructor with specified type and label
		 */
        private NamedValueType(Type propertyType, Type valueType, string label, string valueTypeLabel) {
            this.propertyType = propertyType;
            this.valueType = valueType;
            this.label = label;
            this.valueTypeLabel = valueTypeLabel;
        }

        public Type PropertyType {
            get {
                return this.propertyType;
            }
        }

        public Type ValueType {
            get {
                return this.valueType;
            }
        }

        public string Label {
            get {
                return this.label;
            }
        }

        public string ValueTypeLabel {
            get {
                return this.valueTypeLabel;
            }
        }

    }
}
