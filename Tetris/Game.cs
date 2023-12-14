using System;
using System.Drawing;

namespace Tetris
{
    class Game
    {
        public const int BoardX = 12;  //board x 게임 가로 넓이
        public const int BoardY = 25;
        public const int Cell_Size = 20; // 네모칸 크기
        public const int option_Size_X = 5;

        public int gameScore;
        public int holdBlockNum;

        public Block now = new Block();
        public Block[] nextBlocks = new Block[3];

        public bool[,] gameBoard = new bool[BoardY, BoardX];
        public int[,] gameColorBoard = new int[BoardY, BoardX];

        static private Random rand = new Random();

        public Game()
        {
            Reset();
        }

        public void Reset()
        {
            now.ResetBlock();
            for (int i = 0; i < nextBlocks.Length; i++) nextBlocks[i] = new Block();
            for (int i = 0; i < nextBlocks.Length; i++) SetNewBlock();
            for (int i = 0; i < BoardY; i++) for (int j = 0; j < BoardX; j++) gameBoard[i, j] = false;
            gameScore = 0;
            holdBlockNum = -1;
        }

        public bool CheckGameOver()
        {
            return !CanHere();
        }


        #region Move()
        private bool CanHere()
        {
            int blockX;
            int blockY;
            for (int yy = 0; yy < 4; yy++)
            {
                for (int xx = 0; xx < 4; xx++)
                {
                    if (Block.BLOCK_SHAPE[now.shape, now.turn, yy, xx])
                    {
                        blockX = now.x + xx;
                        blockY = now.y + yy;
                        if (blockX < 0 || BoardX <= blockX || blockY < 0 || BoardY <= blockY || gameBoard[blockY, blockX]) return false;
                    }
                }
            }
            return true;
        }

        public bool MoveLeft()
        {
            now.MoveLeft();
            if (CanHere()) return true;

            now.MoveRight();
            return false;
        }

        public bool MoveRight()
        {
            now.MoveRight();
            if (CanHere()) return true;

            now.MoveLeft();
            return false;
        }

        public bool MoveUp()
        {
            now.MoveUp();
            if (CanHere()) return true;

            now.MoveDown();
            return false;
        }
        public void MoveTurn()
        {
            now.MoveTurn();
            if (CanHere()) return;
            if (MoveLeft()) return;
            if (MoveRight()) return;
            if (MoveUp()) return;
            now.MoveReturn();
        }

        public bool MoveDown()
        {
            now.MoveDown();
            if (CanHere()) return true;
            now.MoveUp();

            //땅에 닿으면
            FillLandBlock();
            gameScore += 4;
            CheckFullLine();
            SetNewBlock();
            return false;
        }

        public void MoveDrop()
        {
            while (MoveDown()) ;
        }

        #endregion

        private void FillLandBlock()
        {
            for (int yy = 0; yy < 4; yy++) //안내려가질때 현재 자리에 블럭 채우기
            {
                for (int xx = 0; xx < 4; xx++)
                {
                    if (Block.BLOCK_SHAPE[now.shape, now.turn, yy, xx])
                    {
                        gameBoard[now.y + yy, now.x + xx] = true;
                        gameColorBoard[now.y + yy, now.x + xx] = now.shape;
                    }
                }
            }
        }

        public void SetNewBlock()
        {
            now.ResetBlock();
            now.shape = nextBlocks[0].shape;
            nextBlocks[0].shape = nextBlocks[1].shape;
            nextBlocks[1].shape = nextBlocks[2].shape;
            nextBlocks[2].ResetBlock();
        }

        public int GetFloorDistance()
        {
            int dis = 0;
            while (++dis < BoardY)
            {
                now.y++;
                if (!CanHere())
                {
                    now.y -= dis;
                    return dis - 1;
                }
            }
            return dis - 1;
        }

        public bool CheckFullLine()
        {
            bool checker = false;
            int point = BoardX;
            for (int yy = 0; yy < BoardY; yy++)
            {
                for (int xx = 0; xx < BoardX; xx++)
                {
                    if (!gameBoard[yy, xx]) break;
                    if (xx == BoardX - 1)
                    {
                        EraseBlock(yy);
                        gameScore += point *= 2;

                        checker = true;
                    }
                }
            }
            return checker;
        }

        public void EraseBlock(int y)
        {
            for (; y >= 0; y--)
            {
                for (int x = 0; x < BoardX; x++)
                {
                    if (y == 0)
                    {
                        gameBoard[y, x] = false;
                    }
                    else
                    {
                        gameBoard[y, x] = gameBoard[y - 1, x];
                        gameColorBoard[y, x] = gameColorBoard[y - 1, x];
                    }
                }
            }
        }

        public bool BlockCheck(int y, int x, int block)
        {
            switch (block)
            {
                case -1: return Block.BLOCK_SHAPE[now.shape, now.turn, y, x];
                case 0: return Block.BLOCK_SHAPE[nextBlocks[0].shape, 0, y, x];
                case 1: return Block.BLOCK_SHAPE[nextBlocks[1].shape, 0, y, x];
                case 2: return Block.BLOCK_SHAPE[nextBlocks[2].shape, 0, y, x];
                case 3: return Block.BLOCK_SHAPE[holdBlockNum, 0, y, x];
                default: return false;
            }
        }

        public Rectangle GetRect(int y, int x)
        {
            return new Rectangle((now.x + x) * Cell_Size, (now.y + y) * Cell_Size, Cell_Size, Cell_Size);
        }
    }
}
