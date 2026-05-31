using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace TMGSSaveEditor.Core
{
    public partial class BooleanControl : UserControl, MainWindow.ObjectInspector
    {
        MainWindow form1;
        MainWindow.ObjectInfo objectInfo;

        public BooleanControl()
        {
            InitializeComponent();
        }

        public void registerParent(MainWindow form)
        {
            form.onReset += Form_onReset;
            form.onHandleObject += Form_onHandleObject;
            form1 = form;
        }

        private void Form_onReset()
        {
            this.IsVisible = false;
            objectInfo = null;
        }

        private void Form_onHandleObject(MainWindow.ObjectInfo oi)
        {
            if (!(oi.obj is Boolean b))
            {
                return;
            }

            CheckBox1.Content = oi.fieldInfo.Name;
            CheckBox1.IsChecked = b;

            this.IsVisible = true;
            objectInfo = oi;
        }

        private void CheckBox1_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (objectInfo == null) return;
            form1.setObject(objectInfo, CheckBox1.IsChecked ?? false);
        }
    }
}