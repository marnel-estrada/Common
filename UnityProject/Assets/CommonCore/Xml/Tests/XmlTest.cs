using Common;
using Common.Xml;

using UnityEngine;

public class XmlTest : MonoBehaviour {
    [SerializeField]
    private TextAsset sampleXml;

    private void Awake() {
        Assertion.AssertNotNull(this.sampleXml, "sampleXml");

        SimpleXmlReader reader = new SimpleXmlReader();
        reader.PrintXML(reader.Read(this.sampleXml.text), 0);
    }
}