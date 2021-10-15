using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace msheadtool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.WriteLine("### msheadtool v" + Assembly.GetExecutingAssembly().GetName().Version + " by KabanFriends ###");

            Console.WriteLine("");
            Console.WriteLine("Checking if CodeUtilities Item API is available...");

            bool useItemAPI = false;

            int port = 31372;
            IPAddress host = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipe = new IPEndPoint(host, port);

            try
            {
                var client = new TcpClient();
                client.Connect(ipe);

                Console.WriteLine("CodeUtilities Item API is available. The tool will automatically send you the head item.");
                useItemAPI = true;

                client.Dispose();
            }catch (Exception e)
            {
                Console.WriteLine("CodeUtilities Item API is not available.");
            }

            while (true)
            {
                Console.WriteLine("");
                Console.Write("Enter MineSkin head ID or URL: ");

                var input = Console.ReadLine();
                var id = input;

                if (Regex.IsMatch(input, "https?:\\/\\/(mineskin.org|minesk.in)\\/.*"))
                {
                    id = Regex.Replace(input, "https?:\\/\\/(mineskin.org|minesk.in)\\/(.*)", "$2");
                }

                var url = "https://api.mineskin.org/get/uuid/" + id;

                if (Regex.IsMatch(id, "^\\d*$"))
                {
                    url = "https://api.mineskin.org/get/id/" + id;
                }

                JsonElement jsonElement = new JsonElement();
                bool exception = false;
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        var jsonObject = JsonDocument.Parse(wc.DownloadString(url));
                        jsonElement = jsonObject.RootElement;

                        var hasError = jsonElement.TryGetProperty("error", out var value);

                        if (hasError == true)
                        {
                            throw new IOException("Received invalid JSON!");
                        }
                    }
                }catch (WebException e)
                {
                    Console.WriteLine("The head with that ID was not found!");
                    exception = true;
                }catch (IOException e)
                {
                    Console.WriteLine("Error while trying to fetch the head data!");
                    Console.WriteLine(e.StackTrace);
                    exception = true;
                }

                if (!exception)
                {
                    var numID = jsonElement.GetProperty("id");

                    Console.WriteLine("Numerical ID: " + numID);
                    Console.WriteLine("In-game Command: /i mshead " + numID);
                    Console.WriteLine("The command has been copied to your clipboard!");

                    ClipboardUtil.SetText("/i mshead " + numID);

                    if (useItemAPI)
                    {
                        var data = jsonElement.GetProperty("data");
                        var texture = data.GetProperty("texture");
                        var value = texture.GetProperty("value");
                        var signature = texture.GetProperty("signature");

                        var nbt = "{id:\\\"minecraft:player_head\\\",tag:{display:{Name:'{\\\"color\\\":\\\"#808080\\\",\\\"text\\\":\\\"Imported Skin\\\"}'},SkullOwner:{Name:\\\"DF-Import-MS\\\",Properties:{textures:[{Value:\\\"" + value + "\\\",Signature:\\\"" + signature + "\\\"}]}}},Count:1b}";

                        var client = new TcpClient();
                        client.Connect(ipe);

                        using (var stream = client.GetStream())
                        {
                            string json = "{\"type\":\"nbt\",\"source\":\"msheadtool\",\"data\":\"" + nbt + "\"}";
                            var buffer = Encoding.UTF8.GetBytes(json);
                            stream.Write(buffer, 0, buffer.Length);
                        }

                        client.Dispose();
                    }
                }
            }
        }
    }
}
