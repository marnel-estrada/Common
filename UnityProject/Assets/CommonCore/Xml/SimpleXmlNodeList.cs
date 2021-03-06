using System.Collections.Generic;

namespace Common.Xml {
	public class SimpleXmlNodeList : List<SimpleXmlNode> {

	    public SimpleXmlNode Pop() {
	        SimpleXmlNode node = this[this.Count - 1];
	        RemoveAt(this.Count - 1);
	        return node;
	    }
	
	    public void Push(SimpleXmlNode node) {
	        Add(node);
	    }
	}

}

