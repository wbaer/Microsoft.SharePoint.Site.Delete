#region Imports
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Text;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.StsAdmin;
#endregion
[assembly: CLSCompliant(true)]

namespace Microsoft.SharePoint.Site.Nibbler
{
    [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
    public class DeleteCommands : ISPStsadmCommand
    {
        /// <summary>
        /// Gets the help message.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        #region GetHelpMessage
        public string GetHelpMessage(string command)
        {
            return "-url <url>";
        }
        #endregion
        /// <summary>
        /// Runs the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="keyValues">The key values.</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        #region Run
        public int Run(string command, StringDictionary keyValues, out string output)
        {
            command = command.ToLowerInvariant();

            switch (command)
            {
                case "nibblesite":
                    return this.NibbleSite(keyValues, out output);

                default:
                    throw new InvalidOperationException();
            }
        }
        #endregion
        /// <summary>
        /// Initializes a new instance of the <see cref="NibbleSite"/> class.
        /// </summary>
        private int NibbleSite(StringDictionary keyValues, out string output)
        {
            if (!keyValues.ContainsKey("url"))
            {
                throw new InvalidOperationException("Syntax error in argument: url.");
            }

            String url = keyValues["url"];
            StringBuilder sb = new StringBuilder();

            SPSite site = new SPSite(url);

            NibbleWeb(site.OpenWeb(), true);

            try
            {
                site.Delete();
            }
            catch
            {
                throw new Exception("Site " + url + " delete failed");
            }
            output = sb.ToString();
            return 0;
        }
        static void NibbleWeb(SPWeb web, bool bRoot)
        {
            if (web.Webs.Count != 0)    
            {
                ArrayList webs = new ArrayList(web.Webs.Count);

                foreach (SPWeb webT in web.Webs)
                {
                    webs.Add(webT);
                }
                foreach (SPWeb webT in webs)
                {
                    NibbleWeb(webT, false);
                }
            }
            NibbleLists(web, web.Url);
            if (!bRoot)
            {
                string url = web.Url;
                try
                {
                    web.Delete();
                }
                catch
                {
                    throw new Exception("Web " + url + " delete failed");
                }
            }
        }
        static void NibbleLists(SPWeb web, string strWebUrl)
        {
            ArrayList lists = new ArrayList(web.Lists.Count);
            foreach (SPList list in web.Lists)
            {
                if (!list.Hidden)
                {
                    lists.Add(list);
                }
            }
            foreach (SPList list in lists)
            {
                NibbleList(list, strWebUrl);
            }
        }
        static void NibbleList(SPList list, string strWebUrl)
        {
            NibbleFolder(list.RootFolder, true, strWebUrl);

            string url = strWebUrl + "/" + list.Title;
            try
            {
                list.Delete();
                //Console.WriteLine("List " + url + " deleted.");
            }
            catch
            {
                throw new Exception("List " + url + " delete failed");
                //Console.WriteLine("List " + url + " delete failed");
            }
        }
        static void NibbleFolder(SPFolder folder, bool bRoot, string strWebUrl)
        {
            if (folder.SubFolders.Count != 0)
            {
                ArrayList folders = new ArrayList(folder.SubFolders.Count);

                foreach (SPFolder folderT in folder.SubFolders)
                {
                    folders.Add(folderT);
                }

                foreach (SPFolder folderT in folders)
                {
                    NibbleFolder(folderT, false, strWebUrl);
                }
            }
            if (!bRoot)
            {
                string url = strWebUrl + "/" + folder.Url;
                try
                {
                    folder.Delete();
                    //Console.WriteLine("Folder " + url + " deleted.");
                }
                catch
                {
                    throw new Exception("Folder " + url + " delete failed");
                    //Console.WriteLine("Folder " + url + " delete failed");
                }
            }
        }
    }
}
