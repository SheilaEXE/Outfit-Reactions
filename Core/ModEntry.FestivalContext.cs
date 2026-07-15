using StardewValley;
using StardewValley.GameData;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OutfitReactions;

public sealed partial class ModEntry
{
    private const int DaysPerSeason = 28;
    private const int DaysPerYear = DaysPerSeason * 4;

    private string GetFestivalContextForAiPrompt()
    {
        if (Config?.IncludeFestivalContextForAi != true)
            return "";

        try
        {
            List<FestivalOccurrence> festivals = LoadFestivalCalendar();
            if (festivals.Count == 0)
                return "No enabled festivals were found in the current game calendar.";

            int currentSeasonIndex = GetSeasonIndex(Game1.currentSeason);
            int currentDayIndex = currentSeasonIndex * DaysPerSeason + Math.Clamp(Game1.dayOfMonth - 1, 0, DaysPerSeason - 1);

            List<FestivalOccurrence> today = festivals
                .Where(festival => festival.ContainsDay(currentDayIndex))
                .OrderBy(festival => festival.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            FestivalOccurrence previous = festivals
                .Where(festival => festival.EndDayIndex < currentDayIndex)
                .OrderByDescending(festival => festival.EndDayIndex)
                .FirstOrDefault();
            int previousDaysAgo;
            if (previous != null)
            {
                previousDaysAgo = currentDayIndex - previous.EndDayIndex;
            }
            else
            {
                previous = festivals.OrderByDescending(festival => festival.EndDayIndex).FirstOrDefault();
                previousDaysAgo = previous == null ? 0 : currentDayIndex + DaysPerYear - previous.EndDayIndex;
            }

            List<(FestivalOccurrence Festival, int DaysUntil)> upcoming = festivals
                .Where(festival => festival.StartDayIndex > currentDayIndex)
                .Select(festival => (festival, festival.StartDayIndex - currentDayIndex))
                .Concat(festivals
                    .Where(festival => festival.StartDayIndex <= currentDayIndex)
                    .Select(festival => (festival, DaysPerYear - currentDayIndex + festival.StartDayIndex)))
                .OrderBy(entry => entry.Item2)
                .ThenBy(entry => entry.festival.Name, StringComparer.CurrentCultureIgnoreCase)
                .Take(3)
                .Select(entry => (entry.festival, entry.Item2))
                .ToList();

            List<FestivalOccurrence> currentSeason = festivals
                .Where(festival => festival.SeasonIndex == currentSeasonIndex)
                .OrderBy(festival => festival.StartDay)
                .ThenBy(festival => festival.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            StringBuilder context = new StringBuilder();
            context.Append("Authoritative festival calendar (localized and mod-aware). ");
            if (today.Count > 0)
            {
                context.Append("TODAY: ")
                    .Append(string.Join("; ", today.Select(festival => FormatFestivalDate(festival, includeStatus: false))))
                    .Append(". ");
            }
            else
            {
                context.Append("There is no festival today. ");
            }

            if (previous != null && !today.Contains(previous))
            {
                context.Append("Most recently passed: ")
                    .Append(previous.Name)
                    .Append(" ended ")
                    .Append(previousDaysAgo)
                    .Append(previousDaysAgo == 1 ? " day ago" : " days ago")
                    .Append(". ");
            }

            if (upcoming.Count > 0)
            {
                context.Append("Next festivals: ")
                    .Append(string.Join("; ", upcoming.Select(entry =>
                        $"{entry.Festival.Name} in {entry.DaysUntil} {(entry.DaysUntil == 1 ? "day" : "days")} ({FormatFestivalDate(entry.Festival, includeStatus: false)})")))
                    .Append(". ");
            }

            if (currentSeason.Count > 0)
            {
                context.Append("Current-season schedule: ")
                    .Append(string.Join("; ", currentSeason.Select(festival => FormatFestivalWithRelativeStatus(festival, currentDayIndex))))
                    .Append(". ");
            }

            context.Append("This is calendar awareness only: do not claim the NPC or farmer is attending a festival unless the current location supports that. Mention a festival only when it naturally matters to the outfit, timing, or conversation.");
            return context.ToString();
        }
        catch (Exception ex)
        {
            if (DebugLog)
                Monitor.Log("[FESTIVAL CONTEXT] Could not build the festival calendar: " + ex.Message, StardewModdingAPI.LogLevel.Warn);
            return "Festival calendar information is currently unavailable.";
        }
    }

    private List<FestivalOccurrence> LoadFestivalCalendar()
    {
        List<FestivalOccurrence> festivals = new List<FestivalOccurrence>();
        Regex activeDatePattern = new Regex("^(spring|summer|fall|winter)([1-9]|1[0-9]|2[0-8])$", RegexOptions.IgnoreCase);

        Dictionary<string, string> activeFestivals = DataLoader.Festivals_FestivalDates(Game1.temporaryContent);
        foreach ((string dateKey, string rawName) in activeFestivals)
        {
            Match match = activeDatePattern.Match(dateKey ?? "");
            if (!match.Success || !int.TryParse(match.Groups[2].Value, out int day))
                continue;

            int seasonIndex = GetSeasonIndex(match.Groups[1].Value);
            string name = ResolveFestivalName(rawName, dateKey);
            festivals.Add(new FestivalOccurrence(name, seasonIndex, day, day, isPassive: false));
        }

        Dictionary<string, PassiveFestivalData> passiveFestivals = DataLoader.PassiveFestivals(Game1.content);
        foreach ((string id, PassiveFestivalData data) in passiveFestivals)
        {
            if (data == null || !data.ShowOnCalendar || data.StartDay < 1 || data.StartDay > DaysPerSeason || data.EndDay < data.StartDay || data.EndDay > DaysPerSeason)
                continue;

            if (!IsPassiveFestivalEnabled(data))
                continue;

            int seasonIndex = GetSeasonIndex(data.Season.ToString());
            string name = ResolveFestivalName(data.DisplayName, id);
            festivals.Add(new FestivalOccurrence(name, seasonIndex, data.StartDay, data.EndDay, isPassive: true));
        }

        return festivals
            .GroupBy(festival => festival.DeduplicationKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(festival => festival.StartDayIndex)
            .ThenBy(festival => festival.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private bool IsPassiveFestivalEnabled(PassiveFestivalData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.Condition))
            return true;

        try
        {
            return GameStateQuery.CheckConditions(data.Condition);
        }
        catch (Exception ex)
        {
            if (DebugLog)
                Monitor.Log("[FESTIVAL CONTEXT] Ignored a passive festival with an invalid condition: " + ex.Message, StardewModdingAPI.LogLevel.Trace);
            return false;
        }
    }

    private static string ResolveFestivalName(string rawName, string fallback)
    {
        string name = TokenParser.ParseText(rawName)?.Trim();
        return string.IsNullOrWhiteSpace(name) ? fallback : name;
    }

    private static int GetSeasonIndex(string season)
    {
        string normalized = (season ?? "").Trim().ToLowerInvariant();
        if (normalized.Contains("summer"))
            return 1;
        if (normalized.Contains("fall") || normalized.Contains("autumn"))
            return 2;
        if (normalized.Contains("winter"))
            return 3;
        return 0;
    }

    private static string GetSeasonName(int seasonIndex)
    {
        return seasonIndex switch
        {
            1 => "summer",
            2 => "fall",
            3 => "winter",
            _ => "spring"
        };
    }

    private static string FormatFestivalDate(FestivalOccurrence festival, bool includeStatus)
    {
        string day = festival.StartDay == festival.EndDay
            ? festival.StartDay.ToString()
            : festival.StartDay + "-" + festival.EndDay;
        string value = festival.Name + " on " + GetSeasonName(festival.SeasonIndex) + " " + day;
        return includeStatus && festival.IsPassive ? value + " (passive/multi-day festival)" : value;
    }

    private static string FormatFestivalWithRelativeStatus(FestivalOccurrence festival, int currentDayIndex)
    {
        string date = FormatFestivalDate(festival, includeStatus: true);
        if (festival.ContainsDay(currentDayIndex))
        {
            int dayNumber = currentDayIndex - festival.StartDayIndex + 1;
            int daysRemaining = festival.EndDayIndex - currentDayIndex;
            if (festival.StartDay == festival.EndDay)
                return date + " (today)";
            return date + $" (active today, day {dayNumber}, {daysRemaining} {(daysRemaining == 1 ? "day" : "days")} remaining)";
        }

        if (festival.EndDayIndex < currentDayIndex)
        {
            int daysAgo = currentDayIndex - festival.EndDayIndex;
            return date + $" (passed {daysAgo} {(daysAgo == 1 ? "day" : "days")} ago)";
        }

        int daysUntil = festival.StartDayIndex - currentDayIndex;
        return date + $" (in {daysUntil} {(daysUntil == 1 ? "day" : "days")})";
    }

    private sealed class FestivalOccurrence
    {
        public string Name { get; }
        public int SeasonIndex { get; }
        public int StartDay { get; }
        public int EndDay { get; }
        public bool IsPassive { get; }
        public int StartDayIndex => SeasonIndex * DaysPerSeason + StartDay - 1;
        public int EndDayIndex => SeasonIndex * DaysPerSeason + EndDay - 1;
        public string DeduplicationKey => Name + "|" + SeasonIndex + "|" + StartDay + "|" + EndDay;

        public FestivalOccurrence(string name, int seasonIndex, int startDay, int endDay, bool isPassive)
        {
            Name = name;
            SeasonIndex = seasonIndex;
            StartDay = startDay;
            EndDay = endDay;
            IsPassive = isPassive;
        }

        public bool ContainsDay(int dayIndex)
        {
            return dayIndex >= StartDayIndex && dayIndex <= EndDayIndex;
        }
    }
}
