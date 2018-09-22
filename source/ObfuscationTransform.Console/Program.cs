using ObfuscationTransform.Core;
using ObfuscationTransform.Core.Factory;
using ObfuscationTransform.Extensions;
using ObfuscationTransform.Parser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ObfuscationTransform.Container;
using ObfuscationTransform.Transformation;

namespace ObfuscationTrasnform.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //get bytes of code
            var file = ConfigurationManager.AppSettings["ProgramToObfuscate"];
            System.Console.WriteLine($"Obfuscating file name: {file}...");
            System.Console.WriteLine();

            //obfuscate assembly
            try
            {
                var programObfusator = Container.Resolve<IPeTransform>();
                var statistics = Container.Resolve<IStatistics>();
                var fileObfuscated = Path.GetFileNameWithoutExtension(file) + "Obf" + Path.GetExtension(file);

                statistics.NewStatistics += (o, e) => OnNewStatistics(o, e);

                //obfuscate file here!
                programObfusator.Transform(file);

                System.Console.WriteLine();
                System.Console.WriteLine();
                System.Console.WriteLine($"More stats:\nTotal number of assembly instructions on original program (including 'nops'): {statistics.NumberOfInstructions}");
                System.Console.WriteLine($"Instruction which are effective (excluding 'nop's) {statistics.EffectiveInstructions}");
                System.Console.WriteLine("");
                System.Console.WriteLine($"generated obfuscation file name: {fileObfuscated}");
                System.Console.WriteLine("Finished obfuscation.");
            }
            catch (ApplicationException ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            System.Console.ReadKey();
        }

        

        private static bool headerWritten = false;
        private static void OnNewStatistics(object sender, StatisiticsEventArgs e)
        {
            if (!headerWritten)
            {
                System.Console.WriteLine($"Confusion factor | Added Bytes | Misinterpreted instructions | Added Instructions | Added Junk Bytes");
                System.Console.WriteLine("_____________________________________________________________________________________________________");
                headerWritten = true;
            }

            float confusionFactor = e.MisinterpretedInstructions / (float)e.EffectiveInstructions;
            System.Console.Write($"\r{confusionFactor*100,10:#0.00}%{e.AddedBytes,15}" +
                 $"{e.MisinterpretedInstructions,20}{e.AddedInstructions,25}" +
                 $"{e.AddedJunkBytes,20}");

        }

   

    }
}
