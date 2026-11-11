using Godot;
using System;

namespace PursualRPG.Scripts.Core
{
    public partial class RandomizerManager : Node
    {
        [Export] public int DiceFaces { get; set; } = 6;

        public int PickRandomSelector(int faces, int amount = 1)
        {
            DiceFaces = faces;
            GD.Print($"--- Iniciando Sistema de Sorteio ({amount} Dado(s) de {DiceFaces} faces) ---");
            
            int totalResult = 0;
            for (int i = 0; i < amount; i++)
            {
                int chosenMethod = (int)(GD.Randi() % 3) + 1;
                GD.Print($"[Selector] O método sorteado para o rolamento foi o Método {chosenMethod}.");

                int result = 0;
                switch (chosenMethod)
                {
                    case 1: result = RollMethod1_GodotRandom(); break;
                    case 2: result = RollMethod2_DotNetRandom(); break;
                    case 3: result = RollMethod3_TimeBasedSeed(); break;
                }
                totalResult += result;
            }

            GD.Print($"[Resultado Final] O total parou no número: {totalResult}\n");
            return totalResult;
        }

        private int RollMethod1_GodotRandom()
        {
            GD.Print("[Método 1] Utilizando GD.Randi() nativo do Godot.");
            return (int)(GD.Randi() % DiceFaces) + 1;
        }

        private int RollMethod2_DotNetRandom()
        {
            GD.Print("[Método 2] Utilizando a biblioteca padrão do .NET (System.Random).");
            Random dotnetRandom = new Random();
            return dotnetRandom.Next(1, DiceFaces + 1);
        }

        private int RollMethod3_TimeBasedSeed()
        {
            GD.Print("[Método 3] Extraindo dados de Time.GetTimeStringFromSystem() para criar uma Seed matemática.");
            string timeString = Time.GetTimeStringFromSystem();
            string cleanTimeString = timeString.Replace(":", "");
            
            if (int.TryParse(cleanTimeString, out int timeSeed))
            {
                Random seededRandom = new Random(timeSeed);
                return seededRandom.Next(1, DiceFaces + 1);
            }
            return 1;
        }
    }
}