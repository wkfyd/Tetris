using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    public partial class Form1 : Form
    {
        Game myGame;

        private int edge_Size_X = 2;
        private int edge_Size_Y = 2;
        private int lastScore = 0;
        private int downTickInterval = 600;
        private int playTime = 0;
        private bool isLose = false;
        private bool putUp = false;
        private bool isPlay;

        Point boardStartPoint;
        Point boardEndPoint;
        Rectangle startButtonRect;
        Rectangle quitButtonRect;

        SolidBrush[] blockBrushes = new SolidBrush[8];
        SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(255, 100, 100, 100));

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(255, 0, 0, 0);
            SetBlockBrushes();
            SetSize();
            Reset();
        }

        private void Reset()
        {
            myGame = new Game();
            downTickInterval = 600;
            isPlay = false;
        }

        private void SetBlockBrushes()
        {
            blockBrushes[0] = new SolidBrush(Color.FromArgb(000, 255, 255));
            blockBrushes[1] = new SolidBrush(Color.FromArgb(255, 255, 000));
            blockBrushes[2] = new SolidBrush(Color.FromArgb(050, 000, 255));
            blockBrushes[3] = new SolidBrush(Color.FromArgb(255, 100, 000));
            blockBrushes[4] = new SolidBrush(Color.FromArgb(255, 000, 000));
            blockBrushes[5] = new SolidBrush(Color.FromArgb(000, 200, 000));
            blockBrushes[6] = new SolidBrush(Color.FromArgb(255, 000, 255));
            blockBrushes[7] = new SolidBrush(Color.FromArgb(200, 200, 200));
        }

        private void SetSize()
        {
            //클라이언트 사이즈
            int formSizeX = edge_Size_X * 5 + Game.BoardX * 2;
            int formSizeY = edge_Size_Y * 2 + Game.BoardY;
            SetClientSizeCore(formSizeX * Game.Cell_Size, formSizeY * Game.Cell_Size);

            //보드판 사이즈
            boardStartPoint = new Point(edge_Size_X * 2 + Game.option_Size_X, edge_Size_Y);
            boardEndPoint = new Point(boardStartPoint.X + Game.BoardX, edge_Size_Y + Game.BoardY);
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawGame(e.Graphics);
            DrawOption(e.Graphics);
            if (!isPlay) DrawStartScene(e.Graphics);
        }


        private void DrawStartScene(Graphics g)
        {
            Font font = new Font("Gill Sans MT", 23, FontStyle.Bold);
            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255));

            //씬배경
            Rectangle rect = new Rectangle(boardStartPoint.X * Game.Cell_Size, boardStartPoint.Y * Game.Cell_Size
                , (boardEndPoint.X - boardStartPoint.X) * Game.Cell_Size, (boardEndPoint.Y - boardStartPoint.Y) * Game.Cell_Size);
            g.FillRectangle(brush, rect);

            //START배경
            rect = new Rectangle(rect.X + rect.Width / 4, rect.Y + rect.Height / 3, rect.Width / 2, rect.Height / 10);
            brush = new SolidBrush(Color.FromArgb(20, 20, 20));
            g.FillRectangle(brush, rect);
            startButtonRect = rect;

            //START 글자
            brush = new SolidBrush(Color.FromArgb(250, 250, 250));
            g.DrawString("START", font, brush, rect.X, rect.Y + 5);

            //QUIT배경
            rect.Y += edge_Size_Y * 3 * Game.Cell_Size;
            brush = new SolidBrush(Color.FromArgb(20, 20, 20));
            g.FillRectangle(brush, rect);
            quitButtonRect = rect;

            //QUIT 글자
            brush = new SolidBrush(Color.FromArgb(250, 250, 250));
            g.DrawString("  QUIT", font, brush, rect.X, rect.Y + 5);

            if (lastScore != 0)
            {
                rect.Y -= edge_Size_Y * 5 * Game.Cell_Size;
                brush = new SolidBrush(Color.FromArgb(20, 20, 20));
                g.DrawString(Convert.ToString(lastScore) + "점", font, brush, rect.X, rect.Y);
                if (isLose) g.DrawString("W I N", font, brush, rect.X, rect.Y - 50);
                else g.DrawString("L O S E", font, brush, rect.X, rect.Y - 50);
            }
        }

        #region DrawGame
        private void DrawGame(Graphics g)
        {
            DrawBackground(g, boardStartPoint, boardEndPoint);
            DrawBlock(g, boardStartPoint);
            DrawLine(g, boardStartPoint, boardEndPoint);
        }

        private void DrawBackground(Graphics g, Point startPoint, Point endPoint)
        {
            startPoint.X *= Game.Cell_Size;
            startPoint.Y *= Game.Cell_Size;
            endPoint.X = endPoint.X * Game.Cell_Size - startPoint.X;
            endPoint.Y = endPoint.Y * Game.Cell_Size - startPoint.Y;
            g.FillRectangle(backgroundBrush, new Rectangle(startPoint.X, startPoint.Y
                , endPoint.X, endPoint.Y));
        }

        private void DrawBlock(Graphics g, Point startPoint)
        {
            DrawStackedBlock(g, startPoint);
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    DrawNowBlock(g, startPoint, y, x);
                    DrawDropPreView(g, startPoint, y, x);
                }
            }
        }

        private void DrawNowBlock(Graphics g, Point startPoint, int y, int x)
        {
            Rectangle rect;
            if (myGame.BlockCheck(y, x, -1))
            {
                rect = myGame.GetRect(y + startPoint.Y, x + startPoint.X);
                g.FillRectangle(blockBrushes[myGame.now.shape], rect);
            }
        }

        private void DrawStackedBlock(Graphics g, Point startPoint)  // 바닥에 쌓여있는 블록
        {
            Rectangle rect;
            for (int y = 0; y < Game.BoardY; y++)
            {
                for (int x = 0; x < Game.BoardX; x++)
                {
                    if (myGame.gameBoard[y, x])
                    {
                        rect = new Rectangle((x + startPoint.X) * Game.Cell_Size, (y + startPoint.Y) * Game.Cell_Size, Game.Cell_Size, Game.Cell_Size);
                        g.FillRectangle(blockBrushes[myGame.gameColorBoard[y, x]], rect);
                    }
                }
            }
        }


        private void DrawLine(Graphics g, Point startPoint, Point endPoint)
        {
            Point drawStartPoint = new Point();
            Point drawEndPoint = new Point();
            Pen linePen = new Pen(Color.FromArgb(255, 50, 50, 50), 1);
            for (int x = 0; x <= endPoint.X - startPoint.X; x++) //세로줄
            {
                drawStartPoint.X = (startPoint.X + x) * Game.Cell_Size;
                drawStartPoint.Y = startPoint.Y * Game.Cell_Size;
                drawEndPoint.X = drawStartPoint.X;
                drawEndPoint.Y = endPoint.Y * Game.Cell_Size;
                g.DrawLine(linePen, drawStartPoint, drawEndPoint);
            }

            for (int y = 0; y <= endPoint.Y - startPoint.Y; y++) //가로줄
            {
                drawStartPoint.X = startPoint.X * Game.Cell_Size;
                drawStartPoint.Y = (startPoint.Y + y) * Game.Cell_Size;
                drawEndPoint.X = endPoint.X * Game.Cell_Size;
                drawEndPoint.Y = drawStartPoint.Y;
                g.DrawLine(linePen, drawStartPoint, drawEndPoint);
            }
        }


        private void DrawDropPreView(Graphics g, Point startPoint, int y, int x) //Drop 미리보기
        {
            Rectangle rect;
            if (myGame.BlockCheck(y, x, -1))
            {
                int dis = myGame.GetFloorDistance();
                if (dis != 0)
                {
                    Color color = blockBrushes[myGame.now.shape].Color;
                    Pen gPen = new Pen(color, 2);
                    rect = myGame.GetRect(y + dis + startPoint.Y, x + startPoint.X);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(100, color.R, color.G, color.B)), rect);
                    g.DrawRectangle(gPen, rect);
                }
            }
        }
        #endregion

        #region DrawOption
        private void DrawOption(Graphics g)
        {
            DrawNextArea(g);
            DrawString(g);
        }

        private void DrawNextArea(Graphics g)
        {
            Point startPoint = new Point(edge_Size_X * 3 + Game.BoardX + Game.option_Size_X, edge_Size_Y);
            Point endPoint = new Point(startPoint.X + Game.option_Size_X, 18);
            DrawBackground(g, startPoint, endPoint);
            if (isPlay)
            {
                Rectangle rect;
                for (int nxt = 0; nxt < myGame.nextBlocks.Length; nxt++)
                {
                    for (int yy = 0; yy < 4; yy++)
                    {
                        for (int xx = 0; xx < 4; xx++)
                        {
                            if (myGame.BlockCheck(yy, xx, nxt))
                            {
                                rect = new Rectangle((xx + startPoint.X) * Game.Cell_Size
                                    , (startPoint.Y + yy + 1 + nxt * 5) * Game.Cell_Size
                                    , Game.Cell_Size, Game.Cell_Size);
                                g.FillRectangle(blockBrushes[myGame.nextBlocks[nxt].shape], rect);
                            }
                        }
                    }
                }
            }
            DrawLine(g, startPoint, endPoint);
        }

        private void DrawString(Graphics g)
        {
            int nextX = (edge_Size_X * 3 + Game.BoardX + Game.option_Size_X) * Game.Cell_Size;
            int nextY = edge_Size_Y * Game.Cell_Size;
            Font font = new Font("Gill Sans MT", 25, FontStyle.Bold);
            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 254, 255, 255));
            g.DrawString("NEXT", font, brush, nextX, nextY - Game.Cell_Size);
            g.DrawString("SCORE", font, brush, edge_Size_X * Game.Cell_Size - 10, (edge_Size_Y * 4) * Game.Cell_Size);

            //플레이시간
            g.DrawString("TIME", font, brush, edge_Size_X * Game.Cell_Size, (Game.BoardY - 2) * Game.Cell_Size);
            string secondStr = playTime % 60 < 10 ? "0" + playTime % 60 : "" + playTime % 60;
            string minuteStr = playTime / 60 < 10 ? "0" + playTime / 60 : "" + playTime / 60;
            g.DrawString(minuteStr + ":" + secondStr,
                font, brush, edge_Size_X * Game.Cell_Size, (Game.BoardY) * Game.Cell_Size);

            scoreLabel.Location = new Point(edge_Size_X * Game.Cell_Size - 10, (edge_Size_Y * 5) * Game.Cell_Size);
            scoreLabel.Text = Convert.ToString(myGame.gameScore);
        }
        #endregion

        #region Event
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isPlay)
            {
                switch (e.KeyCode)
                {
                    case Keys.Down: myGame.MoveDown(); break;
                    case Keys.Right: myGame.MoveRight(); break;
                    case Keys.Left: myGame.MoveLeft(); break;
                    case Keys.Space: myGame.MoveDrop(); break;
                    case Keys.Up:
                        if (!putUp) myGame.MoveTurn();
                        putUp = true;
                        break;
                }
                CheckGameOver();
                Invalidate();
            }
        }


        private void downTimer_Tick(object sender, EventArgs e)
        {
            if (isPlay)
            {
                myGame.gameScore++;
                if (!myGame.MoveDown()) CheckGameOver();
                if (downTickInterval > 200) downTimer.Interval = downTickInterval--;

                Invalidate();
            }
        }

        private void oclock_Tick(object sender, EventArgs e)
        {
            if (isPlay) playTime++;
            else playTime = 0;
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if ((startButtonRect.X < e.Location.X &&
                e.Location.X < startButtonRect.X + startButtonRect.Width) &&
                (startButtonRect.Y <= e.Location.Y &&
                e.Location.Y <= startButtonRect.Y + startButtonRect.Height))
            {
                isPlay = true;
            }

            else if ((quitButtonRect.X <= e.Location.X &&
                e.Location.X <= quitButtonRect.X + quitButtonRect.Width) &&
                (quitButtonRect.Y <= e.Location.Y &&
                e.Location.Y <= quitButtonRect.Y + quitButtonRect.Height))
            {
                Close();
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (isPlay)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        putUp = false;
                        break;
                }
            }
        }

        #endregion
        public void CheckGameOver()
        {
            if (myGame.CheckGameOver())
            {
                isLose = false;
                lastScore = myGame.gameScore;
                Reset();
            }

        }

    }
}
