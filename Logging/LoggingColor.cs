using static ScapeCore.Traceability.Logging.LoggingColor;

namespace ScapeCore.Traceability.Logging
{
    public readonly record struct LoggingColor(ColorEnum Color)
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public static readonly LoggingColor Default = new(ColorEnum.Default);
        public static readonly LoggingColor Normal = new(ColorEnum.Normal);
        public static readonly LoggingColor Red = new(ColorEnum.Red);
        public static readonly LoggingColor Green = new(ColorEnum.Green);
        public static readonly LoggingColor Yellow = new(ColorEnum.Yellow);
        public static readonly LoggingColor Blue = new(ColorEnum.Blue);
        public static readonly LoggingColor Magenta = new(ColorEnum.Magenta);
        public static readonly LoggingColor Cyan = new(ColorEnum.Cyan);
        public static readonly LoggingColor Grey = new(ColorEnum.Grey);
        public static readonly LoggingColor Bold = new(ColorEnum.Bold);
        public static readonly LoggingColor NoBold = new(ColorEnum.NoBold);
        public static readonly LoggingColor Underline = new(ColorEnum.Underline);
        public static readonly LoggingColor NoUnderline = new(ColorEnum.NoUnderline);
        public static readonly LoggingColor Reverse = new(ColorEnum.Reverse);
        public static readonly LoggingColor NoReverse = new(ColorEnum.NoReverse);

        private const string ANSI_SCAPE_CODE = "\u001b";

        public LoggingColor(byte r, byte g, byte b) : this(ColorEnum.None)
        {
            R = r;
            G = g;
            B = b;
        }

        public static string operator +(LoggingColor a, LoggingColor b) => a.ToString() + b.ToString();

        public override readonly string? ToString() => Color switch
        {
            ColorEnum.Default => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Default}m",
            ColorEnum.Red => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Red}m",
            ColorEnum.Green => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Green}m",
            ColorEnum.Yellow => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Yellow}m",
            ColorEnum.Blue => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Blue}m",
            ColorEnum.Magenta => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Magenta}m",
            ColorEnum.Cyan => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Cyan}m",
            ColorEnum.Grey => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Grey}m",

            ColorEnum.Bold => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Bold}m",
            ColorEnum.NoBold => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.NoBold}m",
            ColorEnum.Underline => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Underline}m",
            ColorEnum.NoUnderline => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.NoUnderline}m",
            ColorEnum.Reverse => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Reverse}m",
            ColorEnum.NoReverse => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.NoReverse}m",

            ColorEnum.Normal => $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.Default}m" +
                                $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.NoBold}m" +
                                $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.NoReverse}m" +
                                $"{ANSI_SCAPE_CODE}[{(int)ColorEnum.NoUnderline}m",
            _ => ToString(false),
        };

        public readonly string? ToString(bool isBackground) => isBackground ?
                                                               $"{ANSI_SCAPE_CODE}[48;2;{R};{G};{B}m" :
                                                               $"{ANSI_SCAPE_CODE}[38;2;{R};{G};{B}m";
        public enum ColorEnum : int
        {
            None = 0,
            Default = 39,
            Red = 91,
            Green = 92,
            Yellow = 93,
            Blue = 94,
            Magenta = 95,
            Cyan = 96,
            Grey = 97,
            Bold = 1,
            NoBold = 22,
            Underline = 4,
            NoUnderline = 24,
            Reverse = 7,
            NoReverse = 27,
            Normal = Default | NoBold | NoUnderline | NoReverse,
        }
    }
}