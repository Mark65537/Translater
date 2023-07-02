using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Translater
{
    public class Translater
    {
        #region Глобальные переменные
        private static int page = 1;
        private static string frmLng = "ru";
        #endregion
        public static string Translate(string text, string fromLng, string toLng)
        {
            frmLng = fromLng;
            return Translate(text, toLng);
        }
        public static string Translate(string text, string leng)
        {
            if (frmLng.Equals(leng))
            {
                return text;
            }

            string translation=FindStringInJsonFile(text, leng);
            if (translation.Equals(""))
            {
               return TranslateYandexAPI(text, leng);
            }
            else
            { 
               return translation;                
            }
        }

        //создать функцию которая читает json файли ищет строку которая поредается в аргументе функции
        private static string FindStringInJsonFile(string text, string leng, string filePath = "TransWords.json")
        {
            var json = File.ReadAllText(filePath);
            var jsonObj = JObject.Parse(json);

            if (jsonObj.TryGetValue(text, out var translations))
            {
                var translation = translations[leng]?.ToString();
                if (!string.IsNullOrEmpty(translation))
                {
                    return translation;
                }
            }

            return "";
        }

        public static string TranslateYandexAPI(string text, string leng)
        {
            List<string> keys = GetYandexAPIkeys();
            return TranslateYandexAPI(keys, text, leng);            
        }

        public static string TranslateYandexAPI(List<string> keys, string text, string toLng)
        {
            foreach (string key in keys)
            {
                try
                {
                    string url = $"https://translate.yandex.net/api/v1.5/tr.json/translate?key={key}&lang={frmLng}-{toLng}&text={text}";
                    string json = new WebClient().DownloadString(url);
                    JObject obj = JObject.Parse(json);
                    string result = obj["text"][0].ToString();
                    WriteTransWordToJSON(text, toLng, result);
                    return result;
                }
                catch (Exception)
                {

                }
            }
            //TranslateYandexAPI(AddNewAPIkeys(), text, leng);

            throw new Exception($"невозможно перевести {text} c {frmLng} на {toLng}");
        }
        //создать функцию AddNewAPIkeys() которая ищет стороку начинающуюся на trnsl.1.1.20 и заканчивающуюся на знак ' или " на сайте https://github.com/search?p=3&q=https%3A%2F%2Ftranslate.yandex.net%2Fapi%2Fv1.5%2Ftr.json%2Ftranslate%3Fkey%3D&type=Code
        private static List<string> AddNewAPIkeys()
        {
            List<string> keys = GetYandexAPIkeys();
            List<string> newKeys = new List<string>();
            var web = new HtmlWeb();
            string url = $"https://github.com/search?p={page}&q=https%3A%2F%2Ftranslate.yandex.net%2Fapi%2Fv1.5%2Ftr.json%2Ftranslate%3Fkey%3D&type=Code";
            var doc = web.Load(url);
            //найти стороку начинающуюся на trnsl.1.1.20 заканчивающуюся на знак ' или " и длина строки ровна 85
            // Ищем все строки, начинающиеся с "trnsl.1.1.20" и заканчивающиеся на ' или "
            var regex = new Regex(@"trnsl\.1\.1\.20.*?['""]");
            var matches = regex.Matches(doc.Text);

            // Ищем строку длиной 85 символов
            foreach (Match match in matches)
            {
                if (match.Value.Length == 85)
                {
                    if (!keys.Contains(match.Value) && !newKeys.Contains(match.Value))
                    {
                        newKeys.Add(match.Value);
                    }                    
                }
            }

            if (newKeys.Count==0)
            {
                page++;
                AddNewAPIkeys();
            }
            WriteYandexAPIkeys(newKeys);
            return newKeys;
        }

        private static void WriteYandexAPIkeys(List<string> newKeys, string filePath = "YandexAPIkeys.txt")
        {
            //записать в начало файла YandexAPIkeys.txt с сохранением содержимого файла            
            string fileContent = File.ReadAllText(filePath);
            string newContent = string.Join(Environment.NewLine, newKeys) + Environment.NewLine + fileContent;
            File.WriteAllText(filePath, newContent);
        }

        private static void WriteTransWordToJSON(string text, string leng, string result)
        {
            // Load the JSON file
            string jsonFilePath = "TransWords.json";
            string jsonContent = File.ReadAllText(jsonFilePath);
            JObject translations = JObject.Parse(jsonContent);
            // Update the translation

            if (translations.ContainsKey(text))
            {
                JObject translationsForKey = (JObject)translations[text];
                translationsForKey[leng] = result;
            }
            else
            {
                JObject newTranslations = new JObject();
                newTranslations[leng] = result;
                translations[text] = newTranslations;
            }

            // Save the updated JSON file
            string updatedJsonContent = translations.ToString();
            File.WriteAllText(jsonFilePath, updatedJsonContent);           
        }

        private static List<string> GetYandexAPIkeys()
        {
            //построчное чтение файла YandexAPIkeys.txt и запись каждой строки в список keys 
            List<string> keys = new List<string>();

            using (StreamReader sr = new StreamReader("YandexAPIkeys.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    keys.Add(line);
                }
            }
            
            return keys;
        }
       
        public static string TranslateGoogleGET(string text, string from, string to)
        {
            string url = string.Format("https://translate.google.com/?sl={0}&tl={1}&text={2}&op=translate", from, to, HttpUtility.UrlEncode(text));
            StringBuilder result = new StringBuilder();
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                result.Append(wc.DownloadString(url));

                //<span class="ryNqvb" jsaction="click:E6Tfl,GFf3ac,tMZCfe; contextmenu:Nqw7Te,QP7LD; mouseout:Nqw7Te; mouseover:E6Tfl,c2aHje" jsname="W297wb">choose language
            }

            //взять из строки слово между (<span class="ryNqvb" jsaction="click:E6Tfl,GFf3ac,tMZCfe; contextmenu:Nqw7Te,QP7LD; mouseout:Nqw7Te; mouseover:E6Tfl,c2aHje" jsname="W297wb">) и (</span>)
            File.WriteAllText("translate.html", result.ToString());
            string pattern = "<span class=\"ryNqvb\"(.*?)</span>";
            var result2 = Regex.Match(result.ToString(), pattern).Groups[1].Value;

            return result2;
        }
        public static string TranslateGoogle(string text, string from, string to)
        {
            string url = string.Format("http://translate.google.com/translate_a/t?client=t&text={0}&hl=en&sl={1}&tl={2}&ie=UTF-8&oe=UTF-8&multires=1&otf=2&ssel=0&tsel=0&sc=1", HttpUtility.UrlEncode(text), from, to);
            string result = string.Empty;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                result = wc.DownloadString(url);
            }

            return result;
        }
        private static string GetCulture(string code = "")
        {
            if (!string.IsNullOrEmpty(code))
            {
                CultureInfo.CurrentCulture = new CultureInfo(code);
                CultureInfo.CurrentUICulture = new CultureInfo(code);
            }
            return $"CurrentCulture:{CultureInfo.CurrentCulture.Name}, CurrentUICulture:{CultureInfo.CurrentUICulture.Name}";
        }
    }
}
