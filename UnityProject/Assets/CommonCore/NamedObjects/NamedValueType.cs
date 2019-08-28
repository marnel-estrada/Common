using Common.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Common {
    /// <summary>
    /// An enum class for supported named value types
    /// This is an immutable class
    /// </summary>
    public class NamedValueType {

        public static readonly NamedValueType STRING = new NamedValueType(typeof(NamedString), typeof(string), "NamedString", "String");
        public static readonly NamedValueType INT = new NamedValueType(typeof(NamedInt), typeof(int), "NamedInt", "int");
        public static readonly NamedValueType FLOAT = new NamedValueType(typeof(NamedFloat), typeof(float), "NamedFloat", "float");
        public static readonly NamedValueType BOOL = new NamedValueType(typeof(NamedBool), typeof(bool), "NamedBool", "bool");
        public static readonly NamedValueType VECTOR3 = new NamedValueType(typeof(NamedVector3), typeof(Vector3), "NamedVector3", "Vector3");
        public static readonly NamedValueType INT_VECTOR2 = new NamedValueType(typeof(NamedIntVector2), typeof(IntVector2), "NamedIntVector2", "IntVector2");

        public static readonly NamedValueType[] ALL_TYPES = new NamedValueType[] {
            STRING,
            INT,
            FLOAT,
            VECTOR3,
            BOOL,
            INT_VECTOR2
        };

        public static readonly int SupportedTypesCount = ALL_TYPES.Length;

        /**
		 * Converts the specified value Type to an equivalent VariableType.
		 */
        public static NamedValueType ConvertFromValueType(Type type) {
            foreach (NamedValueType varType in ALL_TYPES) {
                if (type == varType.ValueType) {
                    return varType;
                }
            }

            Assertion.Assert(false, "Can't resolve the specified value Type: " + type.Name);
            return null;
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

            Assertion.Assert(false, "Can't resolve the specified property Type: " + type.Name);
            return null;
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
                return propertyType;
            }
        }

        public Type ValueType {
            get {
                return valueType;
            }
        }

        public string Label {
            get {
                return label;
            }
        }

        public string ValueTypeLabel {
            get {
                return valueTypeLabel;
            }
        }

    }
}
