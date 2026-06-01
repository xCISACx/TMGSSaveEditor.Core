using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Linq;

namespace TMGSSaveEditor.Core
{
    public partial class TextControl : UserControl, MainWindow.ObjectInspector
    {
        MainWindow form1;
        MainWindow.ObjectInfo objectInfo;

        // This flag stops the LostFocus event from ruining the Enter key event
        private bool _isSaving = false;

        public TextControl()
        {
            InitializeComponent();
        }

        public void registerParent(MainWindow form)
        {
            form.onReset += Form_onReset;
            form.onHandleObject += Form_onHandleObject;
            form1 = form;
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                ApplyTextValue();
            }
        }

        private void TextBox1_LostFocus(object? sender, Avalonia.Input.FocusChangedEventArgs e)
        {
            ApplyTextValue();
        }

        private void ApplyTextValue()
        {
            // If we are already in the middle of saving, abort!
            if (objectInfo == null || _isSaving) return;

            _isSaving = true;

            try
            {
                if (objectInfo.obj is string)
                {
                    form1.setObject(objectInfo, TextBox1.Text);
                }
                else if (IsNumber(objectInfo.obj))
                {
                    var w = objectInfo.obj.GetType();
                    var parseMethod = (from x in w.GetMethods()
                                       where x.Name == "Parse" && x.GetParameters().Length == 1
                                       select x).First();

                    Object newObj = parseMethod.Invoke(null, new object[] { TextBox1.Text });
                    form1.setObject(objectInfo, newObj);
                }
            }
            catch { }
            finally
            {
                // Once setObject is completely done, unlock the control
                _isSaving = false;
            }
        }

        private void Form_onReset()
        {
            this.IsVisible = false;
            objectInfo = null;
        }

        public static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        private void Form_onHandleObject(MainWindow.ObjectInfo oi)
        {
            if (oi.obj is string s)
            {
                objectInfo = oi;
                Label1.Text = oi.fieldInfo.Name;
                TextBox1.Text = s;
                this.IsVisible = true;
            }
            else if (IsNumber(oi.obj))
            {
                objectInfo = oi;
                Label1.Text = oi.fieldInfo.Name;
                TextBox1.Text = oi.obj.ToString();
                this.IsVisible = true;
            }
        }
    }
}