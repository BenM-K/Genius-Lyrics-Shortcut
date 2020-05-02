using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Win32;
using FlaUI.UIA3;
using Newtonsoft.Json;
using Genius.Models;
using Hotkeys;
using static GeniusShortcut.GeniusAPI;

namespace GeniusShortcut
{
    public partial class Main : Form
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        public const int VK_VOLUME_UP = 0xAF;
        public const int VK_VOLUME_DOWN = 0xAE;

        private const string LAST_UPDATE_DATE = "May 2nd 2020";
        private const string GENIUS_API_TOKEN = "yEwvFUOvsOYufC9VftbnPl6B4dvJWXV0sVKQ661SCl4DgMJcn2gk750RJd-3leqB";

        public GlobalHotkey ghk;
        private Stopwatch stopwatch;
        private dynamic jsonResponse;
        private int modifierSum;
        private string artist;
        private string song;
        private string artistp2;
        private string songp2;
        private string unformattedSong;
        private string unformattedArtist;
        private string geniusSearchTerm;
        private bool exitClickedFromTray;
        private bool changeKeyDialogOpen = false;
        private bool hasModifier = false;
        private bool currentlyFetchingSong = false;
        private bool settingFirstApplied = true;

        private bool startupEnabled = (bool)Properties.Settings.Default["startupEnabled"];
        private bool minimizeToTraySetting = (bool)Properties.Settings.Default["minimizeToTray"];
        private string hotkeySetting = (string)Properties.Settings.Default["hotkey"];
        private string modifierSetting = (string)Properties.Settings.Default["modifier"];

        public Main()
        {
            if (!IsWindows10())
            {
                MessageBox.Show("This application only works on Windows 10", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            InitializeComponent();
        }

        static bool IsWindows10()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            string productName = (string)reg.GetValue("ProductName");

            return productName.StartsWith("Windows 10");
        }

        private void Dialog_Load(object sender, EventArgs e)
        {
            TrayMenuContext();

            CreateHotkeyFromSettings();

            LoadSettings();

            if (hasModifier)
            {
                WriteLine("Trying to register hotkey " + modifierSetting + "+" + hotkeySetting + "...");
            }
            else
            {
                WriteLine("Trying to register hotkey '" + hotkeySetting + "'...");
            }

            if (ghk.Register())
            {
                WriteLine(Environment.NewLine + "Hotkey registered, click the button below to change hotkey");
                WriteLine(Environment.NewLine + "Listening for hotkey...");

                if (hasModifier)
                {
                    Text = "Genius Shortcut (Hotkey: " + modifierSetting + "+" + hotkeySetting + ")";
                }
                else
                {
                    Text = "Genius Shortcut (Hotkey: '" + hotkeySetting + "')";
                }
            }
            else
            {
                WriteLine(Environment.NewLine + "Hotkey failed to register");
            }
        }

        private void Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (minimizeToTray.Checked && !exitClickedFromTray)
            {
                e.Cancel = true;
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
            }
            else
            {
                if (!ghk.Unregiser())
                {
                    MessageBox.Show("Hotkey failed to unregister", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateHotkeyFromSettings()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(modifierSetting))
                {
                    hasModifier = true;
                }

                Enum.TryParse(hotkeySetting, out Keys hotkey);

                string[] mods = modifierSetting.Split('+');

                for (int i = 0; i < mods.Length; i++)
                {
                    if (mods[i] == "ALT")
                    {
                        modifierSum += HotKeyConstants.ALT;
                        continue;
                    }

                    if (mods[i] == "CTRL")
                    {
                        modifierSum += HotKeyConstants.CTRL;
                        continue;
                    }

                    if (mods[i] == "Shift")
                    {
                        modifierSum += HotKeyConstants.SHIFT;
                        continue;
                    }

                    if (mods[i] == "Win")
                    {
                        modifierSum += HotKeyConstants.WIN;
                        continue;
                    }
                }

                ghk = new GlobalHotkey(modifierSum, hotkey, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering hotkey: " + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void LoadSettings()
        {
            if (startupEnabled)
            {
                settingFirstApplied = false;
                startWithWindows.Checked = true;
            }
            else
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    key.DeleteValue("Genius Shortcut", false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Couldn't delete startup key" + Environment.NewLine + "(" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLine(Environment.NewLine + "Couldn't delete startup key");
                }
            }

            if (minimizeToTraySetting)
            {
                minimizeToTray.Checked = true;
            }
            else
            {
                minimizeToTray.Checked = false;
            }
        }

        private void HandleHotkey()
        {
            stopwatch = new Stopwatch();

            stopwatch.Start();

            ResetVariables();

            WriteLine(Environment.NewLine + "Reading song info from system...");

            try
            {
                keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_EXTENDEDKEY, 0);
                keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_EXTENDEDKEY, 0);
                keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_KEYUP, 0);

                Thread.Sleep(10);

                var uiAutomation = new UIA3Automation();
                var rootElement = uiAutomation.GetDesktop();
                var idArtistName = rootElement.FindFirstDescendant(cf => cf.ByAutomationId("idArtistName"));
                var idStreamName = rootElement.FindFirstDescendant(cf => cf.ByAutomationId("idStreamName"));

                if (idArtistName == null && idStreamName == null)
                {
                    MessageBox.Show("Failed to read song info from system" + Environment.NewLine + Environment.NewLine + "Please make sure there currently is a song playing, if there is, please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLine(Environment.NewLine + "Failed to read song info from system");

                    stopwatch.Stop();

                    currentlyFetchingSong = false;
                    findSongBtn.Enabled = true;
                }
                else
                {
                    if (idArtistName.Name != null)
                    {
                        string artistRaw = idArtistName.Name;
                        artist = artistRaw.Replace("Track details ", "");
                        unformattedArtist = artist;
                    }

                    if (idStreamName.Name != null)
                    {
                        string songRaw = idStreamName.Name;
                        song = songRaw.Replace("Track name ", "");
                        unformattedSong = song;
                    }

                    //To possibly implement: If browser page is on genius, open the youtube video for the song on page, if browser page is on rym or other sites open genius lyrics...

                    WriteLine(Environment.NewLine + "Formatting song info...");

                    FormatStrings(ref artist, ref song, ref artistp2, ref songp2);

                    //if (!string.IsNullOrWhiteSpace(artist))
                    //{
                    //    WriteLine(Environment.NewLine + "Attemping to fetch lyrics for '" + textInfo.ToTitleCase(song) + "' by " + textInfo.ToTitleCase(artist) + "...");
                    //}
                    //else
                    //{
                    //    WriteLine(Environment.NewLine + "Attemping to fetch lyrics for '" + textInfo.ToTitleCase(song) + "'...");
                    //}

                    WriteLine(Environment.NewLine + "Attemping to fetch lyrics using generated URLs...");

                    FetchGeniusInfo(artist, song, artistp2, songp2);

                    stopwatch.Stop();

                    currentlyFetchingSong = false;
                    findSongBtn.Enabled = true;

                    WriteLine(Environment.NewLine + "Listening for hotkey...");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetVariables()
        {
            artist = "";
            song = "";
            artistp2 = "";
            songp2 = "";
            unformattedArtist = "";
            unformattedSong = "";
            geniusSearchTerm = "";
        }

        //This section could be reworked, probably a better way of doing all this
        //Some songs won't work
        //Bugs:
		//Messes up if artist looks like "artistA+artistB"
		//Messes up if there is a collection of artists (artistA, artistB, etc.. some artists have a comma in their name soo)
		//If song is playing from spotify, only 1 artist is shown, even if there might be 2 with the track

        private void FormatStrings(ref string artist, ref string song, ref string artistp2, ref string songp2)
        {
            string[] artistAndSongSplit;

            if (song.Contains(": "))
            {
                artistAndSongSplit = song.Split(new string[] { ": " }, StringSplitOptions.None);
                artistAndSongSplit = FormatArtistAndSong(artistAndSongSplit[0], artistAndSongSplit[1]);

                artist = artistAndSongSplit[0];
                song = artistAndSongSplit[1];
                artistp2 = artistAndSongSplit[2];
                songp2 = artistAndSongSplit[3];

                return;
            }

            if (song.Contains("“"))
            {
                artistAndSongSplit = song.Split('“');

                //string[] endQuoteSplitters = new string[] { "”", "\"", "'", "“" };
                string[] endQuoteSplitters = new string[] { "”", "\"", "“" };

                for (int i = 0; i < endQuoteSplitters.Length; i++)
                {
                    if (artistAndSongSplit[1].Contains(endQuoteSplitters[i]))
                    {
                        string[] temp = artistAndSongSplit[1].Split(new string[] { endQuoteSplitters[i] }, StringSplitOptions.None);
                        artistAndSongSplit[1] = temp[0];
                    }
                }

                artistAndSongSplit = FormatArtistAndSong(artistAndSongSplit[0], artistAndSongSplit[1]);

                artist = artistAndSongSplit[0];
                song = artistAndSongSplit[1];
                artistp2 = artistAndSongSplit[2];
                songp2 = artistAndSongSplit[3];

                return;
            }

            string[] songTitleQuoteSplitters = new string[] { "\"", "“" };

            for (int i = 0; i < songTitleQuoteSplitters.Length; i++)
            {
                if (song.Contains(songTitleQuoteSplitters[i]))
                {
                    artistAndSongSplit = song.Split(new string[] { songTitleQuoteSplitters[i] }, StringSplitOptions.None);

                    artistAndSongSplit = FormatArtistAndSong(artistAndSongSplit[0], artistAndSongSplit[1]);

                    artist = artistAndSongSplit[0];
                    song = artistAndSongSplit[1];
                    artistp2 = artistAndSongSplit[2];
                    songp2 = artistAndSongSplit[3];

                    return;
                }
            }

            //string[] songSplitters = new string[] { " - ", "-", " -- ", "--", " – ", " –– ", "–", "––", " | ", " : ", " ~ " };
            string[] songSplitters = new string[] { " - ", " -- ", "--", " – ", " –– ", "–", "––", " | ", " : ", " ~ " };

            for (int i = 0; i < songSplitters.Length; i++)
            {
                if (song.Contains(songSplitters[i]))
                {
                    artistAndSongSplit = song.Split(new string[] { songSplitters[i] }, StringSplitOptions.None);

                    artistAndSongSplit = FormatArtistAndSong(artistAndSongSplit[0], artistAndSongSplit[1]);

                    artist = artistAndSongSplit[0];
                    song = artistAndSongSplit[1];
                    artistp2 = artistAndSongSplit[2];
                    songp2 = artistAndSongSplit[3];

                    return;
                }
            }

            if (artist.Contains(" - Topic"))
            {
                string[] artistNameSplit = artist.Split(new string[] { " - Topic" }, StringSplitOptions.None);

                artistAndSongSplit = FormatArtistAndSong(artistNameSplit[0], song);

                artist = artistAndSongSplit[0];
                song = artistAndSongSplit[1];
                artistp2 = artistAndSongSplit[2];
                songp2 = artistAndSongSplit[3];

                return;
            }
            else
            {
                artistAndSongSplit = FormatArtistAndSong(artist, song);

                artist = artistAndSongSplit[0];
                song = artistAndSongSplit[1];
                artistp2 = artistAndSongSplit[2];
                songp2 = artistAndSongSplit[3];
            }
        }

        private string[] FormatArtistAndSong(string artist, string song)
        {
            string[] artistSplitters = new string[] { " & ", " + ", " x ", " ft. ", " ft ", " feat. ", " feat ", " and " };

            for (int i = 0; i < artistSplitters.Length; i++)
            {
                if (artist.ToLower().Contains(artistSplitters[i]))
                {
                    string[] artistsSplit = artist.ToLower().Split(new string[] { artistSplitters[i] }, StringSplitOptions.None);

                    artist = artistsSplit[0];
                    artistp2 = artistsSplit[1];

                    artistp2 = URLFriendly(artistp2);
                }
            }

            //string[] songArtistSplitters = new string[] { " ft ", " ft. ", " feat ", " feat. " };
            string[] songArtistSplitters = new string[] { " & ", " + ", " x ", " ft. ", " ft ", " feat. ", " feat ", "(ft.", "(ft", "(feat.", "(feat", "[ft.", "[ft", "[feat.", "[feat" };

            for (int i = 0; i < songArtistSplitters.Length; i++)
            {
                if (song.ToLower().Contains(songArtistSplitters[i]))
                {
                    string[] artistsSplit = song.ToLower().Split(new string[] { songArtistSplitters[i] }, StringSplitOptions.None);

                    artistp2 = artistsSplit[1];
                    song = artistsSplit[0];

                    string[] songSplitters0 = new string[] { "(", "[", "{", " prod ", "prod. ", "official", "lyric", "audio" };

                    for (int z = 0; z < songSplitters0.Length; z++)
                    {
                        if (artistp2.Contains(songSplitters0[z]))
                        {
                            artistsSplit = artistp2.ToLower().Split(new string[] { songSplitters0[z] }, StringSplitOptions.None);
                            artistp2 = artistsSplit[0];
                        }
                    }

                    artistp2 = URLFriendly(artistp2);
                }
            }

            string[] songp2Splitters = new string[] { "(", "[", "{", " prod ", "prod. ", "official", "lyric", " audio " };

            for (int i = 0; i < songp2Splitters.Length; i++)
            {
                if (song.ToLower().Contains(songp2Splitters[i]))
                {
                    string[] songs = song.ToLower().Split(new string[] { songp2Splitters[i] }, StringSplitOptions.None);

                    song = songs[0];

                    if (songp2Splitters[i] == "(" || songp2Splitters[i] == "[")
                    {
                        songp2 = songs[1];

                        songp2 = URLFriendly(songp2);
                    }
                }
            }

            artist = URLFriendly(artist);
            song = URLFriendly(song);

            //artist = artist.Replace(" - ", "");

            //int songLastCharIndex = song.Length - 1;
            //char lastChar = song[songLastCharIndex];

            //if (lastChar == ' ')
            //{
            //    StringBuilder sb = new StringBuilder(song);
            //    sb.Remove(songLastCharIndex, 1);
            //    song = sb.ToString();
            //}

            return new string[] { artist, song, artistp2, songp2 };
        }

        //private string FinalStringFormat(string theString)
        //{
        //    theString = theString.Replace("&", "and");
        //    theString = theString.Replace("$", "-");
        //    theString = theString.Replace(" ", "-"); //.ToLower();
        //    theString = theString.Replace(".", "");
            
        //    //This one's a bit weird, look into it
        //    //theString = Regex.Replace(theString, @"/[^a-zA-Z0-9-_]/g", "");
        //    theString = Regex.Replace(theString, @"[^A-Za-z0-9_\.]", "");

        //    return theString;
        //}

        /// <summary>
        /// Produces optional, URL-friendly version of a title, "like-this-one". 
        /// hand-tuned for speed, reflects performance refactoring contributed
        /// by John Gietzen (user otac0n) 
        /// </summary>
        public static string URLFriendly(string title)
        {
            //if (title.Contains("$"))
            //{
            //    title = title.Replace("$", "-");
            //}

           if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    //tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                //removed period and comma, added $
                //else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=')
                else if (c == ' ' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=' || c == '$')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            //just to be safe, might wanna make sure this is robust or even needed
            //if (sb.ToString().Contains("--"))
            //{
            //    sb.ToString().Replace("--", "-");

            //    while (sb.ToString().Contains("--"))
            //    {
            //        sb.ToString().Replace("--", "-");
            //    }
            //}

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }

        private void FetchGeniusInfo(string artist, string song, string artistp2, string songp2)
        {
            string link = "https://genius.com/" + artist + "-" + song + "-lyrics";

            while (link.Contains("--"))
            {
                link = link.Replace("--", "-");
            }

            if (!IsValidLyricsURL(link))
            {
                link = "https://genius.com/" + artist + "-" + song + "-" + songp2 + "-lyrics";

                while (link.Contains("--"))
                {
                    link = link.Replace("--", "-");
                }

                if (!IsValidLyricsURL(link))
                {
                    link = "https://genius.com/" + artist + "-and-" + artistp2 + "-" + song + "-lyrics";

                    while (link.Contains("--"))
                    {
                        link = link.Replace("--", "-");
                    }

                    if (!IsValidLyricsURL(link))
                    {
                        link = "https://genius.com/" + artist + "-and-" + artistp2 + "-" + song + "-" + songp2 + "-lyrics";

                        while (link.Contains("--"))
                        {
                            link = link.Replace("--", "-");
                        }

                        if (!IsValidLyricsURL(link))
                        {
                            WriteLine(Environment.NewLine + "The generated URLs failed, trying to use Genius API to search for lyrics... (resulting song may not be 100% accurate)");

                            string[] songSplit = { "" };
                            string[] songSplitters = new string[] { " - ", " -- ", "--", " – ", " –– ", "–", "––", " | ", " : ", " ~ ", "(", "[", "{", " prod. ", "prod ", "official", "lyric", " audio ", " ft. ", " ft ", " feat. ", " feat ", "(ft.", "(ft", "(feat.", "(feat", "[ft.", "[ft", "[feat.", "[feat" };

                            for (int i = 0; i < songSplitters.Length; i++)
                            {
                                if (unformattedSong.ToLower().Contains(songSplitters[i]))
                                {
                                    songSplit = unformattedSong.Split(new string[] { songSplitters[i] }, StringSplitOptions.None);
                                    break;
                                }
                            }

                            //unformattedArtist = URLFriendly(unformattedArtist);
                            //unformattedSong = URLFriendly(unformattedSong);

                            if (songSplit[0] == "")
                            {
                                geniusSearchTerm = artist + " - " + song;
                            }
                            else
                            {
                                for (int i = 0; i < songSplit.Length; i++)
                                {
                                    songSplit[i] = URLFriendly(songSplit[i]);
                                }

                                if (unformattedArtist == "Various Artists")
                                {
                                    geniusSearchTerm = songSplit[0];
                                }
                                else
                                {
                                    //maybe swap with geniusSearchTerm = songSplit[0] + " - " + songSplit[1] + " " + unformattedArtist; ?
                                    geniusSearchTerm = songSplit[0] + " " + unformattedArtist;
                                }
                            }

                            if (!Task.Run(() => SearchGeniusFound()).Result)
                            {
                                if (unformattedArtist == "Various Artists")
                                {
                                    geniusSearchTerm = unformattedSong;
                                }
                                else
                                {
                                    geniusSearchTerm = unformattedSong + " " + unformattedArtist;
                                }

                                if (!Task.Run(() => SearchGeniusFound()).Result)
                                {
                                    if (songSplit.Length > 1)
                                    {
                                        geniusSearchTerm = songSplit[0] + " " + songSplit[1];
                                    }
                                    else
                                    {
                                        geniusSearchTerm = songSplit[0] + " " + unformattedArtist;
                                    }

                                    if (!Task.Run(() => SearchGeniusFound()).Result)
                                    {
                                        if (songSplit.Length > 1)
                                        {
                                            geniusSearchTerm = songSplit[0] + " - " + songSplit[1] + " " + unformattedArtist;
                                        }
                                        else
                                        {
                                            geniusSearchTerm = songSplit[0] + " " + artist;
                                        }

                                        if (!Task.Run(() => SearchGeniusFound()).Result)
                                        {
                                            if (songSplit.Length > 1)
                                            {
                                                geniusSearchTerm = songSplit[0] + " - " + songSplit[1] + " " + artist;
                                            }
                                            else
                                            {
                                                geniusSearchTerm = unformattedSong;
                                            }

                                            if (!Task.Run(() => SearchGeniusFound()).Result)
                                            {
                                                geniusSearchTerm = artist + " " + song;

                                                if (!Task.Run(() => SearchGeniusFound()).Result)
                                                {
                                                    geniusSearchTerm = artist;

                                                    if (!Task.Run(() => SearchGeniusFound()).Result)
                                                    {
                                                        geniusSearchTerm = song;

                                                        if (!Task.Run(() => SearchGeniusFound()).Result)
                                                        {
                                                            MessageBox.Show("Couldn't find song on genius", "Song not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                                            if (stopwatch.Elapsed.TotalSeconds < 1)
                                                            {
                                                                WriteLine(Environment.NewLine + "Couldn't find song on genius (searched in less than a second)");
                                                            }
                                                            else
                                                            {
                                                                WriteLine(Environment.NewLine + "Couldn't find song on genius (searched in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 2) + " seconds)");
                                                            }

                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Verify song by getting title/artist from genius page info and comparing to unformatted variables
                            //string geniusTitle = jsonResponse.title;
                            //geniusTitle = geniusTitle.ToUpper();

                            //song = song.Replace('-', ' ');
                            //song = song.ToUpper();

                            //int similarity = LevenshteinDistance.Compute(geniusTitle, song);

                            //if (similarity < 60)
                            //{
                            //    switch (searchTermIndex)
                            //    {
                            //        case 1:
                            //            break;
                            //    }
                            //}

                            if (stopwatch.Elapsed.TotalSeconds < 1)
                            {
                                WriteLine(Environment.NewLine + "Possible song match found! Opening in browser... (found in less than a second)");
                            }
                            else
                            {
                                WriteLine(Environment.NewLine + "Possible song match found! Opening in browser... (found in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 2) + " seconds)");
                            }

                            string url = jsonResponse.url;

                            Process.Start(url);

                            return;
                        }
                    }
                }
            }

            if (stopwatch.Elapsed.TotalSeconds < 1)
            {
                WriteLine(Environment.NewLine + "Song found! Opening in browser... (found in less than a second)");
            }
            else
            {
                WriteLine(Environment.NewLine + "Song found! Opening in browser... (found in " + Math.Round(stopwatch.Elapsed.TotalSeconds, 2) + " seconds)");
            }

            Process.Start(link);
        }

        private bool IsValidLyricsURL(string link)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                //if (response != null)
                //{
                //    response.Close();
                //}

                return false;
            }

            //string geniusPageSource = "";

            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    Stream receiveStream = response.GetResponseStream();
            //    StreamReader readStream = null;

            //    if (String.IsNullOrWhiteSpace(response.CharacterSet))
            //        readStream = new StreamReader(receiveStream);
            //    else
            //        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

            //    geniusPageSource = readStream.ReadToEnd();

            //    response.Close();
            //    readStream.Close();
            //}

            response.Close();

            return true;
        }

        private async Task<bool> SearchGeniusFound()
        {
            try
            {
                var geniusClient = new GeniusClient(GENIUS_API_TOKEN);
                var searchResult = await geniusClient.SearchClient.Search(TextFormat.Dom, geniusSearchTerm);

                if (searchResult.Response.Count <= 0)
                {
                    return false;
                }
                else
                {
                    if (searchResult.Response[0].Type == "song")
                    {
                        jsonResponse = JsonConvert.DeserializeObject(searchResult.Response[0].Result.ToString());
                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < searchResult.Response.Count; i++)
                        {
                            if (searchResult.Response[i].Type == "song")
                            {
                                jsonResponse = JsonConvert.DeserializeObject(searchResult.Response[i].Result.ToString());
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to search using Genius API" + Environment.NewLine + "(" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Environment.Exit(1);
            }

            return false;
        }
        
        //Instead of doing this, we should really just make sure that there's no extra dashes before or after the variables when they're sent in...
        //It's safe, but not efficient
        //private string removeDuplicateDashes(string theString)
        //{
        //    bool includesDuplicateDashes = true;

        //    while (includesDuplicateDashes)
        //    {
        //        theString = theString.Replace("--", "-");
        //        includesDuplicateDashes = theString.Contains("--");
        //    }

        //    if (theString[19] == '-')
        //    {
        //        StringBuilder sb = new StringBuilder(theString);
        //        sb.Remove(19, 1);
        //        theString = sb.ToString();
        //    }

        //    return theString;
        //}

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKeyConstants.WM_HOTKEY_MSG_ID)
            {
                changeKeyDialogOpen = false;

                FormCollection fc = Application.OpenForms;

                foreach (Form frm in fc)
                {
                    if (frm.Name == "ChangeHotKey")
                    {
                        changeKeyDialogOpen = true;
                    }
                }

                if (!currentlyFetchingSong && !changeKeyDialogOpen)
                {
                    currentlyFetchingSong = true;
                    findSongBtn.Enabled = false;
                    WriteLine(Environment.NewLine + "Hotkey pressed!");

                    HandleHotkey();
                }
                else
                {
                    //MessageBox.Show("That key is already being used in the currently registered hotkey. Please change the hotkey to another key before using that one again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            base.WndProc(ref m);
        }

        public void WriteLine(string text)
        {
            txtBoxLog.AppendText(text + Environment.NewLine);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            ActiveControl = logLabel;
        }

        private void HotKeyBtn_Click(object sender, EventArgs e)
        {
            ChangeHotKey Dialog = new ChangeHotKey(this);
            Dialog.ShowDialog();
        }

        private void Dialog_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
            }
        }

        private void startWithWindows_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                bool startupKeyExists = key.GetValueNames().Contains("Genius Shortcut");

                if (startWithWindows.Checked)
                {
                    if (!startupKeyExists)
                    {
                        key.SetValue("Genius Shortcut", Application.ExecutablePath);
                        Properties.Settings.Default["startupEnabled"] = true;

                        if (settingFirstApplied)
                        {
                            WriteLine(Environment.NewLine + "Added to startup within the registry" + Environment.NewLine + "(" + key.ToString() + ")");
                        }
                    }
                }
                else
                {
                    settingFirstApplied = true;
                    key.DeleteValue("Genius Shortcut", false);
                    Properties.Settings.Default["startupEnabled"] = false;
                    WriteLine(Environment.NewLine + "Removed from startup");
                }

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                if (startWithWindows.Checked)
                {
                    MessageBox.Show("Couldn't add to startup" + Environment.NewLine + Environment.NewLine + "(" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLine(Environment.NewLine + "Couldn't add to startup");
                }
                else
                {
                    MessageBox.Show("Couldn't remove from startup" + Environment.NewLine + Environment.NewLine + "(" + ex.Message + ")", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLine(Environment.NewLine + "Couldn't remove from startup");
                }
            }
        }

        private void minimizeToTray_CheckedChanged(object sender, EventArgs e)
        {
            if (minimizeToTray.Checked)
            {
                Properties.Settings.Default["minimizeToTray"] = true;
            }
            else
            {
                Properties.Settings.Default["minimizeToTray"] = false;
            }

            Properties.Settings.Default.Save();
        }

        private void findSongBtn_Click(object sender, EventArgs e)
        {
            currentlyFetchingSong = true;
            findSongBtn.Enabled = false;

            HandleHotkey();
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBox.Show("Genius Shortcut " + "(" + version + ")" + Environment.NewLine + Environment.NewLine + "Instantly get the lyrics for whatever song you're listening to with the push of a button." + Environment.NewLine + Environment.NewLine + "How does it work?" + Environment.NewLine + Environment.NewLine + "Simply press the hotkey or click the 'Find Song' button. The program will attempt to read song information from the system and will use this information to look up lyrics on genius. If lyrics are found, the lyrics page will be opened in a new tab in the default browser." + Environment.NewLine + Environment.NewLine + "The program first generates possible URLs for the lyrics page based on song information from the system and tests if they are valid. If they aren't, the lyrics are searched for using an API." + Environment.NewLine + Environment.NewLine + "Written in C# by Benjamin Morelli-Kirshner" + Environment.NewLine + Environment.NewLine + "Last updated: " + LAST_UPDATE_DATE, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            }
            else
            {
                notifyIcon.ContextMenuStrip.Show(Cursor.Position.X, Cursor.Position.Y - 70);
            }
        }

        private void TrayMenuContext()
        {
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            //notifyIcon.ContextMenuStrip.Items.Add("Find Song", null, HandleHotkey);
            notifyIcon.ContextMenuStrip.Items.Add("Change Hotkey", null, HotKeyBtn_Click);
            notifyIcon.ContextMenuStrip.Items.Add("About", null, aboutBtn_Click);
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, MenuExit_Click);
        }

        void MenuExit_Click(object sender, EventArgs e)
        {
            exitClickedFromTray = true;
            Application.Exit();
        }
    }
}