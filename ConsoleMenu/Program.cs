using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMenu
{
    class Program
    {
        public enum Smartphones
        {
            iPhone_5 = 1, 
            iPhone_4S = 2, 
            iPhone_4 = 3, 
            Samsung_Galaxy_S4 = 4, 
            Samsung_Galaxy_S3 = 5
        }

        static void Main(string[] args)
        {
            var menu = new ConsoleMenu<Smartphones>(Console.OpenStandardOutput(), Console.OpenStandardInput())
                           {
                               Header = "Please select a Smartphone:"
                           };

            menu.ShowMenu();
            
            Console.WriteLine();
            Console.WriteLine("Your selection: " + menu.SelectedMenuEntry.ToString().Replace("_", " "));
            Console.WriteLine();

            Console.ReadKey();
        }
    }
}
