using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Hotkeys;

namespace GeniusShortcut
{
    public partial class ChangeHotKey : Form
    {
        private List<string> keysEntered = new List<string>();
        private string[] modifiers = new string[] { "ALT", "CTRL", "Shift", "Win" };
        private int modifierSum = 0;
        private bool hasModifier = false;
        private string key;

        private Main mainForm = null;

        public ChangeHotKey(Form callingForm)
        {
            mainForm = callingForm as Main;

            InitializeComponent();
        }

        private void SetBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(keysTxtBox.Text))
            {
                StringBuilder modifiersBuilder = new StringBuilder();
                string newText = null;

                for (int i = 0; i < modifiers.Length; i++)
                {
                    if (keysTxtBox.Text.Contains(modifiers[i]))
                    {
                        hasModifier = true;

                        if (modifiersBuilder.ToString() == string.Empty)
                        {
                            modifiersBuilder.Append(modifiers[i]);

                            if (keysTxtBox.Text.Contains(modifiers[i] + "+"))
                            {
                                newText = keysTxtBox.Text.Replace(modifiers[i] + "+", "");
                            }
                            if (keysTxtBox.Text.Contains("+" + modifiers[i]))
                            {
                                newText = keysTxtBox.Text.Replace("+" + modifiers[i], "");
                            }

                            keysTxtBox.Text = newText;
                        }
                        else
                        {
                            modifiersBuilder.Append("+" + modifiers[i]);

                            if (!keysTxtBox.Text.Contains("+"))
                            {
                                keysTxtBox.Text = "";
                                break;
                            }

                            if (keysTxtBox.Text.Contains(modifiers[i] + "+"))
                            {
                                newText = keysTxtBox.Text.Replace(modifiers[i] + "+", "");
                            }
                            if (keysTxtBox.Text.Contains("+" + modifiers[i]))
                            {
                                newText = keysTxtBox.Text.Replace("+" + modifiers[i], "");
                            }

                            keysTxtBox.Text = newText;
                        }
                    }
                }

                key = keysTxtBox.Text;

                if (string.IsNullOrWhiteSpace(key))
                {
                    MessageBox.Show("Hotkey cannot only be modifier keys; you must add another key other than CTRL, ALT, Windows Key, or shift", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    keysTxtBox.Text = "";
                    keysEntered.Clear();
                    hasModifier = false;
                    keysTxtBox.Focus();

                    return;
                }

                if (!hasModifier)
                {
                    DialogResult result = MessageBox.Show("You have entered no modifier keys (CTRL, ALT, Windows Key, or Shift)" + Environment.NewLine + Environment.NewLine + "The program will attempt to find lyrics every time '" + key + "' is pressed." + Environment.NewLine + Environment.NewLine + "Continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        mainForm.ghk.Unregiser();

                        Enum.TryParse(key, out Keys hotkey);

                        mainForm.ghk = new GlobalHotkey(HotKeyConstants.NOMOD, hotkey, mainForm);

                        mainForm.WriteLine(Environment.NewLine + "Trying to register hotkey '" + key + "'...");

                        if (mainForm.ghk.Register())
                        {
                            mainForm.WriteLine(Environment.NewLine + "Hotkey registered, click the button below to change hotkey");
                            mainForm.Text = "Genius Shortcut (Hotkey: '" + key + "')";

                            Properties.Settings.Default["modifier"] = "";
                            Properties.Settings.Default["hotkey"] = key;

                            Properties.Settings.Default.Save();

                            mainForm.WriteLine(Environment.NewLine + "Settings saved");
                            mainForm.WriteLine(Environment.NewLine + "Listening for hotkey...");
                        }
                        else
                        {
                            mainForm.WriteLine(Environment.NewLine + "Hotkey failed to register");
                        }

                        Hide();
                    }
                    else
                    {
                        keysTxtBox.Text = "";
                        keysEntered.Clear();
                        hasModifier = false;
                        keysTxtBox.Focus();
                    }
                }
                else
                {
                    if (!modifiersBuilder.ToString().Contains("+"))
                    {
                        switch (modifiersBuilder.ToString())
                        {
                            case "ALT":
                                modifierSum = HotKeyConstants.ALT;
                                break;

                            case "CTRL":
                                modifierSum = HotKeyConstants.CTRL;
                                break;

                            case "Shift":
                                modifierSum = HotKeyConstants.SHIFT;
                                break;

                            case "Win":
                                modifierSum = HotKeyConstants.WIN;
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        string[] mods = modifiersBuilder.ToString().Split('+');

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
                            }
                        }
                    }

                    mainForm.ghk.Unregiser();

                    Enum.TryParse(key, out Keys hotkey);

                    mainForm.ghk = new GlobalHotkey(modifierSum, hotkey, mainForm);

                    modifierSum = 0;

                    mainForm.WriteLine(Environment.NewLine + "Trying to register hotkey " + modifiersBuilder.ToString() + "+" + key + "...");

                    if (mainForm.ghk.Register())
                    {
                        mainForm.WriteLine(Environment.NewLine + "Hotkey registered, click the button below to change hotkey.");
                        mainForm.Text = "Genius Shortcut (Hotkey: " + modifiersBuilder.ToString() + "+" + key + ")";

                        Properties.Settings.Default["modifier"] = modifiersBuilder.ToString();
                        Properties.Settings.Default["hotkey"] = key;

                        Properties.Settings.Default.Save();

                        mainForm.WriteLine(Environment.NewLine + "Settings saved");
                        mainForm.WriteLine(Environment.NewLine + "Listening for hotkey...");
                    }
                    else
                    {
                        mainForm.WriteLine(Environment.NewLine + "Hotkey failed to register");
                    }

                    Hide();
                }
            }
            else
            {
                MessageBox.Show("Field cannot be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                keysTxtBox.Text = "";
                keysEntered.Clear();
                hasModifier = false;
                keysTxtBox.Focus();
            }
        }

        private void KeysTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string key = e.KeyCode.ToString();

            if (keysTxtBox.Text.Length == 0)
            {
                switch (key)
                {
                    case "Menu":
                        if (!keysEntered.Contains("ALT"))
                        {
                            keysEntered.Add("ALT");
                            key = "ALT";
                            keysTxtBox.Text += key;
                        }
                        break;

                    case "ControlKey":
                        if (!keysEntered.Contains("CTRL"))
                        {
                            keysEntered.Add("CTRL");
                            key = "CTRL";
                            keysTxtBox.Text += key;
                        }
                        break;

                    case "ShiftKey":
                        if (!keysEntered.Contains("Shift"))
                        {
                            keysEntered.Add("Shift");
                            key = "Shift";
                            keysTxtBox.Text += key;
                        }
                        break;

                    case "LWin":
                        if (!keysEntered.Contains("Win"))
                        {
                            keysEntered.Add("Win");
                            key = "Win";
                            keysTxtBox.Text += key;
                        }
                        break;

                    case "RWin":
                        if (!keysEntered.Contains("Win"))
                        {
                            keysEntered.Add("Win");
                            key = "Win";
                            keysTxtBox.Text += key;
                        }
                        break;

                    default:
                        keysEntered.Add(e.KeyCode.ToString());
                        keysTxtBox.Text += e.KeyCode.ToString();
                        break;
                }
            }
            else
            {
                switch (key)
                {
                    case "Menu":
                        if (!keysEntered.Contains("ALT"))
                        {
                            keysEntered.Add("ALT");
                            key = "ALT";
                            keysTxtBox.Text +=  "+" + key;
                        }
                        break;

                    case "ControlKey":
                        if (!keysEntered.Contains("CTRL"))
                        {
                            keysEntered.Add("CTRL");
                            key = "CTRL";
                            keysTxtBox.Text += "+" + key;
                        }
                        break;

                    case "ShiftKey":
                        if (!keysEntered.Contains("Shift"))
                        {
                            keysEntered.Add("Shift");
                            key = "Shift";
                            keysTxtBox.Text += "+" + key;
                        }
                        break;

                    case "LWin":
                        if (!keysEntered.Contains("Win"))
                        {
                            keysEntered.Add("Win");
                            key = "Win";
                            keysTxtBox.Text += "+" + key;
                        }
                        break;

                    case "RWin":
                        if (!keysEntered.Contains("Win"))
                        {
                            keysEntered.Add("Win");
                            key = "Win";
                            keysTxtBox.Text += "+" + key;
                        }
                        break;

                    default:
                        if ((!keysEntered.Contains(e.KeyCode.ToString())) && (keysEntered[keysEntered.Count - 1] == "ALT" || keysEntered[keysEntered.Count - 1] == "CTRL" || keysEntered[keysEntered.Count - 1] == "Shift" || keysEntered[keysEntered.Count - 1] == "Win"))
                        {
                            keysEntered.Add(e.KeyCode.ToString());
                            keysTxtBox.Text += "+" + e.KeyCode.ToString();
                        }
                        break;
                }
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            keysTxtBox.Text = "";
            keysEntered.Clear();
            hasModifier = false;
            keysTxtBox.Focus();
        }

        private void keysTxtBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(keysTxtBox.Text))
            {
                setBtn.Enabled = true;
            }
            else
            {
                setBtn.Enabled = false;
            }
        }
    }
}