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
using System.Windows.Threading;

namespace Crestiki_noloki
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPlayerTurn = true;
        private char[,] board = new char[3, 3];
        private Button[,] buttons = new Button[3, 3];
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            InitializeButtonsArray();
            NewGame();
        }

        private void InitializeButtonsArray()
        {
            buttons[0, 0] = btn00; buttons[0, 1] = btn01; buttons[0, 2] = btn02;
            buttons[1, 0] = btn10; buttons[1, 1] = btn11; buttons[1, 2] = btn12;
            buttons[2, 0] = btn20; buttons[2, 1] = btn21; buttons[2, 2] = btn22;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e) => NewGame();

        private void NewGame()
        {
            isPlayerTurn = true;
            board = new char[3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    buttons[i, j].Content = "";
                    buttons[i, j].IsEnabled = true;
                    buttons[i, j].Background = Brushes.White;
                    buttons[i, j].Foreground = Brushes.Black;
                }

            StatusTB.Text = "Ваш ход (X)";
            StatusTB.Foreground = Brushes.Black;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayerTurn) return;

            Button button = (Button)sender;
            string[] coordinates = button.Tag.ToString().Split(',');
            int row = int.Parse(coordinates[0]);
            int col = int.Parse(coordinates[1]);

            if (board[row, col] != '\0') return;

            MakeMove(row, col, 'X');

            if (CheckWin('X'))
            {
                StatusTB.Text = "Вы победили!";
                StatusTB.Foreground = Brushes.Green;
                DisableAllButtons();
                return;
            }

            if (IsBoardFull())
            {
                StatusTB.Text = "Ничья!";
                StatusTB.Foreground = Brushes.Blue;
                return;
            }

            isPlayerTurn = false;
            StatusTB.Text = "Ход бота (O)";

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                BotMove();
            };
            timer.Start();
        }

        private void MakeMove(int row, int col, char symbol)
        {
            board[row, col] = symbol;
            buttons[row, col].Content = symbol.ToString();
            buttons[row, col].IsEnabled = false;
        }

        private void BotMove()
        {
            int row = -1, col = -1;

            if (random.Next(0, 2) == 0)
                (row, col) = FindBestMove(2);
            else
                (row, col) = FindRandomMove();

            if (row != -1 && col != -1)
            {
                MakeMove(row, col, 'O');

                if (CheckWin('O'))
                {
                    StatusTB.Text = "Противник победил!";
                    StatusTB.Foreground = Brushes.Red;
                    DisableAllButtons();
                    return;
                }

                if (IsBoardFull())
                {
                    StatusTB.Text = "Ничья!";
                    StatusTB.Foreground = Brushes.Blue;
                    return;
                }

                isPlayerTurn = true;
                StatusTB.Text = "Ваш ход (X)";
            }
        }

        private (int row, int col) FindRandomMove()
        {
            int row, col, attempts = 0;
            do
            {
                row = random.Next(0, 3);
                col = random.Next(0, 3);
                attempts++;
                if (attempts > 20)
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            if (board[i, j] == '\0') return (i, j);
            } while (board[row, col] != '\0');
            return (row, col);
        }

        private (int row, int col) FindBestMove(int maxDepth)
        {
            int bestScore = int.MinValue, bestRow = -1, bestCol = -1;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == '\0')
                    {
                        board[i, j] = 'O';
                        int score = Minimax(board, 0, false, maxDepth);
                        board[i, j] = '\0';
                        if (score > bestScore) { bestScore = score; bestRow = i; bestCol = j; }
                    }
            return (bestRow, bestCol);
        }

        private int Minimax(char[,] board, int depth, bool isMaximizing, int maxDepth)
        {
            if (CheckWin('O')) return 1;
            if (CheckWin('X')) return -1;
            if (IsBoardFull()) return 0;
            if (depth >= maxDepth) return 0;

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (board[i, j] == '\0')
                        {
                            board[i, j] = 'O';
                            bestScore = Math.Max(bestScore, Minimax(board, depth + 1, false, maxDepth));
                            board[i, j] = '\0';
                        }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (board[i, j] == '\0')
                        {
                            board[i, j] = 'X';
                            bestScore = Math.Min(bestScore, Minimax(board, depth + 1, true, maxDepth));
                            board[i, j] = '\0';
                        }
                return bestScore;
            }
        }

        private bool CheckWin(char symbol)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == symbol && board[i, 1] == symbol && board[i, 2] == symbol) return true;
                if (board[0, i] == symbol && board[1, i] == symbol && board[2, i] == symbol) return true;
            }
            if (board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol) return true;
            if (board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol) return true;
            return false;
        }

        private bool IsBoardFull()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == '\0') return false;
            return true;
        }

        private void DisableAllButtons()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    buttons[i, j].IsEnabled = false;
        }
    }
}
