using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.Reflection;

namespace TMGSSaveEditor.Core
{
    public partial class CheatsWindow : Window
    {
        private MainWindow _mainWindow;

        public CheatsWindow()
        {
            InitializeComponent();
        }

        public CheatsWindow(MainWindow mainWindow) : this()
        {
            _mainWindow = mainWindow;

            if (_mainWindow._savedata.ApproachCharacterNames != null)
            {
                ComboApproachCharSelect.ItemsSource = _mainWindow._savedata.ApproachCharacterNames;

                // Select the first character automatically so the box isn't blank
                if (ComboApproachCharSelect.ItemCount > 0)
                {
                    ComboApproachCharSelect.SelectedIndex = 0;
                }

                ComboFriendCharSelect.ItemsSource = _mainWindow._savedata.FriendCharacterNames;

                // Select the first character automatically so the box isn't blank
                if (ComboFriendCharSelect.ItemCount > 0)
                {
                    ComboFriendCharSelect.SelectedIndex = 0;
                }
            }
        }

        private void UnlockAllClothes()
        {
            if (_mainWindow.data == null) return;

            try
            {
                FieldInfo playerField = _mainWindow.data.GetType().GetField("player", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (playerField == null) return;
                object playerObj = playerField.GetValue(_mainWindow.data);

                FieldInfo fashionField = playerObj.GetType().GetField("fashion", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (fashionField == null) return;
                object fashionObj = fashionField.GetValue(playerObj);

                FieldInfo dressesField = fashionObj.GetType().GetField("isPossessionDresses", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (dressesField == null) return;

                bool[] dressesArr = (bool[])dressesField.GetValue(fashionObj);

                if (dressesArr != null)
                {
                    int safeLimit = _mainWindow._savedata.ClothingNames?.Length ?? dressesArr.Length;

                    for (int i = 0; i < dressesArr.Length; i++)
                    {
                        // Only unlock if it falls inside the known clothing limit
                        if (i < safeLimit)
                        {
                            dressesArr[i] = true;
                        }
                        // Explicitly force the padding elements to false to clean up buggy saves
                        else
                        {
                            dressesArr[i] = false;
                        }
                    }

                    _mainWindow.MarkAsChangedAndRefresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MaxAllStats()
        {
            if (_mainWindow.data == null) return;

            try
            {
                FieldInfo playerField = _mainWindow.data.GetType().GetField("player", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (playerField == null) return;
                object playerObj = playerField.GetValue(_mainWindow.data);

                FieldInfo paramsField = playerObj.GetType().GetField("standardParams", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (paramsField == null) return;

                float[] paramsArr = (float[])paramsField.GetValue(playerObj);

                if (paramsArr != null)
                {
                    for (int i = 0; i < paramsArr.Length; i++)
                    {
                        if (i == 6 || i == 7) continue; // SKIP STRESS AND MONEY
                        paramsArr[i] = 999;
                    }

                    _mainWindow.MarkAsChangedAndRefresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MaxMoney()
        {
            if (_mainWindow.data == null) return;
            try
            {
                FieldInfo playerField = _mainWindow.data.GetType().GetField("player", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (playerField == null) return;
                object playerObj = playerField.GetValue(_mainWindow.data);

                FieldInfo paramsField = playerObj.GetType().GetField("standardParams", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (paramsField == null) return;

                float[] paramsArr = (float[])paramsField.GetValue(playerObj);

                if (paramsArr != null && paramsArr.Length > 7)
                {
                    paramsArr[7] = 9800; // MONEY
                    _mainWindow.MarkAsChangedAndRefresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void NoStress()
        {
            if (_mainWindow.data == null) return;
            try
            {
                FieldInfo playerField = _mainWindow.data.GetType().GetField("player", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (playerField == null) return;
                object playerObj = playerField.GetValue(_mainWindow.data);

                FieldInfo paramsField = playerObj.GetType().GetField("standardParams", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (paramsField == null) return;

                float[] paramsArr = (float[])paramsField.GetValue(playerObj);

                if (paramsArr != null && paramsArr.Length > 7)
                {
                    paramsArr[6] = 0; // STRESS
                    _mainWindow.MarkAsChangedAndRefresh();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // SHARED AFFECTION LOGIC

        private void MaximizeCharacterAffection(object character)
        {
            if (character == null) return;

            Type charType = character.GetType();

            FieldInfo lovePointField = charType.GetField("lovePoint", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            FieldInfo friendPointField = charType.GetField("friendPoint", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            FieldInfo emotionStateField = charType.GetField("emotionState", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            FieldInfo intimatePointField= charType.GetField("intimatePoint", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (lovePointField != null) lovePointField.SetValue(character, 255);
            if (friendPointField != null) friendPointField.SetValue(character, 255);
            if (intimatePointField != null) intimatePointField.SetValue(character, 400);

            if (emotionStateField != null)
            {
                object loveEnumValue = Enum.ToObject(emotionStateField.FieldType, 5);
                emotionStateField.SetValue(character, loveEnumValue);
            }

            Debug.WriteLine($"Updated character: Love={lovePointField?.GetValue(character)}, Friend={friendPointField?.GetValue(character)}, EmotionState={emotionStateField?.GetValue(character)}");
        }

        private void MaximizeAllInArray(string arrayFieldName)
        {
            if (_mainWindow.data == null) return;

            try
            {
                FieldInfo arrayField = _mainWindow.data.GetType().GetField(arrayFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (arrayField == null) return;

                Array charsArray = (Array)arrayField.GetValue(_mainWindow.data);
                if (charsArray == null) return;

                foreach (object character in charsArray)
                {
                    MaximizeCharacterAffection(character);
                }

                _mainWindow.MarkAsChangedAndRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MaximizeSpecificInArray(string arrayFieldName, int selectedIndex)
        {
            if (_mainWindow.data == null || selectedIndex < 0) return;

            try
            {
                FieldInfo arrayField = _mainWindow.data.GetType().GetField(arrayFieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (arrayField == null) return;

                Array charsArray = (Array)arrayField.GetValue(_mainWindow.data);
                if (charsArray == null || selectedIndex >= charsArray.Length) return;

                object character = charsArray.GetValue(selectedIndex);

                MaximizeCharacterAffection(character);

                _mainWindow.MarkAsChangedAndRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // BUTTON EVENTS

        private void ButtonUnlockAllClothes_Click(object sender, RoutedEventArgs e)
        {
            UnlockAllClothes();
        }

        private void ButtonMaxStats_Click(object sender, RoutedEventArgs e)
        {
            MaxAllStats();
        }

        private void ButtonMaxMoney_Click(object sender, RoutedEventArgs e)
        {
            MaxMoney();
        }

        private void ButtonMaxAffection_Click(object sender, RoutedEventArgs e)
        {
            MaximizeAllInArray("approachChars");
        }

        private void ButtonMaxAffectionSpecific_Click(object sender, RoutedEventArgs e)
        {
            MaximizeSpecificInArray("approachChars", ComboApproachCharSelect.SelectedIndex);
        }

        private void ButtonMaxFriendAffection_Click(object sender, RoutedEventArgs e)
        {
            MaximizeAllInArray("friendChars");
        }

        private void ButtonMaxFriendAffectionSpecific_Click(object sender, RoutedEventArgs e)
        {
            MaximizeSpecificInArray("friendChars", ComboFriendCharSelect.SelectedIndex);
        }

        private void ButtonNoStress_Click(object? sender, RoutedEventArgs e)
        {
            NoStress();
        }
    }
}