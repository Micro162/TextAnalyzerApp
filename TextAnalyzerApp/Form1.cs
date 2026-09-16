using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextAnalyzerApp
{
    public partial class Form1 : Form
    {
        private TextBox txtInput = null!;
        private Button btnAnalyze = null!;
        private RadioButton rbScreen = null!;
        private RadioButton rbFile = null!;

        private CheckBox chkSentences = null!;
        private CheckBox chkCharacters = null!;
        private CheckBox chkWords = null!;
        private CheckBox chkInterrogative = null!;
        private CheckBox chkExclamatory = null!;

        public Form1()
        {
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.Width = 600;
            this.Height = 500;
            this.Text = "Аналізатор тексту (Task Parallel Library)";

            txtInput = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Left = 20, Top = 20, Width = 540, Height = 150 };
            this.Controls.Add(txtInput);

            GroupBox gbOptions = new GroupBox { Text = "Включити у звіт:", Left = 20, Top = 180, Width = 260, Height = 150 };
            chkSentences = new CheckBox { Text = "Кількість речень", Left = 15, Top = 25, AutoSize = true, Checked = true };
            chkCharacters = new CheckBox { Text = "Кількість символів", Left = 15, Top = 50, AutoSize = true, Checked = true };
            chkWords = new CheckBox { Text = "Кількість слів", Left = 15, Top = 75, AutoSize = true, Checked = true };
            chkInterrogative = new CheckBox { Text = "Питальні речення", Left = 15, Top = 100, AutoSize = true, Checked = true };
            chkExclamatory = new CheckBox { Text = "Окличні речення", Left = 15, Top = 125, AutoSize = true, Checked = true };

            gbOptions.Controls.AddRange(new Control[] { chkSentences, chkCharacters, chkWords, chkInterrogative, chkExclamatory });
            this.Controls.Add(gbOptions);

            GroupBox gbOutput = new GroupBox { Text = "Спосіб виводу звіту:", Left = 300, Top = 180, Width = 260, Height = 100 };
            rbScreen = new RadioButton { Text = "Вивести на екран", Left = 15, Top = 30, AutoSize = true, Checked = true };
            rbFile = new RadioButton { Text = "Зберегти у файлик", Left = 15, Top = 60, AutoSize = true };
            gbOutput.Controls.AddRange(new Control[] { rbScreen, rbFile });
            this.Controls.Add(gbOutput);

            btnAnalyze = new Button { Text = "Проаналізувати", Left = 300, Top = 300, Width = 260, Height = 40 };
            btnAnalyze.Click += BtnAnalyze_Click;
            this.Controls.Add(btnAnalyze);
        }

        private async void BtnAnalyze_Click(object? sender, EventArgs e)
        {
            string text = txtInput.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Будь ласка, введіть текст для аналізу.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool includeSentences = chkSentences.Checked;
            bool includeCharacters = chkCharacters.Checked;
            bool includeWords = chkWords.Checked;
            bool includeInterrogative = chkInterrogative.Checked;
            bool includeExclamatory = chkExclamatory.Checked;

            btnAnalyze.Enabled = false;

            try
            {
                string report = await Task.Run(() => GenerateReport(text, includeSentences, includeCharacters, includeWords, includeInterrogative, includeExclamatory));

                if (rbScreen.Checked)
                {
                    MessageBox.Show(report, "Звіт аналізу тексту", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (rbFile.Checked)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, report);
                            MessageBox.Show("Звіт успішно збережено у файл!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAnalyze.Enabled = true;
            }
        }

        private string GenerateReport(string text, bool incSentences, bool incChars, bool incWords, bool incInterr, bool incExcl)
        {
            var reportLines = new List<string> { "=== ЗВІТ АНАЛІЗУ ТЕКСТУ ===" };

            if (incChars)
            {
                int charCount = text.Length;
                reportLines.Add($"• Кількість символів: {charCount}");
            }

            if (incWords)
            {
                int wordCount = Regex.Matches(text, @"\b\w+\b").Count;
                reportLines.Add($"• Кількість слів: {wordCount}");
            }

            var sentences = Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            if (incSentences)
            {
                reportLines.Add($"• Кількість речень: {sentences.Count}");
            }

            if (incInterr)
            {
                int interrogativeCount = sentences.Count(s => s.TrimEnd().EndsWith("?"));
                reportLines.Add($"• Кількість питальних речень: {interrogativeCount}");
            }

            if (incExcl)
            {
                int exclamatoryCount = sentences.Count(s => s.TrimEnd().EndsWith("!"));
                ReportLinesAddSafe(reportLines, $"• Кількість окличних речень: {exclamatoryCount}");
            }

            return string.Join(Environment.NewLine, reportLines);
        }

        private void ReportLinesAddSafe(List<string> list, string item)
        {
            list.Add(item);
        }
    }
}