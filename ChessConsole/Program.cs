using System;

namespace ChessConsole
{
    public class Program
    {
        static ChessGame game;
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            ConsoleGraphics graphics = new ConsoleGraphics();
            game = new ChessGame();

            do
            {
                game.Draw(graphics);
                graphics.SwapBuffers();
                game.Update();
            } while (game.Running);

            Console.Read();
        }
    }
}
