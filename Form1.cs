using System;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisWinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class Form1 : Form
    {
        private const int GridWidth = 10;
        private const int GridHeight = 20;
        private const int CellSize = 20;

        private int[,] field = new int[GridHeight, GridWidth];
        private int[] tetrominoes = { 0x0F00, 0x2222, 0x44C0, 0x8E00, 0x6440, 0xE200, 0x4E00 };
        private int currentTetromino, currentRotation, currentX, currentY;
        private Random rand = new Random();
        private Timer timer1 = new Timer();
        private int score = 0;

        public Form1()
        {
            this.DoubleBuffered = true;
            this.ClientSize = new Size(GridWidth * CellSize, GridHeight * CellSize);
            this.Text = "Tetris";

            StartNewGame();

            timer1.Interval = 500;
            timer1.Tick += Timer1_Tick;
            this.KeyDown += Form1_KeyDown;
            this.Paint += Form1_Paint;
            timer1.Start();
        }

        private void StartNewGame() // Почати нову гру
        {
            Array.Clear(field, 0, field.Length);
            score = 785;
            SpawnNewTetromino();
        }

        private void SpawnNewTetromino() // Створити новий тетроміно (фігуру)
        {
            currentTetromino = rand.Next(tetrominoes.Length);
            currentRotation = 0;
            currentX = GridWidth / 2 - 1;
            currentY = 0;

            if (!IsValidMove(currentTetromino, currentRotation, currentX, currentY))
            {
                timer1.Stop();
                MessageBox.Show($"Game Over!\nScore: {score}");
                Application.Exit();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e) // Обробник події таймера (кожен крок гри)
        {
            Drop();
            this.Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) // Обробник натискання клавіш (рух фігури)
        {
            if (e.KeyCode == Keys.Left) Move(-1);
            if (e.KeyCode == Keys.Right) Move(1);
            if (e.KeyCode == Keys.Up) Rotate();
            if (e.KeyCode == Keys.Down) Drop();
            this.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e) // Малювання форми (поле гри та фігури)
        {
            DrawField(e.Graphics);
            DrawTetromino(e.Graphics);
        }

        private void DrawField(Graphics g) // Малювання поля гри
        {
            g.Clear(Color.Black);
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    if (field[y, x] != 0)
                    {
                        g.FillRectangle(Brushes.Blue, x * CellSize, y * CellSize, CellSize, CellSize);
                        g.DrawRectangle(Pens.Black, x * CellSize, y * CellSize, CellSize, CellSize);
                    }
                }
            }
        }

        private void DrawTetromino(Graphics g)  // Малювання тетроміно (фігури)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((tetrominoes[currentTetromino] & (1 << (i * 4 + j))) != 0)
                    {
                        g.FillRectangle(Brushes.Red, (currentX + j) * CellSize, (currentY + i) * CellSize, CellSize, CellSize);
                        g.DrawRectangle(Pens.Black, (currentX + j) * CellSize, (currentY + i) * CellSize, CellSize, CellSize);
                    }
                }
            }
        }

        private void Move(int dx) // Рух фігури вліво або вправо
        {
            if (IsValidMove(currentTetromino, currentRotation, currentX + dx, currentY))
            {
                currentX += dx;
            }
        }

        private void Rotate() // Обертання фігури
        {
            int newRotation = (currentRotation + 1) % 4;
            if (IsValidMove(currentTetromino, newRotation, currentX, currentY))
            {
                currentRotation = newRotation;
            }
        }

        private void Drop() // Падіння фігури вниз
        {
            if (IsValidMove(currentTetromino, currentRotation, currentX, currentY + 1))
            {
                currentY++;
            }
            else
            {
                PlaceTetromino();
                ClearLines();
                SpawnNewTetromino();
            }
        }

        private bool IsValidMove(int tetromino, int rotation, int x, int y)  // Перевірка чи можливий хід для фігури
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((tetrominoes[tetromino] & (1 << (i * 4 + j))) != 0)
                    {
                        int newX = x + j;
                        int newY = y + i;

                        if (newX < 0 || newX >= GridWidth || newY >= GridHeight || (newY >= 0 && field[newY, newX] != 0))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void PlaceTetromino() // Розміщення тетроміно на полі гри
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if ((tetrominoes[currentTetromino] & (1 << (i * 4 + j))) != 0)
                    {
                        field[currentY + i, currentX + j] = 1;
                    }
                }
            }
        }

        private void ClearLines() // Видалення заповнених ліній на полі гри
        {
            for (int y = GridHeight - 1; y >= 0; y--)
            {
                bool fullLine = true;
                for (int x = 0; x < GridWidth; x++)
                {
                    if (field[y, x] == 0)
                    {
                        fullLine = false;
                        break;
                    }
                }
                if (fullLine)
                {
                    for (int i = y; i > 0; i--)
                    {
                        for (int j = 0; j < GridWidth; j++)
                        {
                            field[i, j] = field[i - 1, j];
                        }
                    }
                    for (int j = 0; j < GridWidth; j++)
                    {
                        field[0, j] = 0;
                    }
                    score += 100;
                    y++;
                }
            }
        }
