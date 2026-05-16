using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class FetchDatSubsetInfoCmd : RCommand 
    {
        private readonly DatVO datVO;

        public FetchDatSubsetInfoCmd(DatVO datVO) 
        {
            this.datVO = datVO;
        }

        protected override void Run() 
        {
            var games = datVO.Entries;

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

            foreach (var game in games)
            {
                if (!dic.ContainsKey(game.Name)) 
                {
                    dic.Add(game.Name, new List<string>() { "[None]"});
                }

                for (int j = 0; j < game.Roms.Length; j++) 
                {
                    var datRomVO = game.Roms[j];
                    var split = datRomVO.Name.Split(new char[] { '\\' }, StringSplitOptions.None);
                    
                    if (split.Length < 2) continue;
                    
                    else 
                    {
                        if (!dic[game.Name].Contains(split[0]))
                            dic[game.Name].Add(split[0]);
                    }
                }
            }

            KeyValuePair<string, List<string>>[] pairs = dic.ToArray();
            
            for (int i = 0; i < pairs.Length; i++) 
            {
                if (pairs[i].Value.Count == 0)
                    dic.Remove(pairs[i].Key);
            }

            Facade.Instance?.AddToProjectMediator?.SetProjectUiDatSubsetInfo(dic);
        }
    }
}
