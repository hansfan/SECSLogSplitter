using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SECSLogSplitter
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 1)
            {
                string path = args[0].Trim();

                Execute(path);
            }
            else
            {
                Console.WriteLine("Please keyin input full path");
            }
            Console.WriteLine("Please press any key to end.");
            Console.ReadKey();
        }

        private static void Execute(string path)
        {
            InputStream input = new InputStream(path);
            OutputStream output = new OutputStream(path);
            //output.Output(input.GetXMLMsgEnumbertor(), ".xml");
            output.Output(input.GetSECSMsgEnumerator(), ".sml");
        }
    }

    class OutputStream
    {
        private string _path = string.Empty;
        private StreamWriter writer;
        private string directoryName = string.Empty;
        private string fileName = string.Empty;

        public OutputStream(string path)
        {

            directoryName = Path.GetDirectoryName(path);
            fileName = Path.GetFileNameWithoutExtension(path);
            this._path = directoryName + fileName;

        }

        public void Output(List<string>.Enumerator enumerator, string ExtensionName)
        {
            using (writer = new StreamWriter(this._path + ExtensionName))
            {
                while (enumerator.MoveNext())
                {
                    writer.WriteLine(enumerator.Current);
                }
            }
        }

    }

    class InputStream
    {
        private string _path;
        private StreamReader reader;
        private Regex regexXMLStart;
        private Regex regexSECSStart;
        private Regex regexXMLEnd;
        private Regex regexSECSEnd;
        private string pattern4XMLStart = "<message>";
        private string pattern4XMLEnd = "</message>";
        private string pattern4SECSStart = @"^\d{4}-\d{2}-\d{2}-\d{2}:\d{2}:\d{2} (\w+?): 'S\d+F\d+'";
        //2020-05-07-14:33:53 CR_H: 'S1F13'
        private string pattern4SECSEnd = @"^\.$";
        private List<string> list4XML;
        private List<string> list4SECS;
        private const int DATESIZE = 20;

        public InputStream(string path)
        {
            this._path = path;
            regexXMLStart = new Regex(pattern4XMLStart);
            regexSECSStart = new Regex(pattern4SECSStart);
            regexXMLEnd = new Regex(pattern4XMLEnd);
            regexSECSEnd = new Regex(pattern4SECSEnd);
            list4XML = new List<string>();
            list4SECS = new List<string>();

            Split();
        }

        public List<string>.Enumerator GetSECSMsgEnumerator()
        {

            return list4SECS.GetEnumerator(); ;
        }

        public List<string>.Enumerator GetXMLMsgEnumbertor()
        {
            return list4XML.GetEnumerator();
        }

        private void Split()
        {
            string tmp = string.Empty;

            try
            {
                using (reader = new StreamReader(this._path))
                {
                    while (reader.Peek() != -1)
                    {
                        tmp = reader.ReadLine().Trim();

                        if (regexXMLStart.IsMatch(tmp))
                        {
                            //Header
                            list4XML.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                            do
                            {
                                list4XML.Add(tmp);
                                tmp = reader.ReadLine();

                            } while (!regexXMLEnd.IsMatch(tmp));

                            list4XML.Add(tmp);
                        }
                        else if (regexSECSStart.IsMatch(tmp))
                        {

                            //filter Date
                            //tmp = tmp.Substring(20).Trim();

                            //title add Date
                            string date = string.Empty;
                            int index = -1;

                            date = tmp.Substring(0, DATESIZE - 1).Replace(":", "_");
                            tmp = tmp.Substring(DATESIZE);
                            index = tmp.IndexOf(":");
                            tmp = date + tmp.Substring(index);


                            do
                            {
                                list4SECS.Add(tmp);
                                tmp = reader.ReadLine();

                            } while (!regexSECSEnd.IsMatch(tmp));

                            list4SECS.Add(tmp);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("IOException:" + ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }




    }


}
