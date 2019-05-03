using System.Collections.Generic;

namespace Common.Xml {
	public class SimpleXmlNodeList : List<SimpleXmlNode> {

	    public SimpleXmlNode Pop() {
	        SimpleXmlNode node = this[Count - 1];
	        RemoveAt(Count - 1);
	        return node;
	    }
	
	    public void Push(SimpleXmlNode node) {
	        Add(node);
	    }
	}

}

