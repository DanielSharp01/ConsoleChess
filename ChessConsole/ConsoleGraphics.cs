using System;

namespace ChessConsole
{
    /// <summary>
    /// Colored console character for the <see cref="ConsoleGraphics.backBuffer"/> and <see cref="ConsoleGraphics.frontBuffer"/>
    /// </summary>
    public struct CChar : IEquatable<CChar>
    {
        public ConsoleColor Foreground;
        public ConsoleColor Background;
        /// <summary>
        /// Actual character value
        /// </summary>
        public char C;

        public CChar(char c = ' ', ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            Foreground = foreground;
            Background = background;
            C = c;
        }

        public bool Equals(CChar other)
        {
            return other.Foreground == Foreground && other.Background == Background && other.C == C;
        }

        public static bool operator==(CChar lhs, CChar rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(CChar lhs, CChar rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Handles double buffered C# console
    /// </summary>
    public class ConsoleGraphics
    {
        /// <summary>
        /// First everything is drawn to the back buffer for double buffering purposes
        /// you can "swap" buffers with <see cref="SwapBuffers"/>
        /// </summary>
        private CChar[,] backBuffer;

        /// <summary>
        /// Current console's colored character buffer
        /// you can "swap" buffers with <see cref="SwapBuffers"/>
        /// </summary>
        private CChar[,] frontBuffer;

        public ConsoleGraphics()
        {
            backBuffer = new CChar[Console.BufferWidth, Console.BufferHeight];
            frontBuffer = new CChar[Console.BufferWidth, Console.BufferHeight];
        }



        #region DrawMethods

        /// <summary>
        /// Clears the back buffer, next SwapBuffer
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < backBuffer.GetLength(0); i++)
            {
                for (int j = 0; j < backBuffer.GetLength(1); j++)
                {
                    backBuffer[i, j] = new CChar();
                }
            }
        }

        /// <summary>
        /// Draws a colored character to the back buffer
        /// </summary>
        /// <param name="cchar">The colored character to draw</param>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void Draw(CChar cchar, int x, int y)
        {
            backBuffer[x, y] = cchar;
        }

        /// <summary>
        /// Draws a colored character to the back buffer, it doesn't change background color
        /// </summary>
        /// <param name="c">The character to draw</param>
        /// <param name="foreground">Color of the character</param>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void DrawTransparent(char c, ConsoleColor foreground, int x, int y)
        {
            backBuffer[x, y].C = c;
            backBuffer[x, y].Foreground = foreground;
        }

        /// <summary>
        /// Draws an area of colored characters to the back buffer.
        /// The arrays length is used as area width and height.
        /// </summary>
        /// <param name="cchars">The colored character area to draw</param>
        /// <param name="x">Starting X-coord in console buffer</param>
        /// <param name="y">Starting Y-coord in console buffer</param>
        public void DrawArea(CChar[,] cchars, int x, int y)
        {
            for (int i = 0; i < cchars.GetLength(0); i++)
            {
                for (int j = 0; j < cchars.GetLength(1); j++)
                {
                    backBuffer[x + i, y + j] = cchars[i, j];
                }
            }
        }

        /// <summary>
        /// Draws text to the screen. Multiline is not handled.
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="foreground">Foreground color of text</param>
        /// <param name="background">Background color of text</param>
        /// <param name="x">Starting X-coord in console buffer</param>
        /// <param name="y">Starting Y-coord in console buffer</param>
        public void DrawText(string text, ConsoleColor foreground, ConsoleColor background, int x, int y)
        {
            CChar[,] area = new CChar[text.Length, 1];
            for (int i = 0; i < text.Length; i++)
            {
                area[i, 0] = new CChar(text[i], foreground, background);
            }

            DrawArea(area, x, y);
        }

        /// <summary>
        /// Draws text to the screen with a transparent background. Multiline is not handled.
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="foreground">Foreground color of text</param>
        /// <param name="x">Starting X-coord in console buffer</param>
        /// <param name="y">Starting Y-coord in console buffer</param>
        public void DrawTextTrasparent(string text, ConsoleColor foreground, int x, int y)
        {
            CChar[,] area = new CChar[text.Length, 1];
            for (int i = 0; i < text.Length; i++)
            {
                area[i, 0] = new CChar(text[i], foreground, backBuffer[x + i, y].Background);
            }

            DrawArea(area, x, y);
        }

        /// <summary>
        /// Fills an area of the back buffer with one specic colored character
        /// The arrays length is used as area width and height.
        /// </summary>
        /// <param name="cchar">The colored character area to draw</param>
        /// <param name="x">Starting X-coord in console buffer</param>
        /// <param name="y">Starting Y-coord in console buffer</param>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        public void FillArea(CChar cchar, int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    backBuffer[x + i, y + j] = cchar;
                }
            }
        }

        /// <summary>
        /// Clears area on the screen
        /// </summary>
        /// <param name="x">Starting X-coord in console buffer</param>
        /// <param name="y">Starting Y-coord in console buffer</param>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        public void ClearArea(int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    backBuffer[x + i, y + j] = new CChar();
                }
            }
        }

        #endregion

        #region Darken_Lighten

        /// <summary>
        /// Darkens the background color of a colored character in the back buffer.
        /// If background color is already dark or no dark version exists it leaves it unchanged.
        /// </summary>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void DarkenBackground(int x, int y)
        {
            switch (backBuffer[x, y].Background)
            {
                case ConsoleColor.Blue:
                    backBuffer[x, y].Background = ConsoleColor.DarkBlue;
                    break;
                case ConsoleColor.Green:
                    backBuffer[x, y].Background = ConsoleColor.DarkGreen;
                    break;
                case ConsoleColor.Yellow:
                    backBuffer[x, y].Background = ConsoleColor.DarkYellow;
                    break;
                case ConsoleColor.Magenta:
                    backBuffer[x, y].Background = ConsoleColor.DarkMagenta;
                    break;
                case ConsoleColor.Gray:
                    backBuffer[x, y].Background = ConsoleColor.DarkGray;
                    break;
                case ConsoleColor.Cyan:
                    backBuffer[x, y].Background = ConsoleColor.DarkCyan;
                    break;
                case ConsoleColor.Red:
                    backBuffer[x, y].Background = ConsoleColor.DarkRed;
                    break;
            }
        }

        /// <summary>
        /// Darkens the foreground color of a colored character in the back buffer.
        /// If foreground color is already dark or no dark version exists it leaves it unchanged.
        /// </summary>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void DarkenForeground(int x, int y)
        {
            switch (backBuffer[x, y].Foreground)
            {
                case ConsoleColor.Blue:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkBlue;
                    break;
                case ConsoleColor.Green:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkGreen;
                    break;
                case ConsoleColor.Yellow:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkYellow;
                    break;
                case ConsoleColor.Magenta:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkMagenta;
                    break;
                case ConsoleColor.Gray:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkGray;
                    break;
                case ConsoleColor.Cyan:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkCyan;
                    break;
                case ConsoleColor.Red:
                    backBuffer[x, y].Foreground = ConsoleColor.DarkRed;
                    break;
            }
        }

        /// <summary>
        /// Lightens the background color of a colored character in the back buffer.
        /// If background color is already light or no light version exists it leaves it unchanged.
        /// </summary>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void LightenBackground(int x, int y)
        {
            switch (backBuffer[x, y].Background)
            {
                case ConsoleColor.DarkBlue:
                    backBuffer[x, y].Background = ConsoleColor.Blue;
                    break;
                case ConsoleColor.DarkGreen:
                    backBuffer[x, y].Background = ConsoleColor.Green;
                    break;
                case ConsoleColor.DarkYellow:
                    backBuffer[x, y].Background = ConsoleColor.Yellow;
                    break;
                case ConsoleColor.DarkMagenta:
                    backBuffer[x, y].Background = ConsoleColor.Magenta;
                    break;
                case ConsoleColor.DarkGray:
                    backBuffer[x, y].Background = ConsoleColor.Gray;
                    break;
                case ConsoleColor.DarkCyan:
                    backBuffer[x, y].Background = ConsoleColor.Cyan;
                    break;
                case ConsoleColor.DarkRed:
                    backBuffer[x, y].Background = ConsoleColor.Red;
                    break;
            }
        }

        /// <summary>
        /// Lightens the foreground color of a colored character in the back buffer.
        /// If foreground color is already light or no light version exists it leaves it unchanged.
        /// </summary>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void LightenForeground(int x, int y)
        {
            switch (backBuffer[x, y].Foreground)
            {
                case ConsoleColor.DarkBlue:
                    backBuffer[x, y].Foreground = ConsoleColor.Blue;
                    break;
                case ConsoleColor.DarkGreen:
                    backBuffer[x, y].Foreground = ConsoleColor.Green;
                    break;
                case ConsoleColor.DarkYellow:
                    backBuffer[x, y].Foreground = ConsoleColor.Yellow;
                    break;
                case ConsoleColor.DarkMagenta:
                    backBuffer[x, y].Foreground = ConsoleColor.Magenta;
                    break;
                case ConsoleColor.DarkGray:
                    backBuffer[x, y].Foreground = ConsoleColor.Gray;
                    break;
                case ConsoleColor.DarkCyan:
                    backBuffer[x, y].Foreground = ConsoleColor.Cyan;
                    break;
                case ConsoleColor.DarkRed:
                    backBuffer[x, y].Foreground = ConsoleColor.Red;
                    break;
            }
        }

        #endregion

        #region Color Getters/Setters
        /// <summary>
        /// Sets the background color of the back buffer at (x, y)
        /// </summary>
        /// <param name="color">New background color</param>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void SetBackground(ConsoleColor color, int x, int y)
        {
            backBuffer[x, y].Background = color;
        }

        /// <summary>
        /// Gets the background color of the back buffer at (x, y)
        /// </summary>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        /// <returns>Background color at (x, y)</returns>
        public ConsoleColor GetBackground(int x, int y)
        {
            return backBuffer[x, y].Background;
        }

        /// <summary>
        /// Sets the foreground color of the back buffer at (x, y)
        /// </summary>
        /// <param name="color">New foreground color</param>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        public void SetForeground(ConsoleColor color, int x, int y)
        {
            backBuffer[x, y].Foreground = color;
        }

        /// <summary>
        /// Gets the foreground color of the back buffer at (x, y)
        /// </summary>
        /// <param name="x">X-coord in console buffer</param>
        /// <param name="y">Y-coord in console buffer</param>
        /// <returns>Foreground color at (x, y)</returns>
        public ConsoleColor GetForeground(int x, int y)
        {
            return backBuffer[x, y].Foreground;
        }
        #endregion

        /// <summary>
        /// Overwrites the FrontBuffer and redraws the character if it's different from the BackBuffer
        /// </summary>
        public void SwapBuffers()
        {
            for (int i = 0; i < backBuffer.GetLength(0); i++)
            {
                for (int j = 0; j < backBuffer.GetLength(1); j++)
                {
                    if (frontBuffer[i, j] != backBuffer[i, j])
                    {
                        Console.SetCursorPosition(i, j);
                        Console.ForegroundColor = backBuffer[i, j].Foreground;
                        Console.BackgroundColor = backBuffer[i, j].Background;
                        Console.Write(backBuffer[i, j].C);
                        frontBuffer[i, j] = backBuffer[i, j];
                    }
                }
            }
        }
    }
}
