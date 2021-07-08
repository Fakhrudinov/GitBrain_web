using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiHttpGets2File
{
    public class ResponceObject
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string  body { get; set; }
    }

    class Program
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly HttpClient client = new HttpClient();

        static string filePath = "result.txt";

        static async  Task Main(string[] args)
        {
            //Fakhrudinov Alexander / Фахрудинов Александр
            //asbuka@gmail.com
            //
            //Необходимо написать консольное приложение, которое выполнит асинхронно 10 HTTP-запросов на получение постов из блога,
            //по целочисленному идентификатору, начиная с id=4 по id=13.
            //Для выполнения запросов необходимо использовать HttpClient
            //(см. https://docs.microsoft.com/ru-ru/dotnet/api/system.net.http.httpclient?view=net-5.0).
            //В результате работы программы должен получиться текстовый файл под названием result.txt,
            //в котором сохранена полученная информация о постах в следующем формате:
            //  userId
            //  id
            //  title
            //  body
            //Между постами присутствует пробел
            //Всего в файле должно оказаться 10 постов.
            //Документация к api: https://jsonplaceholder.typicode.com/ 
            //Метод получения поста по id: https://jsonplaceholder.typicode.com/posts/1 


            // Здесь создаем набор задач
            var tasks = new List<Task<ResponceObject>>();

            Console.WriteLine("Creating task pull");
            for (int i = 4; i < 14; i++)
            {
                var task = GetRequestWithId(i);
                tasks.Add(task);
            }

            //отправляем задачи на выполнение
            Console.WriteLine("Send task pull to execute");
            try
            {
                //ждать выполнения не более 5сек
                cts.CancelAfter(5000);
                // Ждем, пока все задачи будут готовы
                await Task.WhenAll(tasks);

                Console.WriteLine("All Task complete!");           
            }
            catch (Exception oce)
            {
                Console.WriteLine("Exception thrown when task executing - " + oce.Message);
            }
            finally
            {
                cts.Dispose();
            }

            //успешные таски запишем в файл
            Console.WriteLine("Write success posts to file " + filePath);
            foreach (var task in tasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    PrepareDataAndWriteToFile(task);
                }
            }

            Console.WriteLine("Jobs done.");
            //Console.ReadLine();
        }

        private static void PrepareDataAndWriteToFile(Task<ResponceObject> respObj)
        {
            //готовим строку для записи в файл
            string dataToFile =
                $"{respObj.Result.userId}\r\n" +
                $"{respObj.Result.id}\r\n" +
                $"{respObj.Result.title}\r\n" +
                $"{respObj.Result.body}\r\n";

            WriteToFile(dataToFile);
        }

        private static void WriteToFile(string dataToFile)
        {
            try
            {
                File.AppendAllText(filePath, dataToFile + Environment.NewLine);
            }
            catch (Exception f)
            {
                Console.WriteLine("Write to file exception - " + f.Message);
            }            
        }

        static async Task<ResponceObject> GetRequestWithId(int id)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/" + id, cts.Token);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                ResponceObject result = JsonSerializer.Deserialize<ResponceObject>(responseBody);

                Console.WriteLine($"Task with id {id} completed.");
                return result;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return null;
        }
    }
}
