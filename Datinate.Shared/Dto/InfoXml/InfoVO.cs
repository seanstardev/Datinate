using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace RadioLibCore.RadioResource 
{
    public class InfoVO 
    {
        public string InfoXmlVersion;

        // mame only - gameinit.dat
        public string StartupText = "";
        public string Lookup = "";

        // TODO: Note that MAME sets players on a 'per-release' basis... In that case I will set to parent game's players.
        public string Players = "";

        public string Name = "";
        public DescriptionVO DescriptionVO = DescriptionVO.EMPTY;
        public EmulationVO EmulationVO = EmulationVO.EMPTY;

        public string Developer = "";

        public ReleaseVO[] ReleaseVOs           = new ReleaseVO[] { };
        public CreditVO[] CreditVOs             = new CreditVO[] { };
        public MiscPropertyVO[] MiscPropertyVOs = new MiscPropertyVO[] { };
        public CompilationVO[] CompilationVOs   = new CompilationVO[] { };

        public string [] AlsoKnownAs    = new string[] { };
        public string [] AlsoOn         = new string[] { };

        public string Genre = "";

        public string[] Contributors    = new string[] { };

        public string SystemLookup = "";

        public string ResourceEnum = "";

        public InfoVO(
            string infoXmlVersion
            , string lookup
            , string name
            , string resourceEnum
            , DescriptionVO descriptionVO
            , string developer
            , string players
            , ReleaseVO[] releaseVOs
            , string genre
            , CreditVO[] creditVOs
            , string [] alsoKnownAs
            , string [] alsoOn
            , MiscPropertyVO[] miscPropertyVOs
            , string startupText
            , string[] contributors
            , CompilationVO[] compilationVOs
            , EmulationVO emulationVO
        ) {
            this.InfoXmlVersion = infoXmlVersion;
            Lookup          = lookup;
            Name            = name;
            DescriptionVO   = descriptionVO;
            Developer       = developer;
            Players         = players;

            ReleaseVOs      = releaseVOs;
            Genre           = genre;
            CreditVOs       = creditVOs;

            AlsoKnownAs     = alsoKnownAs;
            AlsoOn          = alsoOn;
            MiscPropertyVOs = miscPropertyVOs;
            StartupText     = startupText;
            Contributors    = contributors;

            CompilationVOs  = compilationVOs;
            EmulationVO = emulationVO;

            ResourceEnum = resourceEnum;
        }

        public InfoVO(string lookup) {
            this.Lookup = lookup;
        }
        public void AddMiscPropertyVO(MiscPropertyVO miscPropertyVO) {
            List<MiscPropertyVO> list = MiscPropertyVOs.ToList();
            list.Add(miscPropertyVO);
            MiscPropertyVOs = list.ToArray();
        }

        public void AddMiscPropertyVOs(MiscPropertyVO[] miscPropertyVOs) {
            List<MiscPropertyVO> list = MiscPropertyVOs.ToList();
            list.AddRange(miscPropertyVOs);
            MiscPropertyVOs = list.ToArray();
        }

        public void AddCreditVO(CreditVO creditVO) {
            List<CreditVO> list = CreditVOs.ToList();
            list.Add(creditVO);
            CreditVOs = list.ToArray();
        }

        public void AddCreditVOs(CreditVO[] creditVOs) {
            List<CreditVO> list = CreditVOs.ToList();
            list.AddRange(creditVOs);
            CreditVOs = list.ToArray();
        }

        public void AddRelease(ReleaseVO releaseVO) {
            List<ReleaseVO> list = ReleaseVOs.ToList();
            list.Add(releaseVO);
            ReleaseVOs = list.ToArray();
        }
    }
}
