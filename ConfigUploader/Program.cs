using System;
using System.Diagnostics;
using System.IO;

namespace ConfigUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ler variáveis de ambiente
            string accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT");
            string accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_KEY");
            string containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER");

            // Caminho do arquivo a ser carregado
            string filePath = @"..\RabbitMq.Nuget\bin\Release\RabbitMq.Nuget.1.0.0.nupkg";
            string fileName = "RabbitMq.Nuget.1.0.0.nupkg";

            // Montar o comando para o Azure CLI
            string command = $"az storage blob upload --account-name {accountName} --account-key {accountKey} --container-name {containerName} --file \"{filePath}\" --name \"{fileName}\" --overwrite";

            // Executar o comando no sistema
            ExecuteCommand(command);
        }

        // Método para executar o comando
        static void ExecuteCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    Console.WriteLine(output);
                }
            }
        }
    }
}
