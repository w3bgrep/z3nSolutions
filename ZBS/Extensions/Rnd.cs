using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace ZBSolutions
{
    public class Rnd
    {


        public string Seed()
        {
            return Blockchain.GenerateMnemonic("English", 12);
        }
        public string RandomHex(int length)
        {
            const string chars = "0123456789abcdef";
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return "0x" + new string(result);
        }
        public string RandomHash(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string Nickname()
        {
            string[] adjectives = {
                "Sunny", "Mystic", "Wild", "Cosmic", "Shadow", "Lunar", "Blaze", "Dream", "Star", "Vivid",
                "Frost", "Neon", "Gloomy", "Swift", "Silent", "Fierce", "Radiant", "Dusk", "Nova", "Spark",
                "Crimson", "Azure", "Golden", "Midnight", "Velvet", "Stormy", "Echo", "Vortex", "Phantom", "Bright",
                "Chill", "Rogue", "Daring", "Lush", "Savage", "Twilight", "Crystal", "Zesty", "Bold", "Hazy",
                "Vibrant", "Gleam", "Frosty", "Wicked", "Serene", "Bliss", "Rusty", "Hollow", "Sleek", "Pale"
            };

            string[] nouns = {
                "Wolf", "Viper", "Falcon", "Spark", "Catcher", "Rider", "Echo", "Flame", "Voyage", "Knight",
                "Raven", "Hawk", "Storm", "Tide", "Drift", "Shade", "Quest", "Blaze", "Wraith", "Comet",
                "Lion", "Phantom", "Star", "Cobra", "Dawn", "Arrow", "Ghost", "Sky", "Vortex", "Wave",
                "Tiger", "Ninja", "Dreamer", "Seeker", "Glider", "Rebel", "Spirit", "Hunter", "Flash", "Beacon",
                "Jaguar", "Drake", "Scout", "Path", "Glow", "Riser", "Shadow", "Bolt", "Zephyr", "Forge"
            };

            string[] suffixes = { "", "", "", "", "", "X", "Z", "Vibe", "Glow", "Rush", "Peak", "Core", "Wave", "Zap" };

            Random random = new Random(Guid.NewGuid().GetHashCode());

            string adjective = adjectives[random.Next(adjectives.Length)];
            string noun = nouns[random.Next(nouns.Length)];
            string suffix = suffixes[random.Next(suffixes.Length)];

            string nickname = $"{adjective}{noun}{suffix}";

            if (nickname.Length > 15)
            {
                nickname = nickname.Substring(0, 15);
            }

            return nickname;
        }
        public int Int(IZennoPosterProjectModel project, string Var)
        {
            string value = string.Empty;
            try
            {
                value = project.Variables[Var].Value;
            }
            catch (Exception e)
            {
                project.SendInfoToLog(e.Message);
            }
            if (value == string.Empty) project.L0g($"no Value from [{Var}] `w");

            if (value.Contains("-"))
            {
                var min = int.Parse(value.Split('-')[0].Trim());
                var max = int.Parse(value.Split('-')[1].Trim());
                return new Random().Next(min, max);
            }
            return int.Parse(value.Trim());
        }




    }
}
