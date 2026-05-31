using Avalonia.Controls;
using System;
using System.Reflection.Emit;

namespace TMGSSaveEditor.Core
{
    public partial class DateControl : UserControl, MainWindow.ObjectInspector
    {
        MainWindow form1;
        MainWindow.ObjectInfo objectInfo;

        public DateControl()
        {
            InitializeComponent();
        }

        public void registerParent(MainWindow form)
        {
            form.onReset += Form_onReset;
            form.onHandleObject += Form_onHandleObject;
            form1 = form;
        }

        private void Form_onHandleObject(MainWindow.ObjectInfo objInfo)
        {
            Type objType = objInfo.obj.GetType();
            if (objType != typeof(DateTime))
            {
                return;
            }

            Label1.Text = objInfo.fieldInfo.Name;

            DatePicker1.SelectedDate = (DateTime)objInfo.obj;

            objectInfo = objInfo;
            this.IsVisible = true;
        }

        private void Form_onReset()
        {
            this.IsVisible = false;
            objectInfo = null;
        }

        private void DatePicker1_SelectedDateChanged(object? sender, Avalonia.Controls.DatePickerSelectedValueChangedEventArgs e)
        {
            if (objectInfo == null || !DatePicker1.SelectedDate.HasValue) return;
            form1.setObject(objectInfo, DatePicker1.SelectedDate.Value.DateTime);
        }
    }
}