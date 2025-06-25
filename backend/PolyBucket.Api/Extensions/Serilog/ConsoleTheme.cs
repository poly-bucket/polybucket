using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Extensions.Serilog
{
    public class ConsoleTheme
    {
        public static readonly SystemConsoleTheme CustomTheme = new(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new() { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.SecondaryText] = new() { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.TertiaryText] = new() { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.Invalid] = new() { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.Null] = new() { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Name] = new() { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.String] = new() { Foreground = ConsoleColor.Cyan },
                [ConsoleThemeStyle.Number] = new() { Foreground = ConsoleColor.Magenta },
                [ConsoleThemeStyle.Boolean] = new() { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Scalar] = new() { Foreground = ConsoleColor.Green },
                [ConsoleThemeStyle.LevelVerbose] = new() { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.LevelDebug] = new() { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.LevelInformation] = new() { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.LevelWarning] = new() { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.LevelError] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
                [ConsoleThemeStyle.LevelFatal] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
            });
    }
} 