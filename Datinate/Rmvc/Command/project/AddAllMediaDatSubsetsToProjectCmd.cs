using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC 
{
    public class AddAllMediaDatSubsetsToProjectCmd : RCommand
    {
        private readonly DatVO dat;
        private readonly DAT_GROUP_ENUM datGroupEnum;

        public AddAllMediaDatSubsetsToProjectCmd(DatVO dat, DAT_GROUP_ENUM datGroupEnum)
        {
            this.dat = dat;
            this.datGroupEnum = datGroupEnum;
        }

        protected override void Run()
        {
            if (datGroupEnum != DAT_GROUP_ENUM.R2DAT_EMUMOVIES)
            {
                Debug.WriteLine(this +" Only EmuMovies supports this command at this time.");
                return;
            }
            var games = dat.Entries;

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

            foreach (var game in games)
            {
                if (!dic.ContainsKey(game.Name))
                {
                    dic.Add(game.Name, new List<string>());
                }

                for (int j = 0; j < game.Roms.Length; j++)
                {
                    var datRomVO = game.Roms[j];

                    if (!dic[game.Name].Contains(datRomVO.Name))
                        dic[game.Name].Add(datRomVO.Name);
                }
            }

            KeyValuePair<string, List<string>>[] pairs = dic.ToArray();

            foreach (var kvp in pairs)
            {
                var entryName = kvp.Key;

                if (kvp.Value.Count == 0)
                    dic.Remove(entryName);

                else
                {
                    var enumMatch = EmuMoviesHelper.GetMediaTypeFromEntryName(entryName);

                    if (enumMatch == null) continue;

                    var subset = new DatSubsetFilter(entryName);
                    var cmd = new AddDatToProjectCmd(
                        dat,
                        COLLECTION_SET_ENUM.Media,
                        DatinateEnums.DAT_GROUP_TARGET_ENUM.MEDIA_INCLUDE_GROUP,
                        DAT_GROUP_ENUM.R2DAT_EMUMOVIES,
                        enumMatch.ToString(),
                        subset);

                    base.ExecuteCommand(cmd);

                    // NOTE: This command will already have show the user an error message:
                    if (cmd.AddToProjectSucceeded == false)
                        break;
                }
            }
        }
    }
}
