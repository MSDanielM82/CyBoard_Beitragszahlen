using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CyBoard_Beitragszahlen
{
    public partial class CyBoard_Beitragszahlen : Form
    {
        string CSVFile = "";
        string Seiten;
        string filename;
        string UserBeitraege;
        string UserReaktionen;
        string UserPunkte;
        string UserTrophaen;

        public CyBoard_Beitragszahlen()
        {
            InitializeComponent();
            numericUpDown1.TextChanged += new EventHandler(NumericUpDown1_ValueChanged); //EventHandler, damit der Button Text aktualisiert wird
        }
        private void NumericUpDown1_ValueChanged(Object sender, EventArgs e)
        {
            btn_StartSeiteX.Text = "Ab Seite " + numericUpDown1.Value.ToString() + " starten";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //Direkt die Member List anzeigen, zur Überprüfung, ob man eingeloggt ist
            webBrowser1.Navigate("https://www.moonsault.de/members-list/?pageNo=1&sortField=wbbPosts&sortOrder=DESC"); 
        }


        /// <summary>
        /// Es wird die Webseite geladen und gewartet, bis der Ladevorgang abgeschlossen wurde
        /// </summary>
        /// <param name="URL"></param>
        public void LoadPage(string URL)
        {
            webBrowser1.Navigate(URL);
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }
        }
        

    public void CreateCSV()
        {
            filename = @"c:\cyboard_postingzahlen\export_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".csv";
            using (StreamWriter w = File.AppendText(filename))
            {
                CSVFile = "UserID" + ";" + "UserName" + ";" + "UserBeitraege" + ";" + "UserReaktionen" + ";" + "UserPunkte" + ";" + "UserTrophaen" + ";" + "Datum" + ";" + "Uhrzeit" + ";" + "Seite" + ";" + System.Environment.NewLine;
                w.WriteLine(CSVFile);
                w.Close();
            }
        }

        private void btn_StartSeite1_Click(object sender, EventArgs e)
        {
            CreateCSV(); //CSV wird erstellt
            DurchlaufSeiten(1); //
            
        }

        public void ErmittlungSeitenanzahl()
        {

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(webBrowser1.DocumentText);
            foreach (HtmlNode seiten in doc.DocumentNode.SelectNodes("//div[@class='paginationBottom']"))
            {
                var pageshtml = new HtmlAgilityPack.HtmlDocument();
                pageshtml.LoadHtml(seiten.InnerHtml);
                Seiten = pageshtml.DocumentNode.SelectNodes("nav").First().Attributes["data-pages"].Value;
                MessageBox.Show("Anzahl Seiten:" + Seiten);
            }
        }
        public void DurchlaufSeiten(int pagestart)
        {
            ErmittlungSeitenanzahl();

            int page = pagestart;
            int page_total = int.Parse(Seiten);

            while (page < page_total+1)
            {
                LoadPage(@"https://www.moonsault.de/members-list/?pageNo=" + page.ToString() + "&sortField=wbbPosts&sortOrder=DESC");
                WebseiteAuslesen(page);
                lbl_fortschritt.Text = page.ToString() + "/" + page_total.ToString();
                page++;

            }
            }
     
        private void WebseiteAuslesen(int page)
        { 
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(webBrowser1.DocumentText);
            
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//ol[@class='containerList userList']"))
            {
                var innerhtmlDoc = new HtmlAgilityPack.HtmlDocument();
                innerhtmlDoc.LoadHtml(node.InnerHtml);

                var htmlNodes = innerhtmlDoc.DocumentNode.SelectNodes("//li/div[@class='box48']");
                foreach (var nodes in htmlNodes)
                {
                    //Initialisierung der Beiträge, Reaktionen, Trophaen und Punkte
                    UserBeitraege = "0";
                    UserReaktionen = "0";
                    UserTrophaen = "0";
                    UserPunkte = "0";

                    //Auslesen des Namen und der ID
                    var tmp_html = new HtmlAgilityPack.HtmlDocument();
                    tmp_html.LoadHtml(nodes.InnerHtml);
                    
                    string UserName = tmp_html.DocumentNode
                       .SelectNodes("//a")
                       .First()
                       .Attributes["title"].Value;
                    string UserID = tmp_html.DocumentNode
                        .SelectNodes("//a[@class='userLink username']")
                        .First()
                        .Attributes["data-object-id"]
                        .Value;
                    

                    //Für jeden DL Knoten werden die Daten ausgelesen. Beiträge, Reaktionen, Punkte, Trophaen
                    int counter = 0;
                    foreach (HtmlNode infos in tmp_html.DocumentNode.SelectNodes("//dl[@class='plain inlineDataList small']/dd"))
                    {
                        switch (counter)
                        {
                            case 0:
                                UserBeitraege = infos.InnerText;
                                break;
                            case 1:
                                UserReaktionen = infos.InnerText;
                                break;
                            case 2:
                                UserPunkte = infos.InnerText;
                                break;
                            case 3:
                                UserTrophaen = infos.InnerText;
                                break;
                        }
                        counter++;
                    }

                    //CSV Schreiben
                    using (StreamWriter w = File.AppendText(filename))
                    {
                        CSVFile = UserID + ";" + UserName + ";" + UserBeitraege.Replace(".", "") + ";" + UserReaktionen.Replace(".", "") + ";" + UserPunkte.Replace(".", "") + ";" + UserTrophaen.Replace(".", "") + ";" + DateTime.Now.ToString("d") + ";" + DateTime.Now.ToString("HH:mm:ss") + ";" + page.ToString() + ";" + System.Environment.NewLine;
                        w.WriteLine(CSVFile);
                        w.Close();
                    }

                   

                }
            }

                
            
        }
      

        private void btn_StartSeiteX_Click(object sender, EventArgs e)
        {
            CreateCSV();
            LoadPage(@"https://www.moonsault.de/members-list/?pageNo=1&sortField=wbbPosts&sortOrder=DESC&letter=");
            int start = int.Parse(numericUpDown1.Value.ToString());
            DurchlaufSeiten(start);
        }

        private void beendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
    }

