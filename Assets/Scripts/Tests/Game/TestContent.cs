using System.Linq;
using LastTrainHome.Game.Content;
using NUnit.Framework;
using UnityEditor;

namespace LastTrainHome.Game.Tests
{
    /// <summary>Content for tests: a small valid level to break on purpose, and the game's own content files.</summary>
    internal static class TestContent
    {
        public const string CatalogPath = "Assets/Data/LevelCatalog.asset";

        /// <summary>Central with Harbour and Market off it, 10 minutes each. Agnes needs Harbour by 00:10, Dev Market by 00:30.</summary>
        public const string Level = @"{
            ""title"": ""Monday"", ""startStation"": ""Central"", ""departure"": ""00:00"", ""closing"": ""01:00"",
            ""stations"": [ { ""id"": ""Central"", ""x"": 1, ""y"": 2 }, { ""id"": ""Harbour"", ""displayName"": ""The Harbour"", ""label"": ""left"" },
                            { ""id"": ""Market"", ""x"": 3 }, { ""id"": ""Depot"", ""closed"": true } ],
            ""tracks"": [ { ""from"": ""Central"", ""to"": ""Harbour"", ""minutes"": 10 }, { ""from"": ""Central"", ""to"": ""Market"", ""minutes"": 10 },
                          { ""from"": ""Market"", ""to"": ""Depot"", ""minutes"": 15, ""closed"": true } ],
            ""rules"": { ""asks"": 1 },
            ""nightCard"": { ""kicker"": ""Night {night} of {nights}"", ""featured"": ""Agnes"", ""featuredLine"": ""Her night."", ""lines"": [ ""Someone waits."" ] },
            ""platform"": { ""kicker"": ""{day}"", ""headline"": ""Meet them."", ""coach"": ""Ask one."" },
            ""passengers"": [
                { ""name"": ""Agnes"", ""destination"": ""Harbour"", ""deadline"": ""00:10"", ""platform"": ""The ferry."", ""ask"": ""My grandson."",
                  ""revealsBigNight"": true,
                  ""onTime"": { ""line"": ""Made the ferry."", ""keepsake"": ""ticket"", ""weekNote"": ""Waved him off."" },
                  ""late"": { ""line"": ""Missed it."", ""logLine"": ""Missed."", ""carry"": ""She posts it tomorrow."" },
                  ""notReached"": { ""line"": ""Taxi."" } },
                { ""name"": ""Dev"", ""destination"": ""Market"", ""deadline"": ""00:30"", ""platform"": ""Exam."", ""planQuote"": ""Exam at nine."", ""ask"": ""Dates."",
                  ""onTime"": { ""line"": ""Asleep."" }, ""late"": { ""line"": ""Awake."" }, ""notReached"": { ""line"": ""Sofa."" } } ],
            ""variants"": [ { ""if"": { ""night"": ""Night0"", ""passenger"": ""Dev"", ""status"": ""missed"" },
                              ""passenger"": ""Dev"", ""deadline"": ""00:20"", ""platform"": ""Told to come early."",
                              ""onTime"": { ""line"": ""Early, for once."" } } ],
            ""coach"": [ { ""when"": ""empty"", ""text"": ""Click a station."" },
                         { ""when"": ""anyLate"", ""text"": ""{late} would be late."" },
                         { ""when"": ""deadEnd"", ""station"": ""Harbour"", ""text"": ""Dead end."" },
                         { ""when"": ""always"", ""text"": ""Still waiting: {waiting}. {nextDeadline} is next."" } ],
            ""logbook"": { ""tomorrow"": ""Tomorrow is tight."" }
        }";

        public static LevelCatalog Catalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            Assert.That(catalog, Is.Not.Null, $"No level catalog at {CatalogPath}.");
            return catalog;
        }

        public static GameContent Game() => Catalog().LoadContent();

        public static Level GameLevel(string nightId) => Game().Levels.Single(level => level.Night.Id == nightId);
    }
}
