using Avalonia.Input;

namespace TAS.Avalonia;

// from WinForms
public enum Keys {
    /// <summary><para>No key pressed.</para></summary>
    None = 0,
    /// <summary><para>The left mouse button.</para></summary>
    LButton = 1,
    /// <summary><para>The right mouse button.</para></summary>
    RButton = 2,
    /// <summary><para>The CANCEL key.</para></summary>
    Cancel = RButton | LButton, // 0x00000003
    /// <summary><para>The middle mouse button (three-button mouse).</para></summary>
    MButton = 4,
    /// <summary><para>The first x mouse button (five-button mouse).</para></summary>
    XButton1 = MButton | LButton, // 0x00000005
    /// <summary><para>The second x mouse button (five-button mouse).</para></summary>
    XButton2 = MButton | RButton, // 0x00000006
    /// <summary><para>The BACKSPACE key.</para></summary>
    Back = 8,
    /// <summary><para>The TAB key.</para></summary>
    Tab = Back | LButton, // 0x00000009
    /// <summary><para>The LINEFEED key.</para></summary>
    LineFeed = Back | RButton, // 0x0000000A
    /// <summary><para>The CLEAR key.</para></summary>
    Clear = Back | MButton, // 0x0000000C
    /// <summary><para>The RETURN key.</para></summary>
    Return = Clear | LButton, // 0x0000000D
    /// <summary><para>The ENTER key.</para></summary>
    Enter = Return, // 0x0000000D
    /// <summary><para>The SHIFT key.</para></summary>
    ShiftKey = 16, // 0x00000010
    /// <summary><para>The CTRL key.</para></summary>
    ControlKey = ShiftKey | LButton, // 0x00000011
    /// <summary><para>The ALT key.</para></summary>
    Menu = ShiftKey | RButton, // 0x00000012
    /// <summary><para>The PAUSE key.</para></summary>
    Pause = Menu | LButton, // 0x00000013
    /// <summary><para>The CAPS LOCK key.</para></summary>
    CapsLock = ShiftKey | MButton, // 0x00000014
    /// <summary><para>The CAPS LOCK key.</para></summary>
    Capital = CapsLock, // 0x00000014
    /// <summary><para>The IME Kana mode key.</para></summary>
    KanaMode = Capital | LButton, // 0x00000015
    /// <summary><para>The IME Hanguel mode key. (maintained for compatibility; use HangulMode) </para></summary>
    HanguelMode = KanaMode, // 0x00000015
    /// <summary><para>The IME Hangul mode key.</para></summary>
    HangulMode = HanguelMode, // 0x00000015
    /// <summary><para>The IME Junja mode key.</para></summary>
    JunjaMode = HangulMode | RButton, // 0x00000017
    /// <summary><para>The IME final mode key.</para></summary>
    FinalMode = ShiftKey | Back, // 0x00000018
    /// <summary><para>The IME Kanji mode key.</para></summary>
    KanjiMode = FinalMode | LButton, // 0x00000019
    /// <summary><para>The IME Hanja mode key.</para></summary>
    HanjaMode = KanjiMode, // 0x00000019
    /// <summary><para>The ESC key.</para></summary>
    Escape = HanjaMode | RButton, // 0x0000001B
    /// <summary><para>The IME convert key.</para></summary>
    IMEConvert = FinalMode | MButton, // 0x0000001C
    /// <summary><para>The IME nonconvert key.</para></summary>
    IMENonconvert = IMEConvert | LButton, // 0x0000001D
    /// <summary><para>The IME accept key. Obsolete, use <see cref="F:System.Windows.Forms.Keys.IMEAccept" /> instead.</para></summary>
    IMEAceept = IMEConvert | RButton, // 0x0000001E
    /// <summary><para>The IME mode change key.</para></summary>
    IMEModeChange = IMEAceept | LButton, // 0x0000001F
    /// <summary><para>The SPACEBAR key.</para></summary>
    Space = 32, // 0x00000020
    /// <summary><para>The PAGE UP key.</para></summary>
    PageUp = Space | LButton, // 0x00000021
    /// <summary><para>The PAGE UP key.</para></summary>
    Prior = PageUp, // 0x00000021
    /// <summary><para>The PAGE DOWN key.</para></summary>
    PageDown = Space | RButton, // 0x00000022
    /// <summary><para>The PAGE DOWN key.</para></summary>
    Next = PageDown, // 0x00000022
    /// <summary><para>The END key.</para></summary>
    End = Next | LButton, // 0x00000023
    /// <summary><para>The HOME key.</para></summary>
    Home = Space | MButton, // 0x00000024
    /// <summary><para>The LEFT ARROW key.</para></summary>
    Left = Home | LButton, // 0x00000025
    /// <summary><para>The UP ARROW key.</para></summary>
    Up = Home | RButton, // 0x00000026
    /// <summary><para>The RIGHT ARROW key.</para></summary>
    Right = Up | LButton, // 0x00000027
    /// <summary><para>The DOWN ARROW key.</para></summary>
    Down = Space | Back, // 0x00000028
    /// <summary><para>The SELECT key.</para></summary>
    Select = Down | LButton, // 0x00000029
    /// <summary><para>The PRINT key.</para></summary>
    Print = Down | RButton, // 0x0000002A
    /// <summary><para>The EXECUTE key.</para></summary>
    Execute = Print | LButton, // 0x0000002B
    /// <summary><para>The PRINT SCREEN key.</para></summary>
    PrintScreen = Down | MButton, // 0x0000002C
    /// <summary><para>The PRINT SCREEN key.</para></summary>
    Snapshot = PrintScreen, // 0x0000002C
    /// <summary><para>The INS key.</para></summary>
    Insert = Snapshot | LButton, // 0x0000002D
    /// <summary><para>The DEL key.</para></summary>
    Delete = Snapshot | RButton, // 0x0000002E
    /// <summary><para>The HELP key.</para></summary>
    Help = Delete | LButton, // 0x0000002F
    /// <summary><para>The 0 key.</para></summary>
    D0 = Space | ShiftKey, // 0x00000030
    /// <summary><para>The 1 key.</para></summary>
    D1 = D0 | LButton, // 0x00000031
    /// <summary><para>The 2 key.</para></summary>
    D2 = D0 | RButton, // 0x00000032
    /// <summary><para>The 3 key.</para></summary>
    D3 = D2 | LButton, // 0x00000033
    /// <summary><para>The 4 key.</para></summary>
    D4 = D0 | MButton, // 0x00000034
    /// <summary><para>The 5 key.</para></summary>
    D5 = D4 | LButton, // 0x00000035
    /// <summary><para>The 6 key.</para></summary>
    D6 = D4 | RButton, // 0x00000036
    /// <summary><para>The 7 key.</para></summary>
    D7 = D6 | LButton, // 0x00000037
    /// <summary><para>The 8 key.</para></summary>
    D8 = D0 | Back, // 0x00000038
    /// <summary><para>The 9 key.</para></summary>
    D9 = D8 | LButton, // 0x00000039
    /// <summary><para>The A key.</para></summary>
    A = 65, // 0x00000041
    /// <summary><para>The B key.</para></summary>
    B = 66, // 0x00000042
    /// <summary><para>The C key.</para></summary>
    C = B | LButton, // 0x00000043
    /// <summary><para>The D key.</para></summary>
    D = 68, // 0x00000044
    /// <summary><para>The E key.</para></summary>
    E = D | LButton, // 0x00000045
    /// <summary><para>The F key.</para></summary>
    F = D | RButton, // 0x00000046
    /// <summary><para>The G key.</para></summary>
    G = F | LButton, // 0x00000047
    /// <summary><para>The H key.</para></summary>
    H = 72, // 0x00000048
    /// <summary><para>The I key.</para></summary>
    I = H | LButton, // 0x00000049
    /// <summary><para>The J key.</para></summary>
    J = H | RButton, // 0x0000004A
    /// <summary><para>The K key.</para></summary>
    K = J | LButton, // 0x0000004B
    /// <summary><para>The L key.</para></summary>
    L = H | MButton, // 0x0000004C
    /// <summary><para>The M key.</para></summary>
    M = L | LButton, // 0x0000004D
    /// <summary><para>The N key.</para></summary>
    N = L | RButton, // 0x0000004E
    /// <summary><para>The O key.</para></summary>
    O = N | LButton, // 0x0000004F
    /// <summary><para>The P key.</para></summary>
    P = 80, // 0x00000050
    /// <summary><para>The Q key.</para></summary>
    Q = P | LButton, // 0x00000051
    /// <summary><para>The R key.</para></summary>
    R = P | RButton, // 0x00000052
    /// <summary><para>The S key.</para></summary>
    S = R | LButton, // 0x00000053
    /// <summary><para>The T key.</para></summary>
    T = P | MButton, // 0x00000054
    /// <summary><para>The U key.</para></summary>
    U = T | LButton, // 0x00000055
    /// <summary><para>The V key.</para></summary>
    V = T | RButton, // 0x00000056
    /// <summary><para>The W key.</para></summary>
    W = V | LButton, // 0x00000057
    /// <summary><para>The X key.</para></summary>
    X = P | Back, // 0x00000058
    /// <summary><para>The Y key.</para></summary>
    Y = X | LButton, // 0x00000059
    /// <summary><para>The Z key.</para></summary>
    Z = X | RButton, // 0x0000005A
    /// <summary><para>The left Windows logo key (Microsoft Natural Keyboard).</para></summary>
    LWin = Z | LButton, // 0x0000005B
    /// <summary><para>The right Windows logo key (Microsoft Natural Keyboard).</para></summary>
    RWin = X | MButton, // 0x0000005C
    /// <summary><para>The application key (Microsoft Natural Keyboard).</para></summary>
    Apps = RWin | LButton, // 0x0000005D
    /// <summary><para>The 0 key on the numeric keypad.</para></summary>
    NumPad0 = 96, // 0x00000060
    /// <summary><para>The 1 key on the numeric keypad.</para></summary>
    NumPad1 = NumPad0 | LButton, // 0x00000061
    /// <summary><para>The 2 key on the numeric keypad.</para></summary>
    NumPad2 = NumPad0 | RButton, // 0x00000062
    /// <summary><para>The 3 key on the numeric keypad.</para></summary>
    NumPad3 = NumPad2 | LButton, // 0x00000063
    /// <summary><para>The 4 key on the numeric keypad.</para></summary>
    NumPad4 = NumPad0 | MButton, // 0x00000064
    /// <summary><para>The 5 key on the numeric keypad.</para></summary>
    NumPad5 = NumPad4 | LButton, // 0x00000065
    /// <summary><para>The 6 key on the numeric keypad.</para></summary>
    NumPad6 = NumPad4 | RButton, // 0x00000066
    /// <summary><para>The 7 key on the numeric keypad.</para></summary>
    NumPad7 = NumPad6 | LButton, // 0x00000067
    /// <summary><para>The 8 key on the numeric keypad.</para></summary>
    NumPad8 = NumPad0 | Back, // 0x00000068
    /// <summary><para>The 9 key on the numeric keypad.</para></summary>
    NumPad9 = NumPad8 | LButton, // 0x00000069
    /// <summary><para>The multiply key.</para></summary>
    Multiply = NumPad8 | RButton, // 0x0000006A
    /// <summary><para>The add key.</para></summary>
    Add = Multiply | LButton, // 0x0000006B
    /// <summary><para>The separator key.</para></summary>
    Separator = NumPad8 | MButton, // 0x0000006C
    /// <summary><para>The subtract key.</para></summary>
    Subtract = Separator | LButton, // 0x0000006D
    /// <summary><para>The decimal key.</para></summary>
    Decimal = Separator | RButton, // 0x0000006E
    /// <summary><para>The divide key.</para></summary>
    Divide = Decimal | LButton, // 0x0000006F
    /// <summary><para>The F1 key.</para></summary>
    F1 = NumPad0 | ShiftKey, // 0x00000070
    /// <summary><para>The F2 key.</para></summary>
    F2 = F1 | LButton, // 0x00000071
    /// <summary><para>The F3 key.</para></summary>
    F3 = F1 | RButton, // 0x00000072
    /// <summary><para>The F4 key.</para></summary>
    F4 = F3 | LButton, // 0x00000073
    /// <summary><para>The F5 key.</para></summary>
    F5 = F1 | MButton, // 0x00000074
    /// <summary><para>The F6 key.</para></summary>
    F6 = F5 | LButton, // 0x00000075
    /// <summary><para>The F7 key.</para></summary>
    F7 = F5 | RButton, // 0x00000076
    /// <summary><para>The F8 key.</para></summary>
    F8 = F7 | LButton, // 0x00000077
    /// <summary><para>The F9 key.</para></summary>
    F9 = F1 | Back, // 0x00000078
    /// <summary><para>The F10 key.</para></summary>
    F10 = F9 | LButton, // 0x00000079
    /// <summary><para>The F11 key.</para></summary>
    F11 = F9 | RButton, // 0x0000007A
    /// <summary><para>The F12 key.</para></summary>
    F12 = F11 | LButton, // 0x0000007B
    /// <summary><para>The F13 key.</para></summary>
    F13 = F9 | MButton, // 0x0000007C
    /// <summary><para>The F14 key.</para></summary>
    F14 = F13 | LButton, // 0x0000007D
    /// <summary><para>The F15 key.</para></summary>
    F15 = F13 | RButton, // 0x0000007E
    /// <summary><para>The F16 key.</para></summary>
    F16 = F15 | LButton, // 0x0000007F
    /// <summary><para>The F17 key.</para></summary>
    F17 = 128, // 0x00000080
    /// <summary><para>The F18 key.</para></summary>
    F18 = F17 | LButton, // 0x00000081
    /// <summary><para>The F19 key.</para></summary>
    F19 = F17 | RButton, // 0x00000082
    /// <summary><para>The F20 key.</para></summary>
    F20 = F19 | LButton, // 0x00000083
    /// <summary><para>The F21 key.</para></summary>
    F21 = F17 | MButton, // 0x00000084
    /// <summary><para>The F22 key.</para></summary>
    F22 = F21 | LButton, // 0x00000085
    /// <summary><para>The F23 key.</para></summary>
    F23 = F21 | RButton, // 0x00000086
    /// <summary><para>The F24 key.</para></summary>
    F24 = F23 | LButton, // 0x00000087
    /// <summary><para>The NUM LOCK key.</para></summary>
    NumLock = F17 | ShiftKey, // 0x00000090
    /// <summary><para>The SCROLL LOCK key.</para></summary>
    Scroll = NumLock | LButton, // 0x00000091
    /// <summary><para>The left SHIFT key.</para></summary>
    LShiftKey = F17 | Space, // 0x000000A0
    /// <summary><para>The right SHIFT key.</para></summary>
    RShiftKey = LShiftKey | LButton, // 0x000000A1
    /// <summary><para>The left CTRL key.</para></summary>
    LControlKey = LShiftKey | RButton, // 0x000000A2
    /// <summary><para>The right CTRL key.</para></summary>
    RControlKey = LControlKey | LButton, // 0x000000A3
    /// <summary><para>The left ALT key.</para></summary>
    LMenu = LShiftKey | MButton, // 0x000000A4
    /// <summary><para>The right ALT key.</para></summary>
    RMenu = LMenu | LButton, // 0x000000A5
    /// <summary><para>The browser back key (Windows 2000 or later).</para></summary>
    BrowserBack = LMenu | RButton, // 0x000000A6
    /// <summary><para>The browser forward key (Windows 2000 or later).</para></summary>
    BrowserForward = BrowserBack | LButton, // 0x000000A7
    /// <summary><para>The browser refresh key (Windows 2000 or later).</para></summary>
    BrowserRefresh = LShiftKey | Back, // 0x000000A8
    /// <summary><para>The browser stop key (Windows 2000 or later).</para></summary>
    BrowserStop = BrowserRefresh | LButton, // 0x000000A9
    /// <summary><para>The browser search key (Windows 2000 or later).</para></summary>
    BrowserSearch = BrowserRefresh | RButton, // 0x000000AA
    /// <summary><para>The browser favorites key (Windows 2000 or later).</para></summary>
    BrowserFavorites = BrowserSearch | LButton, // 0x000000AB
    /// <summary><para>The browser home key (Windows 2000 or later).</para></summary>
    BrowserHome = BrowserRefresh | MButton, // 0x000000AC
    /// <summary><para>The volume mute key (Windows 2000 or later).</para></summary>
    VolumeMute = BrowserHome | LButton, // 0x000000AD
    /// <summary><para>The volume down key (Windows 2000 or later).</para></summary>
    VolumeDown = BrowserHome | RButton, // 0x000000AE
    /// <summary><para>The volume up key (Windows 2000 or later).</para></summary>
    VolumeUp = VolumeDown | LButton, // 0x000000AF
    /// <summary><para>The media next track key (Windows 2000 or later).</para></summary>
    MediaNextTrack = LShiftKey | ShiftKey, // 0x000000B0
    /// <summary><para>The media previous track key (Windows 2000 or later).</para></summary>
    MediaPreviousTrack = MediaNextTrack | LButton, // 0x000000B1
    /// <summary><para>The media Stop key (Windows 2000 or later).</para></summary>
    MediaStop = MediaNextTrack | RButton, // 0x000000B2
    /// <summary><para>The media play pause key (Windows 2000 or later).</para></summary>
    MediaPlayPause = MediaStop | LButton, // 0x000000B3
    /// <summary><para>The launch mail key (Windows 2000 or later).</para></summary>
    LaunchMail = MediaNextTrack | MButton, // 0x000000B4
    /// <summary><para>The select media key (Windows 2000 or later).</para></summary>
    SelectMedia = LaunchMail | LButton, // 0x000000B5
    /// <summary><para>The start application one key (Windows 2000 or later).</para></summary>
    LaunchApplication1 = LaunchMail | RButton, // 0x000000B6
    /// <summary><para>The start application two key (Windows 2000 or later).</para></summary>
    LaunchApplication2 = LaunchApplication1 | LButton, // 0x000000B7
    /// <summary><para>The OEM Semicolon key on a US standard keyboard (Windows 2000 or later).</para></summary>
    OemSemicolon = MediaStop | Back, // 0x000000BA
    /// <summary><para>The OEM plus key on any country/region keyboard (Windows 2000 or later).</para></summary>
    Oemplus = OemSemicolon | LButton, // 0x000000BB
    /// <summary><para>The OEM comma key on any country/region keyboard (Windows 2000 or later).</para></summary>
    Oemcomma = LaunchMail | Back, // 0x000000BC
    /// <summary><para>The OEM minus key on any country/region keyboard (Windows 2000 or later).</para></summary>
    OemMinus = Oemcomma | LButton, // 0x000000BD
    /// <summary><para>The OEM period key on any country/region keyboard (Windows 2000 or later).</para></summary>
    OemPeriod = Oemcomma | RButton, // 0x000000BE
    /// <summary><para>The OEM question mark key on a US standard keyboard (Windows 2000 or later).</para></summary>
    OemQuestion = OemPeriod | LButton, // 0x000000BF
    /// <summary><para>The OEM tilde key on a US standard keyboard (Windows 2000 or later).</para></summary>
    Oemtilde = 192, // 0x000000C0
    /// <summary><para>The OEM open bracket key on a US standard keyboard (Windows 2000 or later).</para></summary>
    OemOpenBrackets = Oemtilde | Escape, // 0x000000DB
    /// <summary><para>The OEM pipe key on a US standard keyboard (Windows 2000 or later).</para></summary>
    OemPipe = Oemtilde | IMEConvert, // 0x000000DC
    /// <summary><para>The OEM close bracket key on a US standard keyboard (Windows 2000 or later).</para></summary>
    OemCloseBrackets = OemPipe | LButton, // 0x000000DD
    /// <summary><para>The OEM singled/double quote key on a US standard keyboard (Windows 2000 or later).</para></summary>
    OemQuotes = OemPipe | RButton, // 0x000000DE
    /// <summary><para>The OEM 8 key.</para></summary>
    Oem8 = OemQuotes | LButton, // 0x000000DF
    /// <summary><para>The OEM angle bracket or backslash key on the RT 102 key keyboard (Windows 2000 or later).</para></summary>
    OemBackslash = Oemtilde | Next, // 0x000000E2
    /// <summary><para>The PROCESS KEY key.</para></summary>
    ProcessKey = Oemtilde | Left, // 0x000000E5
    /// <summary><para>The ATTN key.</para></summary>
    Attn = OemBackslash | Capital, // 0x000000F6
    /// <summary><para>The CRSEL key.</para></summary>
    Crsel = Attn | LButton, // 0x000000F7
    /// <summary><para>The EXSEL key.</para></summary>
    Exsel = Oemtilde | D8, // 0x000000F8
    /// <summary><para>The ERASE EOF key.</para></summary>
    EraseEof = Exsel | LButton, // 0x000000F9
    /// <summary><para>The PLAY key.</para></summary>
    Play = Exsel | RButton, // 0x000000FA
    /// <summary><para>The ZOOM key.</para></summary>
    Zoom = Play | LButton, // 0x000000FB
    /// <summary><para>A constant reserved for future use.</para></summary>
    NoName = Exsel | MButton, // 0x000000FC
    /// <summary><para>The PA1 key.</para></summary>
    Pa1 = NoName | LButton, // 0x000000FD
    /// <summary><para>The CLEAR key.</para></summary>
    OemClear = NoName | RButton, // 0x000000FE
    /// <summary><para>The bitmask to extract a key code from a key value.</para></summary>
    KeyCode = 65535, // 0x0000FFFF
    /// <summary><para>The SHIFT modifier key.</para></summary>
    Shift = 65536, // 0x00010000
    /// <summary><para>The CTRL modifier key.</para></summary>
    Control = 131072, // 0x00020000
    /// <summary><para>The ALT modifier key.</para></summary>
    Alt = 262144, // 0x00040000
    /// <summary><para>The bitmask to extract modifiers from a key value.</para></summary>
    Modifiers = -65536, // 0xFFFF0000
    /// <summary><para>The IME accept key, replaces <see cref="F:System.Windows.Forms.Keys.IMEAceept" />.</para></summary>
    IMEAccept = IMEAceept, // 0x0000001E
    /// <summary><para>The OEM 1 key.</para></summary>
    Oem1 = OemSemicolon, // 0x000000BA
    /// <summary><para>The OEM 102 key.</para></summary>
    Oem102 = OemBackslash, // 0x000000E2
    /// <summary><para>The OEM 2 key.</para></summary>
    Oem2 = Oem1 | XButton1, // 0x000000BF
    /// <summary><para>The OEM 3 key.</para></summary>
    Oem3 = Oemtilde, // 0x000000C0
    /// <summary><para>The OEM 4 key.</para></summary>
    Oem4 = Oem3 | Escape, // 0x000000DB
    /// <summary><para>The OEM 5 key.</para></summary>
    Oem5 = Oem3 | IMEConvert, // 0x000000DC
    /// <summary><para>The OEM 6 key.</para></summary>
    Oem6 = Oem5 | LButton, // 0x000000DD
    /// <summary><para>The OEM 7 key.</para></summary>
    Oem7 = Oem5 | RButton, // 0x000000DE
    /// <summary><para>Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods.</para></summary>
    Packet = Oem3 | Right, // 0x000000E7
    /// <summary><para>The computer sleep key.</para></summary>
    Sleep = IMEAccept | A, // 0x0000005F
}

public static class KeysExtensions {
    public static Keys ToWinForms(this Key key) => key switch {
        Key.None => Keys.None,
        Key.Cancel => Keys.Cancel,
        Key.Back => Keys.Back,
        Key.Tab => Keys.Tab,
        Key.LineFeed => Keys.LineFeed,
        Key.Clear => Keys.Clear,
        Key.Return => Keys.Return,
        Key.Pause => Keys.Pause,
        Key.CapsLock => Keys.CapsLock,
        Key.HangulMode => Keys.HangulMode,
        Key.JunjaMode => Keys.JunjaMode,
        Key.FinalMode => Keys.FinalMode,
        Key.KanjiMode => Keys.KanjiMode,
        Key.Escape => Keys.Escape,
        Key.ImeConvert => Keys.IMEConvert,
        Key.ImeNonConvert => Keys.IMENonconvert,
        Key.ImeAccept => Keys.IMEAccept,
        Key.ImeModeChange => Keys.IMEModeChange,
        Key.Space => Keys.Space,
        Key.PageUp => Keys.PageUp,
        Key.PageDown => Keys.PageDown,
        Key.End => Keys.End,
        Key.Home => Keys.Home,
        Key.Left => Keys.Left,
        Key.Up => Keys.Up,
        Key.Right => Keys.Right,
        Key.Down => Keys.Down,
        Key.Select => Keys.Select,
        Key.Print => Keys.Print,
        Key.Execute => Keys.Execute,
        Key.Snapshot => Keys.Snapshot,
        Key.Insert => Keys.Insert,
        Key.Delete => Keys.Delete,
        Key.Help => Keys.Help,
        Key.D0 => Keys.D0,
        Key.D1 => Keys.D1,
        Key.D2 => Keys.D2,
        Key.D3 => Keys.D3,
        Key.D4 => Keys.D4,
        Key.D5 => Keys.D5,
        Key.D6 => Keys.D6,
        Key.D7 => Keys.D7,
        Key.D8 => Keys.D8,
        Key.D9 => Keys.D9,
        Key.A => Keys.A,
        Key.B => Keys.B,
        Key.C => Keys.C,
        Key.D => Keys.D,
        Key.E => Keys.E,
        Key.F => Keys.F,
        Key.G => Keys.G,
        Key.H => Keys.H,
        Key.I => Keys.I,
        Key.J => Keys.J,
        Key.K => Keys.K,
        Key.L => Keys.L,
        Key.M => Keys.M,
        Key.N => Keys.N,
        Key.O => Keys.O,
        Key.P => Keys.P,
        Key.Q => Keys.Q,
        Key.R => Keys.R,
        Key.S => Keys.S,
        Key.T => Keys.T,
        Key.U => Keys.U,
        Key.V => Keys.V,
        Key.W => Keys.W,
        Key.X => Keys.X,
        Key.Y => Keys.Y,
        Key.Z => Keys.Z,
        Key.LWin => Keys.LWin,
        Key.RWin => Keys.RWin,
        Key.Apps => Keys.Apps,
        Key.Sleep => Keys.Sleep,
        Key.NumPad0 => Keys.NumPad0,
        Key.NumPad1 => Keys.NumPad1,
        Key.NumPad2 => Keys.NumPad2,
        Key.NumPad3 => Keys.NumPad3,
        Key.NumPad4 => Keys.NumPad4,
        Key.NumPad5 => Keys.NumPad5,
        Key.NumPad6 => Keys.NumPad6,
        Key.NumPad7 => Keys.NumPad7,
        Key.NumPad8 => Keys.NumPad8,
        Key.NumPad9 => Keys.NumPad9,
        Key.Multiply => Keys.Multiply,
        Key.Add => Keys.Add,
        Key.Separator => Keys.Separator,
        Key.Subtract => Keys.Subtract,
        Key.Decimal => Keys.Decimal,
        Key.Divide => Keys.Divide,
        Key.F1 => Keys.F1,
        Key.F2 => Keys.F2,
        Key.F3 => Keys.F3,
        Key.F4 => Keys.F4,
        Key.F5 => Keys.F5,
        Key.F6 => Keys.F6,
        Key.F7 => Keys.F7,
        Key.F8 => Keys.F8,
        Key.F9 => Keys.F9,
        Key.F10 => Keys.F10,
        Key.F11 => Keys.F11,
        Key.F12 => Keys.F12,
        Key.F13 => Keys.F13,
        Key.F14 => Keys.F14,
        Key.F15 => Keys.F15,
        Key.F16 => Keys.F16,
        Key.F17 => Keys.F17,
        Key.F18 => Keys.F18,
        Key.F19 => Keys.F19,
        Key.F20 => Keys.F20,
        Key.F21 => Keys.F21,
        Key.F22 => Keys.F22,
        Key.F23 => Keys.F23,
        Key.F24 => Keys.F24,
        Key.NumLock => Keys.NumLock,
        Key.Scroll => Keys.Scroll,
        Key.LeftShift => Keys.LShiftKey,
        Key.RightShift => Keys.RShiftKey,
        Key.LeftCtrl => Keys.LControlKey,
        Key.RightCtrl => Keys.RControlKey,
        Key.LeftAlt => Keys.LMenu,
        Key.RightAlt => Keys.RMenu,
        Key.BrowserBack => Keys.BrowserBack,
        Key.BrowserForward => Keys.BrowserForward,
        Key.BrowserRefresh => Keys.BrowserRefresh,
        Key.BrowserStop => Keys.BrowserStop,
        Key.BrowserSearch => Keys.BrowserSearch,
        Key.BrowserFavorites => Keys.BrowserFavorites,
        Key.BrowserHome => Keys.BrowserHome,
        Key.VolumeMute => Keys.VolumeMute,
        Key.VolumeDown => Keys.VolumeDown,
        Key.VolumeUp => Keys.VolumeUp,
        Key.MediaNextTrack => Keys.MediaNextTrack,
        Key.MediaPreviousTrack => Keys.MediaPreviousTrack,
        Key.MediaStop => Keys.MediaStop,
        Key.MediaPlayPause => Keys.MediaPlayPause,
        Key.LaunchMail => Keys.LaunchMail,
        Key.SelectMedia => Keys.SelectMedia,
        Key.LaunchApplication1 => Keys.LaunchApplication1,
        Key.LaunchApplication2 => Keys.LaunchApplication2,
        Key.OemSemicolon => Keys.OemSemicolon,
        Key.OemPlus => Keys.Oemplus,
        Key.OemComma => Keys.Oemcomma,
        Key.OemMinus => Keys.OemMinus,
        Key.OemPeriod => Keys.OemPeriod,
        Key.OemQuestion => Keys.OemQuestion,
        Key.OemTilde => Keys.Oemtilde,
        // Key.AbntC1 => Keys.AbntC1,
        // Key.AbntC2 => Keys.AbntC2,
        Key.OemOpenBrackets => Keys.OemOpenBrackets,
        Key.OemPipe => Keys.OemPipe,
        Key.OemCloseBrackets => Keys.OemCloseBrackets,
        Key.OemQuotes => Keys.OemQuotes,
        Key.Oem8 => Keys.Oem8,
        Key.OemBackslash => Keys.OemBackslash,
        // Key.ImeProcessed => Keys.ImeProcessed,
        // Key.System => Keys.System,
        // Key.OemAttn => Keys.OemAttn,
        // Key.OemFinish => Keys.OemFinish,
        // Key.DbeHiragana => Keys.DbeHiragana,
        // Key.DbeSbcsChar => Keys.DbeSbcsChar,
        // Key.DbeDbcsChar => Keys.DbeDbcsChar,
        // Key.OemBackTab => Keys.OemBackTab,
        // Key.DbeNoRoman => Keys.DbeNoRoman,
        Key.CrSel => Keys.Crsel,
        Key.ExSel => Keys.Exsel,
        Key.EraseEof => Keys.EraseEof,
        Key.Play => Keys.Play,
        // Key.DbeNoCodeInput => Keys.DbeNoCodeInput,
        Key.NoName => Keys.NoName,
        // Key.DbeEnterDialogConversionMode => Keys.DbeEnterDialogConversionMode,
        Key.OemClear => Keys.OemClear,
        // Key.DeadCharProcessed => Keys.DeadCharProcessed,
        // Key.FnLeftArrow => Keys.FnLeftArrow,
        // Key.FnRightArrow => Keys.FnRightArrow,
        // Key.FnUpArrow => Keys.FnUpArrow,
        // Key.FnDownArrow => Keys.FnDownArrow,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
    };
}
