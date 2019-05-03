using System;
using UnityEngine;

namespace Common {
	public static class Assertion {
		public const string DEFAULT_MESSAGE = "AssertionError";
	
		/**
		 * Asserts the specified expression
		 */
		public static void Assert(bool expression, UnityEngine.Object context = null) {
			Assert(expression, DEFAULT_MESSAGE, context);
		}
	
		/**
		 * Asserts the specified expression.
		 */
		public static void Assert(bool expression, string assertErrorMessage, UnityEngine.Object context = null) {
			if (!expression) {
				Debug.LogError(assertErrorMessage, context);
				
#if UNITY_EDITOR
				// Always throw the exception if on editor so we are forced to debug
				throw new Exception(assertErrorMessage);
#endif		
			}
		}
	
		/**
		 * Asserts that the specified pointer is not null.
		 */
		public static void AssertNotNull(object pointer, string name, UnityEngine.Object context = null) {
			Assert(pointer != null, name, context);
		}
	
		/**
		 * Asserts that the specified pointer is not null.
		 */
		public static void AssertNotNull(object pointer, UnityEngine.Object context = null) {
			Assert(pointer != null, DEFAULT_MESSAGE, context);
		}
		
		/**
		 * Asserts that the specified UnityEngine object is not null.
		 */
		public static void AssertNotNull(UnityEngine.Object pointer, string name, UnityEngine.Object context = null) {
			if(!pointer) {
				Assert(false, name, context);
			}
		}
	
		/**
		 * Asserts that the specified UnityEngine object is not null.
		 */
		public static void AssertNotNull(UnityEngine.Object pointer, UnityEngine.Object context = null) {
			if(!pointer) {
				Assert(false, DEFAULT_MESSAGE, context);
			}
		}
		
		/**
		 * Asserts that the specified string is not empty.
		 */
		public static void AssertNotEmpty(string s, string name, UnityEngine.Object context = null) {
			Assert(!string.IsNullOrEmpty(s), name, context);
		}
	
		
		/**
		 * Asserts that the specified string is not empty.
		 */
		public static void AssertNotEmpty(string s, UnityEngine.Object context = null) {
			Assert(!string.IsNullOrEmpty(s), DEFAULT_MESSAGE, context);
		}	
	}
}

