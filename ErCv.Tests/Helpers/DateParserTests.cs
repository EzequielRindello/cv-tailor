using ErCvApi.Helpers;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class DateParserTests
    {
        private static int Index(string value, bool isEnd = false)
        {
            Assert.True(DateParser.TryGetMonthIndex(value, isEnd, out var index), $"could not parse '{value}'");
            return index;
        }

        [Theory]
        [InlineData("Jun 2024", 2024, 6)]
        [InlineData("June 2024", 2024, 6)]
        [InlineData("jun 2024", 2024, 6)]
        [InlineData("Sept 2023", 2023, 9)]
        [InlineData("Dec. 2021", 2021, 12)]
        [InlineData("2024-06", 2024, 6)]
        [InlineData("2024-6", 2024, 6)]
        [InlineData("06/2024", 2024, 6)]
        [InlineData("6/2024", 2024, 6)]
        [InlineData("  May 2020  ", 2020, 5)]
        public void Parses_year_and_month_formats(string value, int year, int month)
        {
            Assert.True(DateParser.TryParse(value, out var y, out var m));
            Assert.Equal(year, y);
            Assert.Equal(month, m);
        }

        [Theory]
        [InlineData("2019")]
        [InlineData("Spring 2019")]
        [InlineData("2016-03-15")]
        [InlineData("13/2020")]   // invalid month: the year is still usable
        public void Year_only_or_loose_text_yields_the_year_without_a_month(string value)
        {
            Assert.True(DateParser.TryParse(value, out var year, out var month));
            Assert.InRange(year, 2016, 2020);
            Assert.Null(month);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("someday")]
        [InlineData("03/16")]
        [InlineData("1800")]
        [InlineData("2999")]
        public void Unreadable_or_implausible_values_do_not_parse(string? value)
        {
            Assert.False(DateParser.TryParse(value, out _, out _));
        }

        [Theory]
        [InlineData("Present")]
        [InlineData("current")]
        [InlineData(" NOW ")]
        public void Present_synonyms_are_recognized_and_map_to_the_current_month(string value)
        {
            Assert.True(DateParser.IsPresent(value));
            Assert.Equal(DateTime.Now.Year * 12 + DateTime.Now.Month - 1, Index(value));
        }

        [Fact]
        public void A_bare_year_is_january_as_a_start_and_december_as_an_end()
        {
            Assert.Equal(Index("Jan 2020"), Index("2020", isEnd: false));
            Assert.Equal(Index("Dec 2020"), Index("2020", isEnd: true));
        }

        [Fact]
        public void Month_names_compare_chronologically_not_alphabetically()
        {
            // F3: as strings "May 2020" sorts after "Dec 2023".
            Assert.True(Index("Dec 2023") > Index("May 2020"));
            Assert.True(Index("2024-01") > Index("12/2023"));
        }

        [Fact]
        public void Recency_key_ranks_present_above_any_date_and_unreadable_last()
        {
            var present = DateParser.RecencyKey("Jan 2018", "Present");
            var recent = DateParser.RecencyKey("Jan 2020", "Dec 2023");
            var older = DateParser.RecencyKey("Jan 2015", "May 2020");
            var unreadable = DateParser.RecencyKey("", "");

            var ordered = new[] { unreadable, older, present, recent }.OrderByDescending(k => k).ToList();

            Assert.Equal(new[] { present, recent, older, unreadable }, ordered);
        }

        [Fact]
        public void Recency_key_breaks_end_date_ties_by_start_date()
        {
            var later = DateParser.RecencyKey("Jan 2023", "Present");
            var earlier = DateParser.RecencyKey("Jan 2015", "Present");

            Assert.True(later.CompareTo(earlier) > 0);
        }

        [Fact]
        public void Recency_key_falls_back_to_the_start_date_when_the_end_is_unreadable()
        {
            var key = DateParser.RecencyKey("Mar 2021", "n/a");

            Assert.Equal(Index("Mar 2021"), key.End);
        }

        [Fact]
        public void Years_of_experience_floors_whole_years_of_a_single_range()
        {
            Assert.Equal(4, DateParser.YearsOfExperience(new (string?, string?)[] { ("Jan 2018", "Dec 2022") }));
            Assert.Equal(2, DateParser.YearsOfExperience(new (string?, string?)[] { ("Jan 2020", "Jan 2022") }));
            Assert.Equal(0, DateParser.YearsOfExperience(new (string?, string?)[] { ("Jan 2020", "Nov 2020") }));
        }

        [Fact]
        public void Years_of_experience_does_not_count_gaps()
        {
            var ranges = new (string?, string?)[]
            {
                ("Jan 2010", "Jan 2012"),   // 2 years
                ("Jan 2018", "Jan 2021")    // 3 years, 6 year gap before it
            };

            Assert.Equal(5, DateParser.YearsOfExperience(ranges));
        }

        [Fact]
        public void Years_of_experience_merges_overlapping_roles()
        {
            var ranges = new (string?, string?)[]
            {
                ("Jan 2015", "Jan 2019"),
                ("Jan 2017", "Jan 2020"),   // overlaps 2 years
                ("Jan 2016", "Jan 2018")    // fully inside the first
            };

            Assert.Equal(5, DateParser.YearsOfExperience(ranges));
        }

        [Fact]
        public void Years_of_experience_treats_a_missing_end_as_ongoing_and_ignores_a_missing_start()
        {
            var start = $"Jan {DateTime.Now.Year - 3}";

            Assert.Equal(3, DateParser.YearsOfExperience(new (string?, string?)[] { (start, ""), ("", "2020") }));
            Assert.Equal(0, DateParser.YearsOfExperience(Array.Empty<(string?, string?)>()));
        }

        [Fact]
        public void Years_of_experience_ignores_ranges_that_end_before_they_start()
        {
            Assert.Equal(0, DateParser.YearsOfExperience(new (string?, string?)[] { ("Jan 2022", "Jan 2020") }));
        }
    }
}
