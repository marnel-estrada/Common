using Common;
using Common.Xml;

using UnityEngine;

public class XmlTest : MonoBehaviour {
    [SerializeField]
    private TextAsset sampleXml;

    private void Awake() {
        Assertion.NotNull(this.sampleXml, "sampleXml");

        SimpleXmlReader.PrintXML(SimpleXmlReader.Read(this.sampleXml.text), 0);
    }
}