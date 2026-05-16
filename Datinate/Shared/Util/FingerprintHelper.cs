using RadioLibCore.RadioDat;
using System.Globalization;
using System.Text.RegularExpressions;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class FingerprintHelper
    {
        public static IReadOnlyCollection<string> GetPartFingerprints(IGameFamily family)
        {
            var fingerprints = new HashSet<string>();

            foreach (var game in family.GetAllGames())
            {
                var partFingerprints = GetPartFingerprints(game);
                foreach (var partFingerprint in partFingerprints)
                {
                    _ = fingerprints.Add(partFingerprint);
                }
            }

            return fingerprints;
        }
        public static IReadOnlyCollection<string> GetPartFingerprints(IGame game)
        {
            var fingerprints = new HashSet<string>();

            foreach (var part in game.GetGameParts(false))
            {
                _ = fingerprints.Add(part.Fingerprint);
            }

            return fingerprints;
        }


        public static string GetFingerprint(IGamePart gamePart)
        {
            return SortFingerprintElementsToString(
                gamePart.GetChecksums()
            );

        }

        public static IReadOnlyCollection<string> GetFingerprintElements(string fingerprint)
        {
            if (fingerprint.Length == 0)
                return Array.Empty<string>();

            var parts = fingerprint.Split(',');

            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            return parts;
        }

        public static string GetFingerprint(string romChecksum)
        {
            return SortFingerprintElements(new[] { romChecksum })[0];
        }
        public static string GetFingerprint(string[] romChecksums)
        {
            return SortFingerprintElementsToString(romChecksums);
        }

        /**
         * SHA1 can be null!
         * MAME_SL -> snes example:
         * 

	<software name="clayfgt2up" cloneof="clayfgt2" supported="no"> <!-- incomplete dump -->
		<!-- Notes: incomplete dump, only 3 out of the 6 chips are available, the original prototype board is unknown -->
		<description>Clay Fighter 2 - Judgment Clay (USA, prototype)</description>
		<year>1995</year>
		<publisher>Interplay</publisher>
		<part name="cart" interface="snes_cart">
			<feature name="slot" value="hirom" />
			<dataarea name="rom" size="3145728">
				<rom name="clay 2 1995.u1" size="524288" crc="3998e4b9" sha1="a0b91861b4e67c290d6621f607560366d8ef7ec4"                 offset="0x000000" />
				<rom name="2.u2"           size="524288" crc="19c4b39f" sha1="86df3dd37504dba24a84eac5bbf50ba26c84b194"                 offset="0x080000" />
				<rom name="3.u3"           size="524288"                                                                status="nodump" offset="0x100000" />
				<rom name="4.u4"           size="524288" crc="dc5e95b5" sha1="b695e6c1f25381d159cb2b3efa8b3a44bed85a86"                 offset="0x180000" />
				<rom name="5.u4"           size="524288"                                                                status="nodump" offset="0x200000" />
				<rom name="6.u6"           size="524288"                                                                status="nodump" offset="0x280000" />
			</dataarea>
		</part>
	</software>

         */
        internal static string[] SortFingerprintElements(string[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] == null) elements[i] = string.Empty;
            }
            elements = elements.Select(s => s.ToLowerInvariant()).ToArray();

            // NOTE: Do not ignore case
            return elements.OrderBy(name => name, StringComparer.Create(new CultureInfo("en-US"), false)).ToArray();

        }
        public static string SortFingerprintElementsToString(string[] elements)
        {
            elements = SortFingerprintElements(elements);
            return String.Join(",", elements);
        }

        public static bool IsValidSha1(string sha1)
        {
            Regex r = new Regex(@"^[a-fA-F0-9]{40}$");
            return r.IsMatch(sha1);
        }
    }
}
