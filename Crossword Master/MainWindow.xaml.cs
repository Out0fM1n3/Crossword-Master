using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//  Test
//  Десятый месяц в году - октябрь, Большой кубик - куб, Кисло-сладкий плод садового дерева - яблоко, Зодиакальное созвездие - рак
//  Октябрь, Куб, Яблоко, Рак
//  Test
namespace Crossword_Master
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] AnswerArray;
        string[] SortAnswerArray;
        string[] QuestionArray;
        private ListBox listbox;
        readonly int rows = 50;
        readonly int columns = 50;
        public MainWindow()
        {
            InitializeComponent();
            CheckButton.Visibility = Visibility.Collapsed;
        }
        private void Creategame()
        {
            var grid = GameGrid;

            // Создание сетки для разметки
            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < columns; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            // Заполнение поля объектами TextBox
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var textBox = new TextBox
                    {
                        Visibility = Visibility.Collapsed,
                    };

                    Grid.SetRow(textBox, i);
                    Grid.SetColumn(textBox, j);
                    grid.Children.Add(textBox);
                }
            }
        }

        private void ResizeGameGrid()
        {
            // Удаление пустых строк
            for (int i = GameGrid.RowDefinitions.Count - 1; i >= 0; i--)
            {
                var isRowEmpty = true;
                for (int j = 0; j < GameGrid.ColumnDefinitions.Count; j++)
                {
                    if (GameGrid.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j) != null)
                    {
                        isRowEmpty = false;
                        break;
                    }
                }
                if (isRowEmpty)
                {
                    GameGrid.RowDefinitions.RemoveAt(i);
                    foreach (var element in GameGrid.Children)
                    {
                        var row = Grid.GetRow((UIElement)element);
                        if (row > i)
                        {
                            Grid.SetRow((UIElement)element, row - 1);
                        }
                    }
                }
            }

            // Удаление пустых столбцов
            for (int i = GameGrid.ColumnDefinitions.Count - 1; i >= 0; i--)
            {
                var isColumnEmpty = true;
                for (int j = 0; j < GameGrid.RowDefinitions.Count; j++)
                {
                    if (GameGrid.Children.Cast<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == j && Grid.GetColumn(e) == i) != null)
                    {
                        isColumnEmpty = false;
                        break;
                    }
                }
                if (isColumnEmpty)
                {
                    GameGrid.ColumnDefinitions.RemoveAt(i);
                    foreach (var element in GameGrid.Children)
                    {
                        var column = Grid.GetColumn((UIElement)element);
                        if (column > i)
                        {
                            Grid.SetColumn((UIElement)element, column - 1);
                        }
                    }
                }
            }
            UpdateListBox();
        }

        private void GetDictionary()
        {
            if (QuestionDictionary_TextBox.Text != null)
            {
                string text = QuestionDictionary_TextBox.Text;
                QuestionArray = text.Split(',');
            }
            if (AnswerDictionary_TextBox.Text != null)
            {
                string text = AnswerDictionary_TextBox.Text.Replace(" ", "").ToLower();
                AnswerArray = text.Split(',');
                PlaceWordsOnGrid();
            }
        }
        private void PlaceWordsOnGrid()
        {
            // Определение размера сетки
            int rows = GameGrid.RowDefinitions.Count;
            int columns = GameGrid.ColumnDefinitions.Count;

            // Получение списка слов из массива array
            List<string> words = new List<string>(AnswerArray);

            // Сортировка слов по убыванию длины
            words.Sort((x, y) => y.Length.CompareTo(x.Length));
            SortAnswerArray = words.ToArray();

            // Размещение первого слова горизонтально в середине сетки
            string firstWord = words[0];
            int startColumn = (columns - firstWord.Length) / 2;

            // Добавление цифры со стрелкой перед первым словом
            PlaceNumberWithArrow(rows / 2, startColumn - 1, 1, true);

            for (int i = 0; i < firstWord.Length; i++)
            {
                PlaceLetterOnGrid(rows / 2, startColumn + i, firstWord[i]);
            }

            // Удаление первого слова из списка
            words.RemoveAt(0);

            // Счетчик для нумерации слов
            int wordCounter = 2;

            // Размещение оставшихся слов на сетке
            foreach (string word in words)
            {
                bool placed = false;
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        // Попытка разместить слово горизонтально на пересечении с другим словом
                        if (CanPlaceWordHorizontally(row, column, word) && HasIntersectionHorizontally(row, column, word))
                        {
                            // Добавление цифры со стрелкой перед словом
                            PlaceNumberWithArrow(row, column - 1, wordCounter++, true);

                            for (int i = 0; i < word.Length; i++)
                            {
                                PlaceLetterOnGrid(row, column + i, word[i]);
                            }
                            placed = true;
                            break;
                        }
                        // Попытка разместить слово вертикально на пересечении с другим словом
                        else if (CanPlaceWordVertically(row, column, word) && HasIntersectionVertically(row, column, word))
                        {
                            // Добавление цифры со стрелкой перед словом
                            PlaceNumberWithArrow(row - 1, column, wordCounter++, false);

                            for (int i = 0; i < word.Length; i++)
                            {
                                PlaceLetterOnGrid(row + i, column, word[i]);
                            }
                            placed = true;
                            break;
                        }
                    }
                    if (placed) break;
                }
            }
            // Очистка всех значений TextBox
            foreach (var child in GameGrid.Children)
            {
                if (child is TextBox textBox)
                {
                    textBox.Text = "";
                }
            }
            // Удаление всех TextBox с Visibility.Collapsed
            for (int i = GameGrid.Children.Count - 1; i >= 0; i--)
            {
                if (GameGrid.Children[i] is TextBox textBox && textBox.Visibility == Visibility.Collapsed)
                {
                    GameGrid.Children.RemoveAt(i);
                }
            }
            ResizeGameGrid();
        }

        // Метод для размещения цифры со стрелкой на сетке
        private void PlaceNumberWithArrow(int row, int column, int number, bool horizontal)
        {
            // Проверка выхода за границы сетки
            if (row < 0 || column < 0 || row >= GameGrid.RowDefinitions.Count || column >= GameGrid.ColumnDefinitions.Count) return;

            Label label = new Label
            {
                Content = number.ToString() + (horizontal ? "→" : "↓"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, column);
            GameGrid.Children.Add(label);
        }

        private void UpdateListBox()
        {
            var result = QuestionArray.Select(x => x.Split(new string[] { " - " }, StringSplitOptions.None))
                          .ToDictionary(x => x[1], x => x[0])
                          .OrderBy(x => Array.IndexOf(SortAnswerArray, x.Key))
                          .Select(x => x.Value)
                          .ToList();

            for (int index = 0; index <= result.Count - 1; index++)
            {
                listbox.Items.Add(index + 1 + ". " + result[index].Trim());
            }
            listbox.Visibility = Visibility.Visible;
        }

        // Метод для проверки возможности размещения слова горизонтально
        private bool CanPlaceWordHorizontally(int row, int column, string word)
        {
            // Проверка выхода за границы сетки
            if (column + word.Length > GameGrid.ColumnDefinitions.Count) return false;

            // Проверка пересечения с другими словами
            for (int i = 0; i < word.Length; i++)
            {
                TextBox textBox = GetTextBoxAt(row, column + i);
                if (textBox.Text != "" && textBox.Text != word[i].ToString()) return false;
            }

            return true;
        }

        // Метод для проверки наличия пересечения горизонтально размещаемого слова с другими словами
        private bool HasIntersectionHorizontally(int row, int column, string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                TextBox textBox = GetTextBoxAt(row, column + i);
                if (textBox.Text == word[i].ToString()) return true;
            }

            return false;
        }

        // Метод для проверки возможности размещения слова вертикально
        private bool CanPlaceWordVertically(int row, int column, string word)
        {
            // Проверка выхода за границы сетки
            if (row + word.Length > GameGrid.RowDefinitions.Count) return false;

            // Проверка пересечения с другими словами
            for (int i = 0; i < word.Length; i++)
            {
                TextBox textBox = GetTextBoxAt(row + i, column);
                if (textBox.Text != "" && textBox.Text != word[i].ToString()) return false;
            }

            return true;
        }

        // Метод для проверки наличия пересечения вертикально размещаемого слова с другими словами
        private bool HasIntersectionVertically(int row, int column, string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                TextBox textBox = GetTextBoxAt(row + i, column);
                if (textBox.Text == word[i].ToString()) return true;
            }

            return false;
        }

        // Метод для размещения буквы на сетке
        private void PlaceLetterOnGrid(int row, int column, char letter)
        {
            TextBox textBox = GetTextBoxAt(row, column);
            textBox.Visibility = Visibility.Visible;
            textBox.Text = letter.ToString();
            textBox.VerticalContentAlignment = VerticalAlignment.Center;
            textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            textBox.Background = Brushes.LightGray;
        }

        // Метод для получения объекта TextBox по координатам на сетке
        private TextBox GetTextBoxAt(int row, int column)
        {
            foreach (var child in GameGrid.Children)
            {
                if (Grid.GetRow(child as UIElement) == row && Grid.GetColumn(child as UIElement) == column)
                    return child as TextBox;
            }
            return null;
        }

        private void ListBoxCreate()
        {
            // Создание элемента listbox
            listbox = new ListBox
            {
                Visibility = Visibility.Collapsed
            };

            // Поиск индекса кнопки в StackPanel
            CheckButton.Visibility = Visibility.Visible;
            int buttonIndex = SecondStackPanel.Children.IndexOf(CheckButton);

            // Добавление элемента listbox перед кнопкой
            SecondStackPanel.Children.Insert(buttonIndex, listbox);
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAnswerTB())
            {
                MessageBox.Show("Похоже вы не заполнили поле для ответов", "Ошибка!");
                Debug.Write("1");
                return;
            }
            if (CheckQuestTB())
            {
                MessageBox.Show("Похоже вы не заполнили поле для вопросов", "Ошибка!");
                Debug.Write("2");
                return;
            }
            if (CheckQuestAnsTB())
            {
                MessageBox.Show("Похоже кол-во вопросов больше чем кол-во ответов.", "Ошибка!");
                Debug.Write("3");
                return;
            }
            if (CheckAnsQuestTB())
            {
                MessageBox.Show("Похоже кол-во ответов больше чем кол-во вопросов.", "Ошибка!");
                Debug.Write("4");
                return;
            }
            else
            {
                ListBoxCreate();
                HideObjects();
                Creategame();
                GetDictionary();
            }
        }

        private bool CheckAnswerTB()
        {
            if (AnswerDictionary_TextBox.Text == "")
            {
                return true;
            }
            return false;
        }
        private bool CheckQuestTB()
        {
            if (QuestionDictionary_TextBox.Text == "")
            {
                return true;
            }
            return false;
        }
        private bool CheckQuestAnsTB()
        {
            int questionCount = QuestionDictionary_TextBox.Text.Split(',').Length;
            int answerCount = AnswerDictionary_TextBox.Text.Split(',').Length;
            return questionCount > answerCount;
        }
        private bool CheckAnsQuestTB()
        {
            int answerCount = AnswerDictionary_TextBox.Text.Split(',').Length;
            int questionCount = QuestionDictionary_TextBox.Text.Split(',').Length;
            return answerCount > questionCount;
        }

        private void HideObjects()
        {
            lbl1.Visibility = Visibility.Collapsed;
            lbl2.Visibility = Visibility.Collapsed;
            QuestionDictionary_TextBox.Visibility = Visibility.Collapsed;
            AnswerDictionary_TextBox.Visibility = Visibility.Collapsed;
            PlayBtn.Visibility = Visibility.Collapsed;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Определение размера сетки
            int rows = GameGrid.RowDefinitions.Count;
            int columns = GameGrid.ColumnDefinitions.Count;
            // Перебор всех TextBox на сетке
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    TextBox textBox = GetTextBoxAt(row, column);
                    if (textBox != null)
                    {
                        // Проверка текста в TextBox
                        if (IsTextCorrect(row, column))
                        {
                            textBox.Background = Brushes.LightGreen;
                        }
                        else
                        {
                            textBox.Background = Brushes.PaleVioletRed;
                        }
                    }
                }
            }
        }

        // Метод для проверки правильности текста
        private bool IsTextCorrect(int row, int column)
        {
            // Определение размера сетки
            int rows = GameGrid.RowDefinitions.Count;
            int columns = GameGrid.ColumnDefinitions.Count;

            // Поиск горизонтального слова
            string horizontalWord = "";
            for (int i = column; i >= 0; i--)
            {
                TextBox textBox = GetTextBoxAt(row, i);
                if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                {
                    break;
                }
                horizontalWord = textBox.Text + horizontalWord;
            }
            for (int i = column + 1; i < columns; i++)
            {
                TextBox textBox = GetTextBoxAt(row, i);
                if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                {
                    break;
                }
                horizontalWord += textBox.Text;
            }

            // Проверка горизонтального слова
            if (AnswerArray.Contains(horizontalWord))
            {
                return true;
            }

            // Поиск вертикального слова
            string verticalWord = "";
            for (int i = row; i >= 0; i--)
            {
                TextBox textBox = GetTextBoxAt(i, column);
                if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                {
                    break;
                }
                verticalWord = textBox.Text + verticalWord;
            }
            for (int i = row + 1; i < rows; i++)
            {
                TextBox textBox = GetTextBoxAt(i, column);
                if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                {
                    break;
                }
                verticalWord += textBox.Text;
            }

            // Проверка вертикального слова
            return AnswerArray.Contains(verticalWord);
        }
    }
}
