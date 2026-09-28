using com.RADIO.Datinate.RMVC.Shared;

namespace datinate.shared
{
    public class Flag 
    {
        private readonly string name;

        private List<string> affectedFiles = new List<string>();
        private List<DatGameVO> affectedGames = new List<DatGameVO>();

        public Flag(string name, DatGameVO[] games) 
        {
            this.name = name;
            affectedFiles = games.Select(g => g.Name).ToList();
            affectedGames.AddRange(games);
        }

        public Flag(string name, List<DatGameVO> games)
        {
            this.name = name;
            for (int i = 0; i < games.Count; i++) 
            {
                affectedFiles.Add(games[i].Name);
            }
            affectedGames.AddRange(games);
        }

        public Flag(string name, string newFile)  
        {
            this.name = name;
            affectedFiles.Add(newFile);
        }

        public void MatchFound(string newFile) 
        {
            for (int i = 0; i < affectedFiles.Count; i++) 
            {
                if (newFile == affectedFiles[i]) 
                    return;
            } 
            affectedFiles.Add(newFile);
        }

        public string GetName() 
        {
            return name;
        }

        public int GetCount() 
        {
            return affectedFiles.Count;
        }

        public string GetFileAt(int i) 
        {
            return affectedFiles[i];
        }
        public DatGameVO GetGameAt(int i)
        {
            return affectedGames[i];
        }
    }
}
