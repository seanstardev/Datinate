using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using RMVC;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using static datinate.shared.DatFilterHelper;

namespace com.RADIO.Datinate.RMVC
{
    public class ExpressionsProxy : RModel 
    {

        string? lastPath;

        protected override void Initialise()
        {

        }
        public void SetProjectRootPath(string projectRoot)
        {
            lastPath = Path.Combine(projectRoot, "Expression");
            Directory.CreateDirectory(lastPath);
        }

        public void ShowLoadExpressionsDialog(EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum) 
        {    
            OpenFileDialog d = new OpenFileDialog();
            
            d.Filter = "XML Files (*.xml)|*.xml";
            d.Multiselect = false;
            d.Title = "Select Expressions XML";
            d.InitialDirectory = lastPath;

            DialogResult dr = d.ShowDialog();

            if (dr == DialogResult.OK) 
            {    
                DatFilter[] expressions = FetchExpressions(d.FileName);
                
                lastPath = Path.GetDirectoryName(d.FileName);

                if (expressionsFileTargetEnum == EXPRESSIONS_FILE_TARGET_ENUM.CUSTOM_LIST) 
                {
                    base.ExecuteCommand(new ApplyExpressionsFileCmd(expressions));
                }
                else 
                {
                    base.ExecuteCommand(new SetProjectLoaderDatExpressionsCmd(d.FileName));
                }
            }
        }

        public void SaveExpressions(DatFilter[] expressions) 
        {
            XmlDocument doc = new XmlDocument();
            XmlElement rootEl = doc.AppendChild(doc.CreateElement("root")) as XmlElement;
            XmlElement expressionsEl = rootEl.AppendChild(doc.CreateElement("expressions")) as XmlElement;

            XmlElement el;
            DatFilter exp;

            string expStr;

            for (int i = 0; i < expressions.Length; i++) 
            {
                exp = expressions[i];
                el = expressionsEl.AppendChild(doc.CreateElement("expression")) as XmlElement;
                el.SetAttribute("action", exp.GetExpressionAction().ToString());

                XmlCDataSection cData;

                expStr = "";
                expStr += Environment.NewLine;
                expStr += exp.GetUserFriendlyExpression();
                expStr += Environment.NewLine;
                cData = doc.CreateCDataSection(expStr);
                el.AppendChild(cData);

            }

            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "XML Files (*.xml)|*.xml";

            d.Title = "Select Expressions XML";
            d.InitialDirectory = lastPath;

            DialogResult dr = d.ShowDialog();

            if (dr == DialogResult.OK) 
            {
                doc.Save(d.FileName);
                MessageBox.Show(
                    "Expressions File Saved", 
                    "OK", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
        }

        public DatFilter[] FetchExpressions(string xmlFullpath) 
        {
            if (string.IsNullOrWhiteSpace(xmlFullpath))
                return Array.Empty<DatFilter>();

            List<DatFilter> list = new List<DatFilter>();

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFullpath);

            XmlNodeList nodeList = doc.SelectNodes("root/expressions/expression");

            EXPRESSION_ACTION_ENUM actionEnum;


            for (int i = 0; i < nodeList.Count; i++) 
            {
                var el = nodeList[i] as XmlElement;
                actionEnum = DatinateHelper.GetEnumFromString<EXPRESSION_ACTION_ENUM>(el.GetAttribute("action"));

                var expression = el.InnerText;
                expression = expression.Replace("\r", "");
                expression = expression.Replace("\n", "");
                expression = expression.Replace("\t", "");

                var e = new DatFilter(
                    expression
                    , actionEnum);
                
                list.Add(e);
            }

            return list.ToArray();
        }
    }
}
