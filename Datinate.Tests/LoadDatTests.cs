using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Proxy.delegates;

namespace Datinate.Tests
{
    /*
    - Exact game count 
    - Exact part count 
    - Exact rom count 
    - Exact byte count
    */
    /// </summary>
    public class LoadDatTests
    {
        [Fact]
        public void TestMameSL_nes()
        {
            var dat = GetMameSLDat(@"DATs/MAME-SL/nes.xml");
            Assert.NotNull(dat);

            var games = dat!.Entries;
            var roms = dat!.Entries.SelectMany(g => g.Roms);
            var totalSize = roms.Aggregate(0UL, (sum, r) => sum + r.Size);

            // 7989     rom     '<rom name=' -
            // 1017     rom     'loadflag="'
            // 14       rom     'status="nodump"'

            Assert.Equal(4552, games.Count);       // # of zips in nes roms folder
            Assert.Equal(7972, roms.Count());       // # of files when extracted
            Assert.Equal(2249853236UL, totalSize);  // total bytes when extracted

            Assert.False(dat.AlwaysOneRomPerGame);
            Assert.False(dat.ContainsChds);
        }

        [Fact]
        public void TestMameSL_vb()
        {
            var dat = GetMameSLDat(@"DATs/MAME-SL/vboy.xml");
            Assert.NotNull(dat);

            var games = dat!.Entries;
            var roms = dat!.Entries.SelectMany(g => g.Roms);
            var size = roms.Aggregate(0UL, (sum, r) => sum + r.Size);

            Assert.Equal(32, games.Count);

            Assert.Equal(40894464UL, size);
            Assert.Equal(games.Count, roms.Count());

            Assert.False(dat.ContainsChds);
            Assert.True(dat.AlwaysOneRomPerGame);
        }


        [Fact]
        public void TestMameSL_psx()
        {
            var dat = GetMameSLDat(@"DATs/MAME-SL/psx.xml");
            Assert.NotNull(dat);

            var games = dat!.Entries;
            var roms = dat!.Entries.SelectMany(g => g.Roms);
            var size = roms.Aggregate(0UL, (sum, r) => sum + r.Size);

            Assert.Equal(3120, games.Count);

            Assert.Equal(0UL, size); // will end up being 819,560,447,998
            Assert.Equal(games.Count, roms.Count());

            Assert.True(dat.ContainsChds);
            Assert.True(dat.AlwaysOneRomPerGame);
        }

        public static DatVO? GetMameSLDat(string datFullpath)
        {
            var path = TestDataHelper.GetTestDataPath(datFullpath);
            var text = File.ReadAllText(path);

            var dat = DatSoftHelper.GetDat(path, text);
            return dat;
        }
    }
}
