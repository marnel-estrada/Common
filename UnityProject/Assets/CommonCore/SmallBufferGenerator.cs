using System.Text;

namespace Common {
    /// <summary>
    /// Code generator for structs representing a small buffer
    /// </summary>
    /// 
    /// <author>
    /// Jackson Dunstan, https://JacksonDunstan.com/articles/5051
    /// </author>
    /// 
    /// <license>
    /// MIT
    /// </license>
    public static class SmallBufferGenerator {
        /// <summary>
        /// Types of error handling
        /// </summary>
        public enum ErrorHandlingStrategy {
            /// <summary>
            /// Perform no error checking at all
            /// </summary>
            None,

            /// <summary>
            /// Check errors using Unity assertions
            /// </summary>
            UnityAssertions,

            /// <summary>
            /// Check errors using exceptions
            /// </summary>
            Exceptions,
        }

        /// <summary>
        /// Types of buffer elements
        /// </summary>
        public enum ElementType {
            /// <summary>
            /// Managed types
            /// </summary>
            Managed,

            /// <summary>
            /// Unmanaged types
            /// </summary>
            Unmanaged,

            /// <summary>
            /// Unmanaged types with support for usage in a Unity Burst-compiled job
            /// </summary>
            UnmanagedWithUnityBurstSupport,

            /// <summary>
            /// Unmanaged types with support for usage in a C# 7 application
            /// </summary>
            UnmanagedWithCsharp7Support,

            /// <summary>
            /// Unmanaged types with support for usage in a C# 7 application and
            /// in a Unity Burst-compiled job
            /// </summary>
            UnmanagedWithCsharp7SupportAndUnityBurstSupport,
        }

        /// <summary>
        /// One level of indentation
        /// </summary>
        private const char OneIndent = '\t';

        /// <summary>
        /// Generate a C# source file for a small buffer type
        /// </summary>
        /// 
        /// <returns>
        /// The generated C# source file
        /// </returns>
        /// 
        /// <param name="namespaceName">
        /// Namespace to put the generated type in
        /// </param>
        /// 
        /// <param name="typeName">
        /// Name of the generated type
        /// </param>
        /// 
        /// <param name="capacity">
        /// Capacity of the buffer to generate in number of elements
        /// </param>
        /// 
        /// <param name="isFixedLength">
        /// If the generated buffer has a fixed length
        /// </param>
        /// 
        /// <param name="boundsCheckStrategy">
        /// Error handling strategy the generated type should use to handle
        /// out-of-bounds errors
        /// </param>
        /// 
        /// <param name="versionCheckStrategy">
        /// Error handling strategy the generated type should use to handle
        /// version check errors (i.e. if the buffer is changed during enumeration)
        /// </param>
        /// 
        /// <param name="elementTypeName">
        /// Name of the type of elements stored in the buffer. Should be usable
        /// without any 'using' statements. Pass null to make the type generic.
        /// </param>
        /// 
        /// <param name="elementType">
        /// Type of the element
        /// </param>
        public static string Generate(string namespaceName, string typeName, int capacity, bool isFixedLength,
            ErrorHandlingStrategy boundsCheckStrategy, ErrorHandlingStrategy versionCheckStrategy,
            string elementTypeName, ElementType elementType) {
            // Replace type name with 'T' to make it generic
            bool isGeneric;
            if (elementTypeName == null) {
                isGeneric = true;
                elementTypeName = "T";
            } else {
                isGeneric = false;
            }

            // Decide if Unity Burst is enabled or not
            bool enableBurst;
            switch (elementType) {
                case ElementType.UnmanagedWithUnityBurstSupport:
                case ElementType.UnmanagedWithCsharp7SupportAndUnityBurstSupport:
                    enableBurst = true;

                    break;
                default:
                    enableBurst = false;

                    break;
            }

            // Decide if C# 7 is enabled or not
            bool enableCsharp7;
            switch (elementType) {
                case ElementType.UnmanagedWithCsharp7Support:
                case ElementType.UnmanagedWithCsharp7SupportAndUnityBurstSupport:
                    enableCsharp7 = true;

                    break;
                default:
                    enableCsharp7 = false;

                    break;
            }

            int indentLevel = 0;

            // File header
            StringBuilder output = new StringBuilder(1024 * 64);
            output.AppendLine("////////////////////////////////////////////////////////////////////////////////");
            output.AppendLine("// Warning: This file was automatically generated by SmallBufferGenerator.");
            output.AppendLine("//          If you edit this by hand, the next run of SmallBufferGenerator");
            output.AppendLine("//          will overwrite your edits.");
            output.AppendLine("////////////////////////////////////////////////////////////////////////////////");
            output.AppendLine();

            // Begin namespace
            output.Append("namespace ");
            output.AppendLine(namespaceName);
            output.AppendLine("{");
            indentLevel++;

            // Begin struct
            output.Append(OneIndent, indentLevel);
            output.AppendLine("[System.Runtime.InteropServices.StructLayout(");
            indentLevel++;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("System.Runtime.InteropServices.LayoutKind.Sequential)]");
            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.Append("public ");
            if (elementType != ElementType.Managed) {
                output.Append("unsafe ");
            }

            output.Append("struct ");
            output.Append(typeName);
            if (isGeneric) {
                output.Append('<');
                output.Append(elementTypeName);
                output.AppendLine(">");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.Append("where T : unmanaged");
                indentLevel--;
            }

            output.AppendLine();
            output.Append(OneIndent, indentLevel);
            output.AppendLine("{");
            indentLevel++;

            if (elementType != ElementType.Managed && enableCsharp7) {
                // Begin enumerator struct
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public ref struct Enumerator");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;

                // Enumerator buffer field
                output.Append(OneIndent, indentLevel);
                output.Append("private readonly ");
                output.Append(elementTypeName);
                output.AppendLine("* m_Elements;");

                // Enumerator index field
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("private int m_Index;");

                // Enumerator version field
                if (!isFixedLength) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine();
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("private readonly int m_OriginalVersion;");

                    output.Append(OneIndent, indentLevel);
                    output.AppendLine();
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("private readonly int* m_Version;");

                    output.Append(OneIndent, indentLevel);
                    output.AppendLine();
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("private readonly int m_Length;");
                }

                // Enumerator constructor
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.Append("public Enumerator(");
                output.Append(elementTypeName);
                output.Append("* elements");
                if (!isFixedLength) {
                    output.Append(", int* version, int length");
                }

                output.AppendLine(")");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Elements = elements;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Index = -1;");
                if (!isFixedLength) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("m_OriginalVersion = *version;");
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("m_Version = version;");
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("m_Length = length;");
                }

                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine();

                // Enumerator MoveNext method
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public bool MoveNext()");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                if (!isFixedLength) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("RequireVersionMatch();");
                }

                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Index++;");
                output.Append(OneIndent, indentLevel);
                output.Append("return m_Index < ");
                if (isFixedLength) {
                    output.Append(capacity);
                } else {
                    output.Append("m_Length");
                }

                output.AppendLine(";");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine();

                // Enumerator Current property
                output.Append(OneIndent, indentLevel);
                output.Append("public ref ");
                output.Append(elementTypeName);
                output.AppendLine(" Current");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("get");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                if (!isFixedLength) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("RequireVersionMatch();");
                }

                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireIndexInBounds();");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("return ref m_Elements[m_Index];");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                if (!isFixedLength) {
                    // RequireVersionMatch method
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine();
                    if (enableBurst) {
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("[Unity.Burst.BurstDiscard]");
                    }

                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("public void RequireVersionMatch()");
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("{");
                    indentLevel++;
                    switch (versionCheckStrategy) {
                        case ErrorHandlingStrategy.UnityAssertions:
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("UnityEngine.Assertions.Assert.IsTrue(");
                            indentLevel++;
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("m_OriginalVersion == *m_Version,");
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("\"Buffer modified during enumeration\");");
                            indentLevel--;

                            break;
                        case ErrorHandlingStrategy.Exceptions:
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("if (m_OriginalVersion != *m_Version)");
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("{");
                            indentLevel++;
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("throw new System.InvalidOperationException(");
                            indentLevel++;
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("\"Buffer modified during enumeration\");");
                            indentLevel--;
                            indentLevel--;
                            output.Append(OneIndent, indentLevel);
                            output.AppendLine("}");

                            break;
                    }

                    indentLevel--;
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("}");
                }

                // RequireIndexInBounds method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                if (enableBurst) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("[Unity.Burst.BurstDiscard]");
                }

                output.Append(OneIndent, indentLevel);
                output.AppendLine("public void RequireIndexInBounds()");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                AppendBoundsCheck(output, "m_Index", indentLevel, isFixedLength, capacity, boundsCheckStrategy);
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                // End enumerator struct
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            // Element fields
            for (int i = 0; i < capacity; ++i) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.Append("private ");
                if (elementType != ElementType.Managed && enableCsharp7) {
                    output.Append("readonly ");
                }

                output.Append(elementTypeName);
                output.Append(" m_Element");
                output.Append(i);
                output.AppendLine(";");
            }

            if (!isFixedLength) {
                // Version field
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("private int m_Version;");

                // Length field
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("private int m_Length;");
            }

            // Indexer
            output.Append(OneIndent, indentLevel);
            output.AppendLine();
            output.Append(OneIndent, indentLevel);
            output.Append("public ");
            if (elementType != ElementType.Managed && enableCsharp7) {
                output.Append("ref ");
            }

            output.Append(elementTypeName);
            output.AppendLine(" this[int index]");
            output.Append(OneIndent, indentLevel);
            output.AppendLine("{");
            indentLevel++;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("get");
            output.Append(OneIndent, indentLevel);
            output.AppendLine("{");
            indentLevel++;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("RequireIndexInBounds(index);");
            output.Append(OneIndent, indentLevel);
            output.Append("return ");
            if (elementType != ElementType.Managed && enableCsharp7) {
                output.Append("ref ");
            }

            output.AppendLine("GetElement(index);");
            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("}");
            if (elementType == ElementType.Managed || !enableCsharp7) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine("set");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireIndexInBounds(index);");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(index, value);");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("}");

            // GetElement
            output.Append(OneIndent, indentLevel);
            output.AppendLine();
            output.Append(OneIndent, indentLevel);
            output.Append("private ");
            if (elementType != ElementType.Managed && enableCsharp7) {
                output.Append("ref ");
            }

            output.Append(elementTypeName);
            output.AppendLine(" GetElement(int index)");
            output.Append(OneIndent, indentLevel);
            output.AppendLine("{");
            indentLevel++;
            if (elementType == ElementType.Managed) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine("switch (index)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                for (int i = 0; i < capacity; ++i) {
                    output.Append(OneIndent, indentLevel);
                    output.Append("case ");
                    output.Append(i);
                    output.Append(": return ");
                    if (enableCsharp7) {
                        output.Append("ref ");
                    }

                    output.Append("m_Element");
                    output.Append(i);
                    output.AppendLine(";");
                }

                output.Append(OneIndent, indentLevel);
                output.Append("default: return default(");
                output.Append(elementTypeName);
                output.AppendLine(");");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            } else {
                output.Append(OneIndent, indentLevel);
                output.Append("fixed (");
                output.Append(elementTypeName);
                output.AppendLine("* elements = &m_Element0)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.Append("return ");
                if (enableCsharp7) {
                    output.Append("ref ");
                }

                output.AppendLine("elements[index];");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("}");

            // SetElement
            output.Append(OneIndent, indentLevel);
            output.AppendLine();
            output.Append(OneIndent, indentLevel);
            output.Append("private void SetElement(int index, ");
            output.Append(elementTypeName);
            output.AppendLine(" value)");
            output.Append(OneIndent, indentLevel);
            output.AppendLine("{");
            indentLevel++;
            if (elementType == ElementType.Managed) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine("switch (index)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                for (int i = 0; i < capacity; ++i) {
                    output.Append(OneIndent, indentLevel);
                    output.Append("case ");
                    output.Append(i);
                    output.Append(": m_Element");
                    output.Append(i);
                    output.AppendLine(" = value; break;");
                }

                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            } else {
                output.Append(OneIndent, indentLevel);
                output.Append("fixed (");
                output.Append(elementTypeName);
                output.AppendLine("* elements = &m_Element0)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("elements[index] = value;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("}");

            // Length constant or property
            output.Append(OneIndent, indentLevel);
            output.AppendLine();
            if (isFixedLength) {
                output.Append(OneIndent, indentLevel);
                output.Append("public const int Length = ");
                output.Append(capacity);
                output.AppendLine(";");
            } else {
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public int Count");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("get");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("return m_Length;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            // Capacity constant
            if (!isFixedLength) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.Append("public const int Capacity = ");
                output.Append(capacity);
                output.AppendLine(";");
            }

            // GetEnumerator method
            if (elementType != ElementType.Managed && enableCsharp7) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public Enumerator GetEnumerator()");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("// Safe because Enumerator is a 'ref struct'");
                output.Append(OneIndent, indentLevel);
                output.Append("fixed (");
                output.Append(elementTypeName);
                output.AppendLine("* elements = &m_Element0)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                if (isFixedLength) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("return new Enumerator(elements);");
                } else {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("fixed (int* version = &m_Version)");
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("{");
                    indentLevel++;
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("return new Enumerator(elements, version, m_Length);");
                    indentLevel--;
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("}");
                }

                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            if (!isFixedLength) {
                // Add method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.Append("public void Add(");
                output.Append(elementTypeName);
                output.AppendLine(" item)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireNotFull();");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(m_Length, item);");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Length++;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Version++;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                // Clear method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public void Clear()");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("for (int i = 0; i < m_Length; ++i)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.Append("SetElement(i, default(");
                output.Append(elementTypeName);
                output.AppendLine("));");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Length = 0;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Version++;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                // Insert method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.Append("public void Insert(int index, ");
                output.Append(elementTypeName);
                output.AppendLine(" value)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireNotFull();");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireIndexInBounds(index);");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("for (int i = m_Length; i > index; --i)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(i, GetElement(i - 1));");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(index, value);");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Length++;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Version++;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                // RemoveAt method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public void RemoveAt(int index)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireIndexInBounds(index);");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("for (int i = index; i < m_Length - 1; ++i)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(i, GetElement(i + 1));");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Length--;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Version++;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                // RemoveRange method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                output.Append(OneIndent, indentLevel);
                output.AppendLine("public void RemoveRange(int index, int count)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireIndexInBounds(index);");
                switch (boundsCheckStrategy) {
                    case ErrorHandlingStrategy.UnityAssertions:
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("UnityEngine.Assertions.Assert.IsTrue(");
                        indentLevel++;
                        output.Append(OneIndent, indentLevel);
                        output.Append("count >= 0");
                        output.AppendLine(",");
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("\"Count must be positive: \" + count);");
                        indentLevel--;

                        break;
                    case ErrorHandlingStrategy.Exceptions:
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("if (count < 0)");
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("{");
                        indentLevel++;
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("throw new System.ArgumentOutOfRangeException(");
                        indentLevel++;
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("\"count\",");
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("\"Count must be positive: \" + count);");
                        indentLevel--;
                        indentLevel--;
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("}");

                        break;
                }

                output.Append(OneIndent, indentLevel);
                output.AppendLine("RequireIndexInBounds(index + count - 1);");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("int indexAfter = index + count;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("int indexEndCopy = indexAfter + count;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("if (indexEndCopy >= m_Length)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("indexEndCopy = m_Length;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("int numCopies = indexEndCopy - indexAfter;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("for (int i = 0; i < numCopies; ++i)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(index + i, GetElement(index + count + i));");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("for (int i = indexAfter; i < m_Length - 1; ++i)");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("SetElement(i, GetElement(i + 1));");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Length -= count;");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("m_Version++;");
                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");

                // RequireNotFull method
                output.Append(OneIndent, indentLevel);
                output.AppendLine();
                if (enableBurst) {
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("[Unity.Burst.BurstDiscard]");
                }

                output.Append(OneIndent, indentLevel);
                output.AppendLine("public void RequireNotFull()");
                output.Append(OneIndent, indentLevel);
                output.AppendLine("{");
                indentLevel++;
                switch (boundsCheckStrategy) {
                    case ErrorHandlingStrategy.UnityAssertions:
                        output.Append(OneIndent, indentLevel);
                        output.Append("UnityEngine.Assertions.Assert.IsTrue(m_Length != ");
                        output.Append(capacity);
                        output.AppendLine(", \"Buffer overflow\");");

                        break;
                    case ErrorHandlingStrategy.Exceptions:
                        output.Append(OneIndent, indentLevel);
                        output.Append("if (m_Length == ");
                        output.Append(capacity);
                        output.AppendLine(")");
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("{");
                        indentLevel++;
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("throw new System.InvalidOperationException(");
                        indentLevel++;
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("\"Buffer overflow\");");
                        indentLevel--;
                        indentLevel--;
                        output.Append(OneIndent, indentLevel);
                        output.AppendLine("}");

                        break;
                }

                indentLevel--;
                output.Append(OneIndent, indentLevel);
                output.AppendLine("}");
            }

            // RequireIndexInBounds method
            output.Append(OneIndent, indentLevel);
            output.AppendLine();
            if (enableBurst) {
                output.Append(OneIndent, indentLevel);
                output.AppendLine("[Unity.Burst.BurstDiscard]");
            }

            output.Append(OneIndent, indentLevel);
            output.AppendLine("public void RequireIndexInBounds(int index)");
            output.Append(OneIndent, indentLevel);
            output.AppendLine("{");
            indentLevel++;
            AppendBoundsCheck(output, "index", indentLevel, isFixedLength, capacity, boundsCheckStrategy);
            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("}");

            // End of struct
            indentLevel--;
            output.Append(OneIndent, indentLevel);
            output.AppendLine("}");

            // End of namespace
            output.AppendLine("}");

            return output.ToString();
        }

        private static void AppendBoundsCheck(StringBuilder output, string indexName, int indentLevel,
            bool isFixedLength, int capacity, ErrorHandlingStrategy boundsCheckStrategy) {
            switch (boundsCheckStrategy) {
                case ErrorHandlingStrategy.UnityAssertions:
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("UnityEngine.Assertions.Assert.IsTrue(");
                    indentLevel++;
                    output.Append(OneIndent, indentLevel);
                    output.Append(indexName);
                    output.Append(" >= 0 && ");
                    output.Append(indexName);
                    output.Append(" < ");
                    if (isFixedLength) {
                        output.Append(capacity);
                    } else {
                        output.Append("m_Length");
                    }

                    output.AppendLine(",");
                    output.Append(OneIndent, indentLevel);
                    output.Append("\"Index out of bounds: \" + ");
                    output.Append(indexName);
                    output.AppendLine(");");
                    indentLevel--;

                    break;
                case ErrorHandlingStrategy.Exceptions:
                    output.Append(OneIndent, indentLevel);
                    output.Append("if (");
                    output.Append(indexName);
                    output.Append(" < 0 || ");
                    output.Append(indexName);
                    output.Append(" >= ");
                    if (isFixedLength) {
                        output.Append(capacity);
                    } else {
                        output.Append("m_Length");
                    }

                    output.AppendLine(")");
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("{");
                    indentLevel++;
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("throw new System.InvalidOperationException(");
                    indentLevel++;
                    output.Append(OneIndent, indentLevel);
                    output.Append("\"Index out of bounds: \" + ");
                    output.Append(indexName);
                    output.AppendLine(");");
                    indentLevel--;
                    indentLevel--;
                    output.Append(OneIndent, indentLevel);
                    output.AppendLine("}");

                    break;
            }
        }
    }
}