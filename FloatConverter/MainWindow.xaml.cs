using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
// 浮動小数点数変換
// 2019/04/9 一応完成
namespace FloatConverter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            radioButtonSingle.IsChecked = true;
            this.KeyDown += KeyDownHandler;
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBoxDecimal), textBoxDecimal);
        }
        // メッセージ表示
        private void DispMsg(string msg)
        {
            textBoxMsg.Foreground = Brushes.Black;
            textBoxMsg.Text = msg;
        }
        // エラーメッセージ表示
        private void ErrMsg(string msg)
        {
            textBoxMsg.Foreground = Brushes.Red;
            textBoxMsg.Text = msg;
        }
        // ラジオボタンクリック（サイズ変更）
        // XAMLで初期値を設定すると起動直後にここに来てラベルの変更でエラーになります。
        private void radioButtonChecked(object sender, RoutedEventArgs e)
        {
            SizeChange();
        }
        // サイズ変更
        private void SizeChange()
        {
            if (radioButtonSingle.IsChecked == true)
            {
                labelBinExponent.Content = "Exponent (8bits)";
                labelBinFraction.Content = "Fraction (23bits)";
            }
            else
            {
                labelBinExponent.Content = "Exponent (11bits)";
                labelBinFraction.Content = "Fraction (52bits)";
            }
            TryDecimalParse(true);
        }
        // 十進数解析（再表示）
        private bool TryDecimalParse(bool bSizeChange)
        {
            if (radioButtonSingle.IsChecked == true)
            {
                float f;
                if (float.TryParse(textBoxDecimal.Text, out f))
                {
                    textBoxDecimal.Text = f.ToString();
                    byte[] byteArray = BitConverter.GetBytes(f);
                    UInt32 ui32 = BitConverter.ToUInt32(byteArray, 0);
                    textBoxHexadecimal.Text = ui32.ToString("X8");
                    PrintBinaryFloat(ui32);
                    return (true);
                }
            }
            else
            {
                double d;
                if (double.TryParse(textBoxDecimal.Text, out d))
                {
                    textBoxDecimal.Text = d.ToString();
                    byte[] byteArray = BitConverter.GetBytes(d);
                    UInt64 ui64 = BitConverter.ToUInt64(byteArray, 0);
                    textBoxHexadecimal.Text = ui64.ToString("X16");
                    PrintBinaryDouble(byteArray);
                    return (true);
                }
            }
            if (bSizeChange && (textBoxDecimal.Text.Length == 0)) return (true);
            textBoxDecimal.Foreground = Brushes.Red;
            ErrMsg((textBoxDecimal.Text.Length == 0) ? "未入力です。" : "浮動小数点数として誤りがあります。");
            return (false);
        }
        // １６進数解析（再表示）
        private bool TryHexadecimalParse()
        {
            string str = textBoxHexadecimal.Text.Replace(" ", "");
            if (str.Length < 1)
            {
                ErrMsg("未入力です。");
                return (false);
            }
            if ((radioButtonSingle.IsChecked == true) && (str.Length > 8))
            {
                ErrMsg("８桁以内で入力してください。");
                return (false);
            }
            if (str.Length > 16)
            {
                ErrMsg("１６桁以内で入力してください。");
                return (false);
            }
            if (radioButtonSingle.IsChecked == true)
            {
                UInt32 ui32 = Convert.ToUInt32(str, 16);
                textBoxHexadecimal.Text = ui32.ToString("X8");
                byte[] byteArray = BitConverter.GetBytes(ui32);
                float f = BitConverter.ToSingle(byteArray, 0);
                textBoxDecimal.Text = f.ToString();
                PrintBinaryFloat(ui32);
            }
            else
            {
                UInt64 ui64 = Convert.ToUInt64(str, 16);
                textBoxHexadecimal.Text = ui64.ToString("X16");
                byte[] byteArray = BitConverter.GetBytes(ui64);
                double d = BitConverter.ToDouble(byteArray, 0);
                textBoxDecimal.Text = d.ToString();
                PrintBinaryDouble(byteArray);
            }
            return (true);
        }
        // バイナリ部分の表示（float）
        private void PrintBinaryFloat(UInt32 ui32)
        {
            textBoxBinSign.Text = ((ui32 & 0x80000000) == 0) ? "0" : "1";
            ui32 |= 0x80000000;
            string strBin = Convert.ToString(ui32, 2);
            textBoxBinExponent.Text = strBin.Substring(1, 8);
            textBoxBinFraction.Text = strBin.Substring(9);
        }
        // バイナリ部分の表示（double）
        private void PrintBinaryDouble(byte[] byteArray)
        {
            // string strBin = Convert.ToString(ui64, 2); ← これがエラーになる！
            string strBin =
                Convert.ToString((byteArray[7] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[6] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[5] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[4] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[3] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[2] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[1] + 0x100), 2).Substring(1) +
                Convert.ToString((byteArray[0] + 0x100), 2).Substring(1);
            textBoxBinSign.Text = strBin.Substring(0, 1);
            textBoxBinExponent.Text = strBin.Substring(1, 11);
            textBoxBinFraction.Text = strBin.Substring(12);
        }
        // KeyDownイベントハンドラ
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            // キー入力されたタイミングでエラーを解除する。
            textBoxDecimal.Foreground = Brushes.Black;
            textBoxHexadecimal.Foreground = Brushes.Black;
            DispMsg("");
            // ラジオボタン？
            var radioButton = FocusManager.GetFocusedElement(this) as RadioButton;
            if ((radioButton == radioButtonSingle) || (radioButton == radioButtonDouble))
            {
                if (e.Key == Key.Enter)
                {
                    RadioButton currentChecked = (bool)radioButtonSingle.IsChecked ? radioButtonSingle : radioButtonDouble;
                    radioButton.IsChecked = true;
                    if (radioButton != currentChecked)
                    {
                        SizeChange();
                    }
                }
                return;
            }
            // テキストボックス？
            var textBox = FocusManager.GetFocusedElement(this) as TextBox;
            if (textBox == null) return;
            if (textBox == textBoxDecimal)
            {
                if (e.Key == Key.Enter)
                {
                    if (TryDecimalParse(false))
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBoxHexadecimal), textBoxHexadecimal);
                        textBoxHexadecimal.CaretIndex = textBoxHexadecimal.Text.Length;
                    }
                    else
                    {
                        textBoxDecimal.Foreground = Brushes.Red;
                    }
                }
                return;
            }
            if (textBox == textBoxHexadecimal)
            {
                if (e.Key == Key.Enter)
                {
                    if (TryHexadecimalParse())
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBoxDecimal), textBoxDecimal);
                        textBoxDecimal.CaretIndex = textBoxDecimal.Text.Length;
                    }
                    else
                    {
                        textBoxHexadecimal.Foreground = Brushes.Red;
                    }
                    return;
                }
                // １６進数（0-9,A-F,a-f）は許す。
                if (((e.Key >= Key.D0) && (e.Key <= Key.D9)) ||
                    ((e.Key >= Key.NumPad0) && (e.Key <= Key.NumPad9)) ||
                    ((e.Key >= Key.A) && (e.Key <= Key.F)))
                {
                    return;
                }
                if (e.Key == Key.Tab) return;
                e.Handled = true;  // １６進数とタブ以外は無視！
            }
        }
    }
}