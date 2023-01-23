using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Analizator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            // проверяем строку на корректность
            Result result = Analyzer.Check(textBoxInput.Text);

            // сообщаем о результате
            labelResult.Text = "Результат: " + result.ErrMessage;
            if (result.ErrPosition != -1)
            {
                textBoxInput.SelectionStart = result.ErrPosition;
                textBoxInput.Focus();
            }
            if (result.ErrMessage == "Нет ошибок.")
            {
                // считаем число итераций цикла
                if (result.CountOfIteration > 0)
                {
                    labelCountOfIteration.Text = "Число итераций цикла: " + result.CountOfIteration;
                }
                
                int count = 0;
                
                // выводим идентификаторы
                labelId.Text = "Идентификаторы\n\n";
                foreach (string st in result.ListId)
                {                   
                    if (count % 2 == 0)
                        labelId.Text += st;
                    else
                        labelId.Text += st + "\n";
                    count++;
                }
                count = 0;
                // выводим константы
                labelConstants.Text = "Константы\n\n";
                foreach (string st in result.ListConst)
                {
                    if (count % 2 == 0)
                        labelConstants.Text += st;
                    else
                        labelConstants.Text += st + "\n";
                    count++;
                }
            }
            else
            {
                labelId.Text = null;
                labelConstants.Text = null;
                labelCountOfIteration.Text = null;
            }
            result.ListId.Clear();
            result.ListConst.Clear();

        }

        private void labelId_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void labelConstants_Click(object sender, EventArgs e)
        {

        }
    }
}
