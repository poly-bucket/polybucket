using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace PolyBucket.Api.Seeders;

public static class ThemeSeeder
{
    public static async Task SeedThemesAsync(PolyBucketDbContext context)
    {
        if (await context.Themes.AnyAsync())
        {
            return; // Themes already seeded
        }

        var themes = new List<Theme>
        {
            new()
            {
                Name = "Ocean Blue",
                Description = "A calming ocean-inspired theme with blue tones",
                IsDefault = true,
                IsActive = true,
                Colors = new ThemeColors
                {
                    Primary = "#0ea5e9",
                    PrimaryLight = "#38bdf8",
                    PrimaryDark = "#0284c7",
                    Secondary = "#6366f1",
                    SecondaryLight = "#818cf8",
                    SecondaryDark = "#4f46e5",
                    Accent = "#06b6d4",
                    AccentLight = "#22d3ee",
                    AccentDark = "#0891b2",
                    BackgroundPrimary = "#0f172a",
                    BackgroundSecondary = "#1e293b",
                    BackgroundTertiary = "#334155"
                }
            },
            new()
            {
                Name = "Purple Dream",
                Description = "A dreamy purple theme with vibrant accents",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#8b5cf6",
                    PrimaryLight = "#a78bfa",
                    PrimaryDark = "#7c3aed",
                    Secondary = "#ec4899",
                    SecondaryLight = "#f472b6",
                    SecondaryDark = "#db2777",
                    Accent = "#f59e0b",
                    AccentLight = "#fbbf24",
                    AccentDark = "#d97706",
                    BackgroundPrimary = "#1e1b4b",
                    BackgroundSecondary = "#312e81",
                    BackgroundTertiary = "#4338ca"
                }
            },
            new()
            {
                Name = "Emerald Forest",
                Description = "A natural green theme inspired by forests",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#10b981",
                    PrimaryLight = "#34d399",
                    PrimaryDark = "#059669",
                    Secondary = "#059669",
                    SecondaryLight = "#10b981",
                    SecondaryDark = "#047857",
                    Accent = "#f59e0b",
                    AccentLight = "#fbbf24",
                    AccentDark = "#d97706",
                    BackgroundPrimary = "#064e3b",
                    BackgroundSecondary = "#065f46",
                    BackgroundTertiary = "#047857"
                }
            },
            new()
            {
                Name = "Sunset Orange",
                Description = "A warm sunset-inspired theme with orange and red tones",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#f97316",
                    PrimaryLight = "#fb923c",
                    PrimaryDark = "#ea580c",
                    Secondary = "#ef4444",
                    SecondaryLight = "#f87171",
                    SecondaryDark = "#dc2626",
                    Accent = "#f59e0b",
                    AccentLight = "#fbbf24",
                    AccentDark = "#d97706",
                    BackgroundPrimary = "#451a03",
                    BackgroundSecondary = "#7c2d12",
                    BackgroundTertiary = "#92400e"
                }
            },
            new()
            {
                Name = "Midnight Purple",
                Description = "A deep purple theme for night owls",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#6366f1",
                    PrimaryLight = "#818cf8",
                    PrimaryDark = "#4f46e5",
                    Secondary = "#8b5cf6",
                    SecondaryLight = "#a78bfa",
                    SecondaryDark = "#7c3aed",
                    Accent = "#06b6d4",
                    AccentLight = "#22d3ee",
                    AccentDark = "#0891b2",
                    BackgroundPrimary = "#0f0f23",
                    BackgroundSecondary = "#1a1a2e",
                    BackgroundTertiary = "#16213e"
                }
            },
            new()
            {
                Name = "Dracula",
                Description = "The iconic Dracula theme with purple, pink, and cyan",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#bd93f9",
                    PrimaryLight = "#d6bcf9",
                    PrimaryDark = "#a78bfa",
                    Secondary = "#ff79c6",
                    SecondaryLight = "#ffa7d7",
                    SecondaryDark = "#ff6bb6",
                    Accent = "#8be9fd",
                    AccentLight = "#b8f2fd",
                    AccentDark = "#5fd4fd",
                    BackgroundPrimary = "#282a36",
                    BackgroundSecondary = "#44475a",
                    BackgroundTertiary = "#6272a4"
                }
            },
            new()
            {
                Name = "Base16 Nord",
                Description = "A cool Arctic-inspired theme with blue and gray tones",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#88c0d0",
                    PrimaryLight = "#a3be8c",
                    PrimaryDark = "#81a1c1",
                    Secondary = "#b48ead",
                    SecondaryLight = "#d08770",
                    SecondaryDark = "#5e81ac",
                    Accent = "#ebcb8b",
                    AccentLight = "#f0f0f0",
                    AccentDark = "#d8dee9",
                    BackgroundPrimary = "#2e3440",
                    BackgroundSecondary = "#3b4252",
                    BackgroundTertiary = "#434c5e"
                }
            },
            new()
            {
                Name = "Atom One Dark",
                Description = "The classic Atom One Dark theme with blue and purple",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#61afef",
                    PrimaryLight = "#98c379",
                    PrimaryDark = "#56b6c2",
                    Secondary = "#c678dd",
                    SecondaryLight = "#e06c75",
                    SecondaryDark = "#be5046",
                    Accent = "#d19a66",
                    AccentLight = "#e5c07b",
                    AccentDark = "#abb2bf",
                    BackgroundPrimary = "#282c34",
                    BackgroundSecondary = "#21252b",
                    BackgroundTertiary = "#181a1f"
                }
            },
            new()
            {
                Name = "Base16 GitHub",
                Description = "GitHub's official color scheme with blue and green",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#0366d6",
                    PrimaryLight = "#28a745",
                    PrimaryDark = "#005cc5",
                    Secondary = "#6f42c1",
                    SecondaryLight = "#e36209",
                    SecondaryDark = "#5a32a3",
                    Accent = "#d73a49",
                    AccentLight = "#f6f8fa",
                    AccentDark = "#cb2431",
                    BackgroundPrimary = "#ffffff",
                    BackgroundSecondary = "#f6f8fa",
                    BackgroundTertiary = "#e1e4e8"
                }
            },
            new()
            {
                Name = "Base16 Darkviolet",
                Description = "A deep violet theme with vibrant pink and cyan accents",
                IsDefault = false,
                IsActive = false,
                Colors = new ThemeColors
                {
                    Primary = "#a675ff",
                    PrimaryLight = "#c8a0ff",
                    PrimaryDark = "#8b5aff",
                    Secondary = "#ff73fd",
                    SecondaryLight = "#ffa0fe",
                    SecondaryDark = "#ff5afd",
                    Accent = "#00d9ff",
                    AccentLight = "#66e6ff",
                    AccentDark = "#00b3cc",
                    BackgroundPrimary = "#000000",
                    BackgroundSecondary = "#231040",
                    BackgroundTertiary = "#432d59"
                }
            }
        };

        foreach (var theme in themes)
        {
            theme.CreatedAt = DateTime.UtcNow;
            theme.UpdatedAt = DateTime.UtcNow;
        }

        context.Themes.AddRange(themes);
        await context.SaveChangesAsync();
    }
}
