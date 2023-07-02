using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Translater
{
    internal class Program
    {
        #region Поля
        private static string motherLanguage = "ru";
        #endregion
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                string firstArg = args[0];
                string secondArg = args[1];

                if (firstArg.StartsWith("-") && firstArg.Length == 3 && !secondArg.StartsWith("-"))
                {
                    Translater.Translate(secondArg, firstArg.Replace("-", ""));
                }
                else
                {
                    Console.WriteLine("Условия не выполнены.");
                }
            }
            else if (args.Length == 3)
            {
                string firstArg = args[0];
                string secondArg = args[1];
                string theerdArg = args[2];

                if (firstArg.Contains("-") && firstArg.Length == 3 
                    && secondArg.Contains("-") && secondArg.Length == 3
                    && !theerdArg.StartsWith("-"))
                {                    
                    Translater.Translate(secondArg, firstArg.Replace("-", ""), secondArg.Replace("-", ""));                   
                }
                else
                {
                    Console.WriteLine("Условия не выполнены.");
                }
            }
            else
            {
                Console.WriteLine(
                   $"{Assembly.GetEntryAssembly().GetName().Name} v.{Assembly.GetEntryAssembly().GetName().Version}\n" +
                   "Usage:\n" +
                   "Translater -<From Leng> -<To Leng> \"Text you want to translate\""
                   );
            }
        }
    }
}
