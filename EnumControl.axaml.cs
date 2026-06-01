using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;

namespace TMGSSaveEditor.Core
{
    public partial class EnumControl : UserControl, MainWindow.ObjectInspector
    {
        MainWindow form1;
        MainWindow.ObjectInfo objectInfo;
        ObservableCollection<object> enumItems = new ObservableCollection<object>();

        public EnumControl()
        {
            InitializeComponent();
            ComboBox1.ItemsSource = enumItems;
        }

        public void registerParent(MainWindow form)
        {
            form.onReset += Form_onReset;
            form.onHandleObject += Form_onHandleObject;
            form1 = form;
        }

        private void Form_onHandleObject(MainWindow.ObjectInfo objInfo)
        {
            if (objInfo.obj == null)
            {
                // Hide the editor panel since there is no data to edit
                this.IsVisible = false;
                return;
            }

            Type objType = objInfo.obj.GetType();
            if (!objType.IsEnum)
            {
                return;
            }

            Label1.Text = objInfo.fieldInfo.Name;
            var enumVals = objType.GetEnumValues();

            enumItems.Clear();
            foreach (var enumVal in enumVals)
            {
                enumItems.Add(enumVal);
            }

            ComboBox1.SelectedItem = objInfo.obj;
            objectInfo = objInfo;
            this.IsVisible = true;
        }

        private void Form_onReset()
        {
            this.IsVisible = false;
            objectInfo = null;

            // Clear selection before clearing list
            ComboBox1.SelectedIndex = -1;

            enumItems.Clear();
        }

        private void ComboBox1_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBox1.SelectedIndex == -1) return;

            if (objectInfo == null || ComboBox1.SelectedItem == null) return;
            form1.setObject(objectInfo, ComboBox1.SelectedItem);
        }
    }
}