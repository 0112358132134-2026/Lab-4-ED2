using System;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] hola = new string[3];

            if (hola[4] == null)
            {
                Console.WriteLine("No existe!");
            }
            Console.ReadLine();

            string prueba = "\n\ttomate";
            string prueba2 = "\u0093Pablo";
            string sub = prueba2.Substring(0, 2);
            Console.WriteLine(sub);
        }
    }
}
