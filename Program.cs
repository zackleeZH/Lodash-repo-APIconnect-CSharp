using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        // replace with personal access token
        string token = "";

        // create httpclient
        using (var client = new HttpClient())
        {
            // URL for GitHub API repository contents
            string Url = $"https://api.github.com/repos/lodash/lodash/contents";

            // By default, it needs to add User-Agent header to avoid error
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

            // If using personal access token
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            try
            {
                // To store letter frequencies
                Dictionary<char, int> letterCount = new Dictionary<char, int>();

                // Fetch files from the repository
                // await is used to make sure the code is paused until the task is finished before moving on the next task.
                await AccessDirectory(Url, client, letterCount);

                // Sort the letter count in descending order
                var sortedDict = letterCount.OrderByDescending(letter => letter.Value);

                // Print the letter count
                Console.WriteLine("Letter present in JS/TS files in descending order:");
                foreach (var letter in sortedDict)
                {
                    Console.WriteLine($"{letter.Key}: {letter.Value}");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"General error: {e.Message}");
            }
        }
    }

    private static async Task AccessDirectory(string url, HttpClient client, Dictionary<char, int> letterCount)
    {
        // Fetch the directory content
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();
        JArray returnResponse = JArray.Parse(jsonResponse);

        foreach (var file in returnResponse)
        {
            string fileName = file["name"].ToString();
            string fileType = file["type"].ToString();

            // If the file is a directory, recurse into it
            if (fileType == "dir")
            {
                string subdirUrl = file["url"].ToString();
                await AccessDirectory(subdirUrl, client, letterCount);
            }
            else
            {
                // If it's a JavaScript or TypeScript file, fetch its content
                if (fileName.EndsWith(".js") || fileName.EndsWith(".ts"))
                {
                    string downloadUrl = file["download_url"].ToString();
                    await AccessFile(downloadUrl, client, letterCount);
                }
            }
        }
    }

    private static async Task AccessFile(string url, HttpClient client, Dictionary<char, int> letterCount)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string fileContent = await response.Content.ReadAsStringAsync();

        // Count the frequency of letters (case insensitive)
        foreach (char c in fileContent)
        {
            if (Char.IsLetter(c)) // Only count letters
            {
                char lowerChar = Char.ToLower(c);
                // if letter exist in dictionary, add the count
                if (letterCount.ContainsKey(lowerChar))
                {
                    letterCount[lowerChar]++;
                }
                else
                {
                    // new letter added into the dictionary with count = 1
                    letterCount[lowerChar] = 1;
                }
            }
        }
    }
}
