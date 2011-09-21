/*
 * This file is part of the Bits on the Run .Net API Library.
 * 
 * The Bits on the Run .Net API Library is free software: you
 * can redistribute it and/or modify it under the terms of
 * the GNU Lesser General Public License as published by the
 * Free Software Foundation, either version 3 of the License,
 * or (at your option) any later version.
 * 
 * The Bits on the Run .Net API Library is distributed in the
 * hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser
 * General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General
 * Public License along with the Bits on the Run .Net API
 * Library.  If not, see <http://www.gnu.org/licenses/>.
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Context;
using Spring.Context.Support;
using BotR.API;
using System.Xml.Linq;
using System.IO;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace BotRAPIConsole {
    class Program {

        static void Main(string[] args) {
            try {

                IApplicationContext ctx = ContextRegistry.GetContext();
                BotRAPI api = ctx.GetObject("BotRAPI") as BotRAPI;

                //test video listing
                Console.WriteLine(api.Call("/videos/list"));

                //params to store with a new video
                NameValueCollection col = new NameValueCollection() {
                    
                {"title", "New test video"},
                    {"tags", "new, test, video, upload"},
                    {"description", "New video2"},
                    {"link", "http://www.bitsontherun.com"},
                    {"author", "Bits on the Run"}
                };

                //create the new vidoe
                string xml = api.Call("/videos/create", col);

                Console.WriteLine(xml);

                XDocument doc = XDocument.Parse(xml);
                var result = (from d in doc.Descendants("status")
                              select new {
                                  Status = d.Value
                              }).FirstOrDefault();

                //make sure the status was "ok" before trying to upload
                if (result.Status.Equals("ok", StringComparison.CurrentCultureIgnoreCase)) {

                    var response = doc.Descendants("link").FirstOrDefault();
                    string url = string.Format("{0}://{1}{2}", response.Element("protocol").Value, response.Element("address").Value, response.Element("path").Value);
                    
                    string filePath = Path.Combine(Environment.CurrentDirectory, "test.mp4");
                    
                    col = new NameValueCollection();
                    FileStream fs = new FileStream(filePath, FileMode.Open);

                    col["file_size"] = fs.Length.ToString();
                    col["file_md5"] = BitConverter.ToString(HashAlgorithm.Create("MD5").ComputeHash(fs)).Replace("-", "").ToLower();
                    col["key"] = response.Element("query").Element("key").Value;
                    col["token"] = response.Element("query").Element("token").Value;

                    fs.Dispose();
                    string uploadResponse = api.Upload(url, col, filePath);

                    Console.WriteLine(uploadResponse);
                }

            } catch (Exception ex) {
                Console.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
