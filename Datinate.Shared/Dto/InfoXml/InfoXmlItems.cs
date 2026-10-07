namespace RadioLibCore.RadioResource
{
    public class ReleaseVO
    {
        public string Name { get; set; } = "";
        public string Region { get; set; } = "";
        public string Publisher { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string DistributionOrBarcode { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public string Rating { get; set; } = "";
        public string Players { get; set; } = "";


        public string Distributor { get; set; } = "";
        public string Comment { get; set; } = "";
        public string Medium { get; set; } = "";
        public string Type { get; set; } = "";

        public InputVO[] InputVOs { get; set; } = new InputVO[] { };
        public AttributePairVO[] EmulationStatusVOs { get; set; } = new AttributePairVO[] { };

        // MAME only:
        public string? MameName = null;

        public string? AssetKey = null;

        internal ReleaseVO(
            string name
            , string region
            , string publisher
            , string productId
            , string distributionOrBarcode
            , string releaseDate
            , string rating
            , string players
            , InputVO[] inputVOs
            , AttributePairVO[] emulationStatusVOs
            , string assetKey
            , string? mameName = null)
        {
            Name = name;
            Region = region;
            Publisher = publisher;
            ProductId = productId;
            DistributionOrBarcode = distributionOrBarcode;
            ReleaseDate = releaseDate;
            Rating = rating;
            Players = players; this.InputVOs = inputVOs;
            EmulationStatusVOs = emulationStatusVOs;
            AssetKey = assetKey;
            MameName = mameName;
        }

        public ReleaseVO(string name) =>
            Name = name;

        override public string ToString()
        {
            string str = "";

            str += "  name: " + Name + "\n";
            str += "  region: " + Region + "\n";
            str += "  publisher: " + Publisher + "\n";
            str += "  productId: " + ProductId + "\n";
            str += "  distribution: " + DistributionOrBarcode + "\n";
            str += "  releaseDate: " + ReleaseDate + "\n";
            str += "  rating: " + Rating + "\n";
            return str;
        }
    }
    public class AttributePairVO
    {
        public string name { get; set; }
        public string value { get; set; }

        public AttributePairVO(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        override public string ToString()
        {
            return name + ": " + value;
        }
    }    
    public class CompilationVO
    {
        public string Name { get; set; } = "";
        public string UrlPart { get; set; } = "";
        public string System { get; set; } = "";

        public CompilationVO(
            string name, string system, string urlPart)
        {
            Name = name;
            System = system;
            UrlPart = urlPart;
        }
    }
    public class CreditVO
    {
        public string Category { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string UrlPart { get; set; }

        public CreditVO(string role, string name, string category = "", string urlPart = "")
        {
            Role = role;
            Name = name;
            Category = category;
            UrlPart = urlPart;
        }

        override public string ToString()
        {
            return ">" + Category + Environment.NewLine + "  >" + Role + Environment.NewLine + "    >" + Name;
        }
    }
    public class DescriptionVO
    {
        public string Description { get; set; }
        public TEXT_FORMAT_ENUM TextFormatEnum { get; set; }
        public bool IsBoilerplate { get; set; }
        public static DescriptionVO EMPTY { get { return new DescriptionVO("", TEXT_FORMAT_ENUM.text, false); } }

        public enum TEXT_FORMAT_ENUM
        {
            text
            , html
            , rich_text
        }
        public DescriptionVO(
            string description,
            TEXT_FORMAT_ENUM textFormatEnum,
            bool isBoilerplate)
        {
            Description = description;
            TextFormatEnum = textFormatEnum;
            IsBoilerplate = isBoilerplate;
        }
    }
    public class EmulationVO
    {
        public string History { get; }
        public static EmulationVO EMPTY { get { return new EmulationVO(""); } }

        public EmulationVO(string history)
        {
            History = history;
        }
    }
    public class InputVO
    {
        public AttributePairVO[] attributePairVOs { get; set; }

        public InputVO(AttributePairVO[] attributePairVOs)
        {
            this.attributePairVOs = attributePairVOs;
        }
    }
    public class MiscPropertyVO
    {
        public string name { get; set; }
        public string value { get; set; }

        public MiscPropertyVO(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        override public string ToString()
        {
            return "Misc Property: " + name + " --- " + value;
        }
    }
    public class SystemVO
    {
        public string htmlDescription { get; set; } = "";
        public string manufacturer { get; set; } = "";
        public string generation { get; set; } = "";
        public string type { get; set; } = "";

        public string developer { get; set; } = "";
        public string retailAvailability { get; set; } = "";
        public string unitsSold { get; set; } = "";
        public string media { get; set; } = "";

        public SystemVO(string htmlDescription)
        {
            this.htmlDescription = htmlDescription;
        }
    }
}