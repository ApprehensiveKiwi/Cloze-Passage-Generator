using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Office.Interop.Word;
using System.Drawing;
using static System.Collections.Specialized.BitVector32;
using Application = Microsoft.Office.Interop.Word.Application;
using Range = Microsoft.Office.Interop.Word.Range;
using Section = Microsoft.Office.Interop.Word.Section;
using System.Runtime.CompilerServices;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Drawing.Printing;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using Table = Microsoft.Office.Interop.Word.Table;

namespace ASS_2025
{
    public struct doolean
    {

    }
    public partial class clozeSettings : System.Windows.Window
    {
        string printPath = "";
        public MainWindow mainParent;
        public bool inSettings = true; // false == student view
        public static List<wordItem> wordBankValues = new();
        string rawPassageText = "";
        //BitmapImage image;
        Random r = new();
        public static int[] timeMappings = new int[]
        {
            60,300,600,900,1200,1800,3600
        }; 
        public clozeSettings()
        {
            InitializeComponent();
            Title = "Crabby Clozes";
        }     
        private void openPassageButton_Click(object sender, RoutedEventArgs e) // creates a dialog menu to choose a file and input it into the input box.
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                rawPassageText = File.ReadAllText(filename);
            }
            generationInputTextbox.Text = rawPassageText;
        }
        private void createStudentViewButton_Click(object sender, RoutedEventArgs e)  //creates an instance of the student view and swithces to it.
        {
            List<(string, bool)> Cloze = generateCloze(generationInputTextbox.Text, autoGenRadio.IsChecked, manualGenerationRadioButton.IsChecked, randomGenerationRadioButton.IsChecked, (int)randomOffsetSlider.Value, (int)missingWordIntervalSlider.Value);

            if (Cloze.Count < 2000)
            {
                if (Cloze.Count != 1)
                {
                    Console.WriteLine(Cloze.Count.ToString());
                    mainParent = new MainWindow(this, generateCloze(generationInputTextbox.Text, autoGenRadio.IsChecked, manualGenerationRadioButton.IsChecked, randomGenerationRadioButton.IsChecked, (int)randomOffsetSlider.Value, (int)missingWordIntervalSlider.Value), (unlimitedTimeAllowedRadio.IsChecked == true) ? 0 : timeMappings[timeSelectionComboBox.SelectedIndex], (userNameInputTextbox.Text == null) ? "Default User" : userNameInputTextbox.Text, sortMethodComboBox.SelectedIndex);
                    Visibility = Visibility.Hidden;
                    mainParent.Show();
                }
            }
            else
            {
                MessageBox.Show($"Cloze passage must be under 2000 words to ensure responsiveness\nCurrently at {Cloze.Count}/2000 words");
            }
        }
        private void missingWordIntervalSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try // try is here since function fires when program is first launched fsr TODO FIX
            {
                wordIntervalDisplayText.Text = Math.Round(missingWordIntervalSlider.Value).ToString();
            }
            catch { }
        }
        public void refreshValues()  // refreshes cloze passage preview and word bank
        {
            List<(string, bool)> Cloze = generateCloze(generationInputTextbox.Text, autoGenRadio.IsChecked, manualGenerationRadioButton.IsChecked, randomGenerationRadioButton.IsChecked, (int)randomOffsetSlider.Value, (int)missingWordIntervalSlider.Value);
            if (Cloze.Count > 2000)
            {
                MessageBox.Show($"Cloze passage must be under 2000 words to ensure responsiveness\nCurrently at {Cloze.Count}/2000 words");
            }
            else
            {
                inputClozeValues(Cloze, generationPreviewTextbox, this, wordBankPreview);
                inputWordBankValues(Cloze, wordBankPreview, sortMethodComboBox.SelectedIndex);
            }
        }
        public static void removeWordBankValue(string s,WrapPanel container)
        {
            //List<wordItem> buffer = new();
            for (int i = 0; i < container.Children.Count; i++)  // iterates over all controls in wordbank control
            {
                if (container.Children[i].GetType() == typeof(wordItem)) // fires when control is a wordbank entry
                {
                    wordItem wI_ = (wordItem)container.Children[i];
                    if (wI_.Tag.ToString() == s) // "s" is the index to be removed, assigned on creation of wordbank
                    {
                        container.Children.Remove(wI_);
                        Globals.wordBankLengthProps.length--;
                        if (Globals.wordBankLengthProps.length == 0) MessageBox.Show($"Cloze Passage Has been sucessfully completed! You can now safely exit the program.");
                        
                    }
                }
            }
        }
        public static List<(string, bool)> generateCloze(string clozeInput,bool? autoGeneration, bool? manualGeneration,bool? randomGeneration,int randomGenerateOffset, int passageInterval)
        {
            clozeInput = Regex.Replace(clozeInput.Trim(), "[?!.,]", "");
            Random r = new();
            List<(string, bool)> betterData = new();
            string[] clozeBuffer = Regex.Replace(clozeInput, @"\t|\n|\r", "").Split(" ");
            if (randomGeneration == true) // random generation
            {
                for (int i = 0; i < clozeBuffer.Length; i++)
                {
                    betterData.Add((clozeBuffer[i], (r.Next(0, randomGenerateOffset) < r.Next(0,100)) ? true : false));
                }
            }
            else if (autoGeneration == true)  // auto generation (every N words)
            {              
                for (int i = 0; i < clozeBuffer.Length; i++)
                {
                    betterData.Add((clozeBuffer[i], i % passageInterval != 0));
                }
            }
            
            else if (manualGeneration == true)  // manual generation (the <quick> brown <fox> == the _____ brown ___)
            {
                bool visible = true;
                for (int i = 0; i < clozeBuffer.Length; i++)
                {
                    if ((clozeBuffer[i].Contains('<')))
                    {
                        visible = false;
                    }
                    betterData.Add((clozeBuffer[i], visible));
                    if ((clozeBuffer[i].Contains('>')))
                    {
                        visible = true;
                    }
                    betterData[i] = (betterData[i].Item1.Trim('<','>'), betterData[i].Item2);
                }
            }
            return betterData;
        }
        private void previewGenerateButton_Click(object sender, RoutedEventArgs e)
        {
            refreshValues();
        }   
        public static void inputClozeValues(List<(string value, bool visible)> input, WrapPanel container, System.Windows.Window this_,WrapPanel wordBank)
        { // takes in list of cloze values and adds them to viewmodel
            Globals.wordBankLengthProps = new();
            container.Children.Clear();
            for (int i = 0; i < input.Count; i++)
            {
                if ((input[i]).visible)
                {
                    container.Children.Add(new TextBlock
                    {
                        Text = $"{input[i].value} "
                    });
                }
                else
                {
                    container.Children.Add(new hiddenTextbox(input[i].value, i, wordBank)
                    {
                        Tag = i
                    });
                    Globals.wordBankLengthProps.maxLength++;
                }            
            }
            Globals.wordBankLengthProps.length = Globals.wordBankLengthProps.maxLength;
        }
        public static (string, bool)[] sortWordBank(List<(string,bool)> input, int sortMethodIndex)
        {
            Random r = new();
            (string,bool)[] inputArr = input.ToArray();
            string[] sortMethods = new string[] // visualisation to easily access sort method by index
            {
                "NoSort",
                "A->Z",
                "Z->A",
                "Pseudo",
                "Length H->L",
                "Length L->H",
            };
            switch (sortMethods[sortMethodIndex])
            {
                case "NoSort":
                    return inputArr;
                case "A->Z":
                    return bubbleSortAlpha(inputArr);
                case "Z->A":
                    return reverseArr(bubbleSortAlpha(inputArr));
                case "Pseudo":
                    return randomSort(inputArr);
                case "Length H->L":
                    return reverseArr(bubbleSortLength(inputArr));
                case "Length L->H":
                    return bubbleSortLength(inputArr);
                default:
                    return inputArr;
            };
            (string, bool)[] reverseArr((string, bool)[] revInput) // reverses data indexes ( min -> max)
            {
                for (int i = 0; i < revInput.Length / 2; i++)
                {
                    (string, bool) tmp = revInput[i];
                    revInput[i] = revInput[revInput.Length - i - 1];
                    revInput[revInput.Length - i - 1] = tmp;
                }
                return revInput;
            }
            (string, bool)[] randomSort((string,bool)[] randInput, int passes = 5)
            {
                (string,bool) temp = new();
                
                for(int i = 0; i < passes * randInput.Length; i++)
                {
                    int r1 = r.Next(0,randInput.Length);
                    int r2 = r.Next(0, randInput.Length);

                    temp = randInput[r1];
                    randInput[r1] = randInput[r2];
                    randInput[r2] = temp;
                }
                return randInput;               
            }
            (string,bool)[] bubbleSortAlpha((string,bool)[] bubInput) // implementation of bubblesort based on integer values of characters
            {
                bool sorted = false;
                while (!sorted)
                {
                    bool swapHappened = false;
                    (string,bool) temp = new();
                    for (int i = 0; i < bubInput.Length; i++)
                    {
                        if (int.Parse(bubInput[i].Item1) > int.Parse(bubInput[Math.Clamp(i + 1, 0, bubInput.Length - 1)].Item1))
                        {
                            temp = bubInput[i + 1];
                            bubInput[i + 1] = bubInput[i];
                            bubInput[i] = temp;
                            swapHappened = true;
                        }
                    }
                    if (!swapHappened) sorted = true;
                }
                return bubInput;
            }
            (string, bool)[] bubbleSortLength((string, bool)[] bubInput)
            {
                bool sorted = false;
                while (!sorted)
                {
                    bool swapHappened = false;
                    (string, bool) temp = new();
                    for (int i = 0; i < bubInput.Length; i++)
                    {
                        if ((bubInput[i].Item1.Length) > (bubInput[Math.Clamp(i + 1, 0, bubInput.Length - 1)].Item1.Length))
                        {
                            temp = bubInput[i + 1];
                            bubInput[i + 1] = bubInput[i];
                            bubInput[i] = temp;
                            swapHappened = true;
                        }
                    }
                    if (!swapHappened) sorted = true;
                }
                return bubInput;
            }
        }
        public static void inputWordBankValues(List<(string value, bool visible)> input, WrapPanel container, int sortMethodIndex)
        {
            try
            {
                wordBankValues.Clear();
                container.Children.Clear();
                (string, bool)[] buffer = sortWordBank(input,sortMethodIndex);
                int counter = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (!buffer[i].Item2)
                    {
                        wordBankValues.Add(new wordItem(buffer[i].Item1)
                        {                  
                            Tag = i
                        });
                        container.Children.Add(wordBankValues[counter]);
                        counter++;
                    }
                }
            }
            catch { }
        }
        private void inputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            inputClozeValues(generateCloze(inputTextbox.Text, false, true, false, -1, -1), outputWrapPanel,this, wordBankPreview);
        }
        private void randomOffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                randomIntervalDisplayText.Text = $"{((randomOffsetSlider.Value+0.001)/2).ToString("#")}%";
            }
            catch { }
        }
        private void sortMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inputWordBankValues(generateCloze(generationInputTextbox.Text, autoGenRadio.IsChecked, manualGenerationRadioButton.IsChecked, randomGenerationRadioButton.IsChecked, (int)randomOffsetSlider.Value, (int)missingWordIntervalSlider.Value), wordBankPreview, sortMethodComboBox.SelectedIndex);
        }
        private void saveClozeButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Docx files (*.docx)|*.doc|All files (*.*)|*.*",
            };

            if (dialog.ShowDialog() == true)
            {
                CreateDocument(generateCloze(generationInputTextbox.Text, autoGenRadio.IsChecked, manualGenerationRadioButton.IsChecked, randomGenerationRadioButton.IsChecked, (int)randomOffsetSlider.Value, (int)missingWordIntervalSlider.Value), dialog.FileName,(alternateCharacterCheckbox.IsChecked == true) ? @"_.-/\"[alternateCharacterCombobox.SelectedIndex] : '_');
            }
        }
        private void CreateDocument(List<(string, bool)> betterData, string path, char clozeChar)
        {
            refreshValues();
            string cloze = "";
            for (int i = 0; i < betterData.Count; i++)
            {
                cloze = $"{cloze} {((betterData[i].Item2) ? betterData[i].Item1 : new string(clozeChar, betterData[i].Item1.Length))}";
            }
            string wordBank = "";
            for (int i = 0; i < betterData.Count; i++)
            {
                wordBank = $"{wordBank} {((!betterData[i].Item2) ? $"• {betterData[i].Item1}," : "")}".Trim(',');
            }
            try
            {
                Application winword = new Application();
                winword.ShowAnimation = false;
                winword.Visible = false;
                object missing = System.Reflection.Missing.Value;
                Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);
                foreach (Section section in document.Sections)
                {
                    Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
                    headerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphDistribute;
                    headerRange.Font.ColorIndex = WdColorIndex.wdBlack;
                    headerRange.Font.Name = "Bahnschrift";
                    headerRange.Font.Bold = 1;
                    headerRange.Font.Size = 11;
                    headerRange.Text = $"Crabby-Clozes {DateTime.Now.ToString("MM/dd/yyyy")}";
                }
                foreach (Section wordSection in document.Sections)
                {
                    Range footerRange = wordSection.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    footerRange.Font.ColorIndex = WdColorIndex.wdBlack;
                    footerRange.Font.Size = 10;
                    footerRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    footerRange.Text = "Default User";
                }
                Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
                object styleHeading1 = "Title";
                para1.Range.set_Style(ref styleHeading1);
                para1.Range.Text = "Crabby Clozes";
                para1.Range.InsertParagraphAfter();
                Microsoft.Office.Interop.Word.Paragraph para2 = document.Content.Paragraphs.Add(ref missing);
                object style2 = "Normal";
                para2.Range.set_Style(ref style2);
                para2.Range.Text = cloze;
                para2.Range.InsertParagraphAfter();
                Table firstTable = document.Tables.Add(para1.Range, 2, 1, ref missing, ref missing);
                firstTable.Borders.Enable = 1;
                foreach (Row row in firstTable.Rows)
                {
                    foreach (Cell cell in row.Cells)
                    {
                        if (cell.RowIndex == 1)
                        {
                            cell.Range.Text = "Wordbank";
                            cell.Range.Font.Bold = 1;
                            cell.Range.Font.Name = "verdana";
                            cell.Range.Font.Size = 10;
                            cell.Shading.BackgroundPatternColor = WdColor.wdColorGray25;
                            cell.VerticalAlignment = WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        }
                        else
                        {
                            cell.Range.Text = wordBank;
                        }
                    }
                }
                document.SaveAs2(path);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;
                printPath = path;
                MessageBox.Show($"Document created successfully!\n@\n{path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            if (printPath != "")
            {
                PrintDialog pDialog = new PrintDialog();
                pDialog.PageRangeSelection = PageRangeSelection.AllPages;
                pDialog.UserPageRangeEnabled = true;
                Nullable<Boolean> print = pDialog.ShowDialog();
                if (print == true)
                {
                    XpsDocument xpsDocument = new XpsDocument(printPath, FileAccess.ReadWrite);
                    FixedDocumentSequence fixedDocSeq = xpsDocument.GetFixedDocumentSequence();
                    pDialog.PrintDocument(fixedDocSeq.DocumentPaginator, "Test print job");
                }
            }
            else MessageBox.Show("No document has been saved in the current instance, use the button above to generate a document before attempting to print.");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            helpContainer.Visibility = (helpContainer.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}