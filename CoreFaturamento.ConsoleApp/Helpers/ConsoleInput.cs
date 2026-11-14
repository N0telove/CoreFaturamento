using System;
using System.Collections.Generic;
using System.Text;

namespace CoreFaturamento.ConsoleApp.Helpers;

public static class ConsoleInput
{

    public static int LerInteiro(string mensagem)
    {
        while (true)
        {
            Console.WriteLine(mensagem);
            if (!int.TryParse(Console.ReadLine(), out var produtoId) || produtoId <= 0)
            {
                Console.WriteLine("Informação inválida.");
                continue;
            }
            return produtoId;
        }
    }
}
