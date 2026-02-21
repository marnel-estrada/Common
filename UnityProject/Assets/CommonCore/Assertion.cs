using System;
using UnityEngine;

#nullable enable

namespace Common {
	public static class Assertion {
		public const string DEFAULT_MESSAGE = "AssertionError";
	
		/// <summary>
		/// Asserts that the expression is true
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="context"></param>
		public static void IsTrue(bool expression, UnityEngine.Object? context = null) {
			IsTrue(expression, DEFAULT_MESSAGE, context);
		}
	
		/// <summary>
		/// Asserts that the expression is true
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="assertErrorMessage"></param>
		/// <param name="context"></param>
		/// <exception cref="Exception"></exception>
		public static void IsTrue(bool expression, string assertErrorMessage, UnityEngine.Object? context = null) {
			if (expression) {
				return;
			}

			Debug.LogError(assertErrorMessage, context);
				
#if UNITY_EDITOR
			// Always throw the exception if on editor so we are forced to debug
			throw new Exception(assertErrorMessage);
#endif
		}

		/// <summary>
		/// Asserts that the expression is false
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="context"></param>
		public static void IsFalse(bool expression, UnityEngine.Object? context = null) {
			IsFalse(expression, DEFAULT_MESSAGE, context);
		}

		/// <summary>
		/// Asserts that the expression is false
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="assertErrorMessage"></param>
		/// <param name="context"></param>
		/// <exception cref="Exception"></exception>
		public static void IsFalse(bool expression, string assertErrorMessage, UnityEngine.Object? context = null) {
			if (!expression) {
				// Expression is already false
				return;
			}
			
			Debug.LogError(assertErrorMessage, context);
				
#if UNITY_EDITOR
			// Always throw the exception if on editor so we are forced to debug
			throw new Exception(assertErrorMessage);
#endif
		}
	
		public static void NotNull(object? pointer, string name, UnityEngine.Object? context = null) {
			IsTrue(pointer != null, name, context);
		}
	
		public static void NotNull(object? pointer, UnityEngine.Object? context = null) {
			IsTrue(pointer != null, DEFAULT_MESSAGE, context);
		}
		
		public static unsafe void NotNull(void* address, UnityEngine.Object? context = null) {
			IsTrue(address != null, DEFAULT_MESSAGE, context);
		}
		
		public static void IsSome<T>(Option<T> option, UnityEngine.Object? context = null) where T : class {
			IsTrue(option.IsSome, "Option should be Some. Got a None instead.", context);
		}

		public static void IsSome<T>(Option<T> option, string name, UnityEngine.Object? context = null) where T : class {
			IsTrue(option.IsSome, name, context);
		}
		
		public static void NotNull(UnityEngine.Object? pointer, string name, UnityEngine.Object? context = null) {
			if(!pointer) {
				IsTrue(false, name, context);
			}
		}
	
		public static void NotNull(UnityEngine.Object? pointer, UnityEngine.Object? context = null) {
			if(!pointer) {
				IsTrue(false, DEFAULT_MESSAGE, context);
			}
		}
		
		/**
		 * Asserts that the specified string is not empty.
		 */
		public static void NotEmpty(string? s, string name, UnityEngine.Object? context = null) {
			IsTrue(!string.IsNullOrWhiteSpace(s), name, context);
		}
	
		
		/**
		 * Asserts that the specified string is not empty.
		 */
		public static void NotEmpty(string? s, UnityEngine.Object? context = null) {
			IsTrue( !string.IsNullOrWhiteSpace(s), DEFAULT_MESSAGE, context);
		}	
	}
}

