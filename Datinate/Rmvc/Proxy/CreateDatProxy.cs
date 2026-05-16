using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using System.Diagnostics;
using System.Xml;

namespace com.RADIO.Datinate.RMVC
{
    internal class CreateDatProxy : RModel 
    {
        protected override void Initialise()
        {

        }

        public void createXmlAndSave(DatVO datVO, string fileFullpath, bool useMachineTags) 
        {

            string gameElementName = useMachineTags ? "machine" : "game";

            if (datVO == null) 
            {
                Debug.WriteLine(this + "ERROR: DatVO is NULL");
                return;
            }
            
            else if (fileFullpath == null) {
                Debug.WriteLine(this + "ERROR: fileFullPath is NULL");
                return;
            }
            

            XmlDocument doc = new XmlDocument();
            // Set the XmlResolver property to null to prevent the docType below from throwing exceptions
            doc.XmlResolver = null;

            doc.CreateXmlDeclaration("1.0", null, null);
            string dtdLink = "http://www.logiqx.com/Dats/datafile.dtd";
            string dtdDef = "-//Logiqx//DTD ROM Management Datafile//EN";

            // <!DOCTYPE datafile PUBLIC "-//Logiqx//DTD ROM Management Datafile//EN" "http://www.logiqx.com/Dats/datafile.dtd">

            XmlDocumentType type = doc.CreateDocumentType(
                "datafile"
                , dtdLink
                , dtdDef
                , null
            );
            doc.AppendChild(type);

            XmlElement root = (XmlElement)doc.AppendChild(doc.CreateElement("datafile"));

            XmlElement header = (XmlElement)root.AppendChild(doc.CreateElement("header"));
            
            addElementAndText(header, "name", datVO.DatHeaderVO.Name, doc);
            addElementAndText(header, "description", datVO.DatHeaderVO.Description, doc);
            addElementAndText(header, "category", datVO.DatHeaderVO.Category, doc);
            addElementAndText(header, "version", datVO.DatHeaderVO.Version, doc);

            addElementAndText(header, "author", datVO.DatHeaderVO.Author, doc);
            addElementAndText(header, "comment", datVO.DatHeaderVO.Comment, doc);

            DatRomVO romVO;

            XmlElement lastRom;
            XmlElement lastGame;

            foreach (var gameVO in datVO.Entries)
            {
                lastGame = addElement(root, gameElementName, doc);
                addAtt(lastGame, "name", gameVO.Name, doc);

                if (getNotEmpty(gameVO.Description))
                    addElementAndText(lastGame, "description", gameVO.Description, doc);

                for (int j = 0; j < gameVO.Roms.Length; j++ ) {
                    romVO = gameVO.Roms[j];
                    lastRom = addElement(lastGame, "rom", doc);

                    if (getNotEmpty(romVO.Name))
                        addAtt(lastRom, "name", romVO.Name, doc);

                    
                    addAtt(lastRom, "size", romVO.Size.ToString(), doc);

                    if (getNotEmpty(romVO.Crc))
                        addAtt(lastRom, "crc", romVO.Crc, doc);
                    
                    if (getNotEmpty(romVO.Md5))
                        addAtt(lastRom, "md5", romVO.Md5, doc);

                    if (getNotEmpty(romVO.Sha1))
                        addAtt(lastRom, "sha1", romVO.Sha1, doc);
                }

            }

            StreamWriter outStream = System.IO.File.CreateText(
                fileFullpath
            );

            doc.Save(outStream);
            outStream.Close();

            base.ExecuteCommand(
                    new ShowMessageCmd("The DAT file has been created.\r" + fileFullpath, ShowMessageCmd.MessageTitleEnum.Ok
                    ));
        }

        bool getNotEmpty(string str) 
        {
            if (str == null)
                return false;
            if (str == "")
                return false;
            return true;
        }

        void addAtt(XmlNode element, string name, string value, XmlDocument doc) 
        {
            (element as XmlElement).SetAttribute(name, value);
        }

        XmlElement addElementAndText(XmlNode parent, string name, string value, XmlDocument doc) 
        {
            XmlElement el = addElement(parent, name, doc);
            el.InnerText = value;
            return el;
        }

        XmlElement addElement(XmlNode parent, string name, XmlDocument doc) 
        {
            return (XmlElement)parent.AppendChild(doc.CreateElement(name));
        }
    }
}
