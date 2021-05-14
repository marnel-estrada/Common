using UnityEngine;
using System.Collections.Generic;
using System;

namespace Common.Xml {
	public static class SimpleXmlReader {
	    private const char TAG_START = '<';
	    private const char TAG_END = '>';
	    private const char SPACE = ' ';
	    private const char QUOTE = '"';
	    private const char SLASH = '/';
	    private const char EQUALS = '=';
	    private const char EXCLAMATION = '!';
        private const char DASH = '-';
        private const char QUESTION_MARK = '?';
	    private static readonly string BEGIN_QUOTE = "" + EQUALS + QUOTE;
	
	    public static SimpleXmlNode Read(string xml) {
	        int tagEndIndex = 0;
	        SimpleXmlNode rootNode = new SimpleXmlNode();
	        SimpleXmlNode currentNode = rootNode;

			int xmlLength = xml.Length;

            bool commentStarted = false;
            int lastCommentIndex = -1;
	
	        while (true) {
		        if(commentStarted) {
			        lastCommentIndex = xml.IndexOf(TAG_END, lastCommentIndex + 1);

			        if(lastCommentIndex < 0) {
				        // Already past the whole XML
				        break;
			        }

			        if(IsEndComment(xml, lastCommentIndex)) {
				        // It's a comment ending. Processing can continue.
				        tagEndIndex = lastCommentIndex;
				        commentStarted = false;
			        } else {
				        // Not a comment ending. Scan next ending tag.
				        continue;
			        }
		        }

		        int index = xml.IndexOf(TAG_START, tagEndIndex);

		        if (index < 0 || index >= xmlLength) {
			        break;
		        }

		        // Check for comment
		        if(IsStartComment(xml, index)) {
			        lastCommentIndex = index;
			        commentStarted = true;
			        continue;
		        }
	
		        index++;
	
		        tagEndIndex = xml.IndexOf(TAG_END, index);
		        if (tagEndIndex < 0 || tagEndIndex >= xmlLength) {
			        break;
		        }

		        // Resolve innertext
		        string innerText = "";
		        if(xml[tagEndIndex] != SLASH) {
			        // This means that that tag does not have "/>" and therefore has an inner text
			        int nextStartTagIndex = xml.IndexOf(TAG_START, tagEndIndex);

			        if (nextStartTagIndex > 0 && nextStartTagIndex < xmlLength) {
				        // Found a start tag. The inner text is between them.
				        int length = nextStartTagIndex - tagEndIndex - 1;
				        innerText = xml.Substring(tagEndIndex + 1, length);
			        }
		        }
	
		        int tagLength = tagEndIndex - index;
		        string xmlTag = xml.Substring(index, tagLength);
	
		        // if the tag starts with a </ then it is an end tag
		        if (xmlTag[0] == SLASH){
			        currentNode = currentNode.ParentNode;
			        continue;
		        }
	
		        bool openTag = !(xmlTag[0] == EXCLAMATION || xmlTag[0] == QUESTION_MARK);

		        // if the tag starts with <! or <? it's a comment

		        // if the tag ends in /> the tag can be considered closed
		        if (xmlTag[tagLength - 1] == SLASH) {
			        // cut away the slash
			        xmlTag = xmlTag.Substring(0, tagLength - 1);
			        openTag = false;
		        }
	
		        SimpleXmlNode node = ParseTag(xmlTag);
		        node.ParentNode = currentNode;
		        node.InnerText = innerText.Trim();
		        currentNode.AddChild(node);

		        if (openTag) {
			        currentNode = node;
		        }
	        }
	        return rootNode;
	    }

        private static bool IsStartComment(string xml, int startIndex) {
            // Check for "!--"
            if(startIndex + 3 >= xml.Length) {
                // Already at the end of the file
                return false;
            }
            return xml[startIndex + 1] == EXCLAMATION && xml[startIndex + 2] == DASH && xml[startIndex + 3] == DASH;
        }

        private static bool IsEndComment(string xml, int tagEndIndex) {
            // Check for "--"
            Assertion.IsTrue(tagEndIndex - 2 >= 0);
            return xml[tagEndIndex - 2] == DASH && xml[tagEndIndex - 1] == DASH;
        }
	
	    private static SimpleXmlNode ParseTag(string xmlTag) {
	        SimpleXmlNode node = new SimpleXmlNode();
	
	        int nameEnd = xmlTag.IndexOf(SPACE, 0);
	        if (nameEnd < 0) {
	            node.TagName = xmlTag;
	            return node;
	        }
	
	        string tagName = xmlTag.Substring(0, nameEnd);
	        node.TagName = tagName;
	
	        string attrString = xmlTag.Substring(nameEnd, xmlTag.Length - nameEnd);
	        return ParseAttributes(attrString, node);
	    }

	    private static SimpleXmlNode ParseAttributes(string xmlTag, SimpleXmlNode node) {
	        int lastIndex = 0;
	
	        while (true) {
	            int index = xmlTag.IndexOf(BEGIN_QUOTE, lastIndex, StringComparison.Ordinal);
	            if (index < 0 || index > xmlTag.Length) break;
	
	            int attrNameIndex = xmlTag.LastIndexOf(SPACE, index);
	            if (attrNameIndex < 0 || attrNameIndex > xmlTag.Length) break;
	
	            attrNameIndex++;
	            string attrName = xmlTag.Substring(attrNameIndex, index - attrNameIndex);
	
	            // skip the equal and quote character
                // TODO Must be able to handle spaces between equals
	            index += 2;
	
	            lastIndex = xmlTag.IndexOf(QUOTE, index);
	            if (lastIndex < 0 || lastIndex > xmlTag.Length) break;
	
	            int tagLength = lastIndex - index;
	            string attrValue = xmlTag.Substring(index, tagLength);
	
	            node.AddAttribute(attrName, attrValue);
	        }
	
	        return node;
	    }
	
	    public static void PrintXML(SimpleXmlNode node, int indent) {
	        indent++;
	
	        foreach (SimpleXmlNode n in node.Children) {
	            string attr = " ";
	            foreach (KeyValuePair<string, string> p in n.Attributes)
	                attr += "[" + p.Key + ": " + p.Value + "] ";
	
	            string indentString = "";
	            for (int i = 0; i < indent; i++)
	                indentString += "/";
	
	            Debug.Log(indentString + " " + n.TagName + attr);
	            PrintXML(n, indent);
	        }
	    }
	}
}

