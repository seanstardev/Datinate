using com.RADIO.Datinate.RMVC;
using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
namespace Datinate.Tests
{
    public class AutoGroup_PClone_Tests
    {
        [Fact]
        public void TestVB()
        {
            var autoGroupModel = new AutoGrouperModel();

            var datFullpath = TestDataHelper.GetMameSLDat("vboy");

            var dat = LoadDatTests.GetMameSLDat(datFullpath);

            Assert.NotNull(dat);

            var adv = new DatAdvanced(
                dat,
                Guid.NewGuid().ToString(),
                DAT_GROUP_ENUM.MAME_SL,
                Guid.NewGuid().ToString(),
                Array.Empty<DatFilter>(), null
                );

            var result = autoGroupModel.Build(
                new AutoGrouperOptions(),
                new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>(),
                new DatAdvanced[] { adv }, 
                Array.Empty<DatAdvanced>());

            var totalGames = 0;
            var totalRoms = 0;

            foreach (var entry in result)
            {
                totalGames += entry.GetTotalIncludedParts();
                totalRoms += entry.GetAllGameParts(false).Select(p => p.GetChecksums().Length).Count();
            }
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestNes()
        {
            var autoGroupModel = new AutoGrouperModel();

            var datFullpath = TestDataHelper.GetMameSLDat("nes");

            var dat = LoadDatTests.GetMameSLDat(datFullpath);

            Assert.NotNull(dat);

            var adv = new DatAdvanced(
                dat,
                Guid.NewGuid().ToString(),
                DAT_GROUP_ENUM.MAME_SL,
                Guid.NewGuid().ToString(),
                Array.Empty<DatFilter>(),
                
                null);

            var result = autoGroupModel.Build(
                new AutoGrouperOptions(),
                new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>(),
                new DatAdvanced[] { adv }, 
                Array.Empty<DatAdvanced>());

            // 14 nodumps

            var totalGames = 0;
            var totalRoms = 0;
            var totalAliases = 0;
            foreach (var entry in result)
            {
                totalGames += entry.GetTotalIncludedParts();
                totalRoms += entry.GetAllGameParts(false).Select(p => p.GetChecksums().Length).Count();

                foreach (var item in entry.GetAllGameParts(false))
                {
                    totalAliases += item.GetSoftwareAliases().Length;
                    foreach (var a in item.GetSoftwareAliases())
                    {
                        Debug.WriteLine(a.GetChecksums()[0] + " --- "+a.GetName());
                    }
                }
            }

            // One identical GAME (1 part: 2 roms):
                // 1e5eec5beb976f79373e589186382c337ed6e84a --- gauntleta


            // 4551

            // 1017 'loadflag="'            

            // 1870 'cloneof="'
            // 4556 '</software>' in dat

            // 14 nodumps

            // 4552 zips in nes folder

            Assert.NotEmpty(result);

        }
    }
}
/**
<software name="gauntlet">
		<description>Gauntlet (USA)</description>
		<year>1988</year>
		<publisher>Tengen</publisher>
		<info name="serial" value="NES-GL-USA"/>
		<part name="cart" interface="nes_cart">
			<feature name="slot" value="namcot_3433" />
			<feature name="pcb" value="NES-DRROM" />
			<feature name="mirroring" value="4screen" />
			<dataarea name="prg" size="131072">
				<rom name="nes-gl-0 prg" size="131072" crc="834d1924" sha1="e1d8553a0deaf3bb17c9ea798ec52f2723db3aea" offset="00000" />
			</dataarea>
			<dataarea name="chr" size="65536">
				<rom name="nes-gl-0 chr" size="65536" crc="26d819a2" sha1="1e5eec5beb976f79373e589186382c337ed6e84a" offset="00000" />
			</dataarea>
			<!-- 2k VRAM on cartridge -->
			<dataarea name="vram" size="2048" />
		</part>
	</software>

	<software name="gauntleta" cloneof="gauntlet">
		<description>Gauntlet (USA, alt PCB)</description>
		<year>1988</year>
		<publisher>Tengen</publisher>
		<info name="serial" value="NES-GL-USA"/>
		<part name="cart" interface="nes_cart">
			<feature name="slot" value="txrom" />
			<feature name="pcb" value="NES-TR1ROM" />
			<feature name="mirroring" value="4screen" />
			<dataarea name="prg" size="131072">
				<rom name="nes-gl-0 prg" size="131072" crc="834d1924" sha1="e1d8553a0deaf3bb17c9ea798ec52f2723db3aea" offset="00000" />
			</dataarea>
			<dataarea name="chr" size="65536">
				<rom name="nes-gl-0 chr" size="65536" crc="26d819a2" sha1="1e5eec5beb976f79373e589186382c337ed6e84a" offset="00000" />
			</dataarea>
			<!-- 8k VRAM on cartridge -->
			<dataarea name="vram" size="8192" />
		</part>
	</software>
*/
