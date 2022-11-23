using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMSimulator.Validators
{
    public static class Validator
    {
        static public bool IsValid(TextBox textBox, int Length,out string output)
        {
            output = string.Empty;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                return false;
            }
            if (Length != 0)
            {
                string text = textBox.Text.Trim();
                if (text.Length != Length)
                {
                    output = text.PadLeft(Length, char.Parse("0"));
                    return true;
                }
            }
            output = textBox.Text;
            return true;
        }
        static public bool ComboNull(ComboBox comboBox)
        {
            if (string.IsNullOrEmpty(comboBox.Text))
            {
                return false;
            }
            return true;
        }

        static public bool IsValidNumeric(TextBox textBox)
        {
            int parsedValue;
            if (!int.TryParse(textBox.Text, out parsedValue))
            {
                return false;
            }
            return true;
        }
        static public bool IsValidDecimal(TextBox textBox)
        {
            decimal parsedValue;
            if (!decimal.TryParse(textBox.Text, out parsedValue))
            {
                return false;
            }
            return true;
        }

        static public bool IsValidLong(TextBox textBox)
        {
            long parsedValue;
            if (!long.TryParse(textBox.Text, out parsedValue))
            {
                return false;
            }
            return true;
        }
    }
}
