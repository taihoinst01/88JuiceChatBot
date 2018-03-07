using JuiceChatBot.Dialogs;
using JuiceChatBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace JuiceChatBot.DB
{
    public class DbConnect
    {
        static Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("/");
        const string CONSTRINGNAME = "conString";
        //ConnectionStringSettings connStr = rootWebConfig.ConnectionStrings.ConnectionStrings[CONSTRINGNAME]
        string connStr = rootWebConfig.ConnectionStrings.ConnectionStrings[CONSTRINGNAME].ToString();
        //string connStr = "Data Source=taiholab.database.windows.net;Initial Catalog=taihoLab_2;User ID=taihoinst;Password=taiho9788!;";
        //string connStr = "Data Source=10.6.222.21,1433;Initial Catalog=konadb;User ID=konadb;Password=Didwoehd20-9!;";
        //StringBuilder sb = new StringBuilder();
        public readonly string TEXTDLG = "2";
        public readonly string CARDDLG = "3";
        public readonly string MEDIADLG = "4";

        public void ConnectDb()
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

        }


        public List<DialogList> SelectInitDialog(String channel)
        {
            SqlDataReader rdr = null;
            List<DialogList> dialogs = new List<DialogList>();
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT   				    ";
                cmd.CommandText += " 	DLG_ID,                 ";
                cmd.CommandText += " 	DLG_TYPE,               ";
                cmd.CommandText += " 	DLG_GROUP,              ";
                cmd.CommandText += " 	DLG_ORDER_NO            ";
                cmd.CommandText += " FROM TBL_DLG     ";
                cmd.CommandText += " WHERE DLG_GROUP = '1'      ";
                cmd.CommandText += " AND USE_YN = 'Y'           ";
                cmd.CommandText += " ORDER BY DLG_ID            ";

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                DButil.HistoryLog(" db SelectInitDialog !! ");
                
                                
                while (rdr.Read())
                {
                    DialogList dlg = new DialogList();
                    dlg.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    dlg.dlgType = rdr["DLG_TYPE"] as string;
                    dlg.dlgGroup = rdr["DLG_GROUP"] as string;
                    dlg.dlgOrderNo = rdr["DLG_ORDER_NO"] as string;
                    
                    using (SqlConnection conn2 = new SqlConnection(connStr))
                    {
                        SqlCommand cmd2 = new SqlCommand();
                        conn2.Open();
                        cmd2.Connection = conn2;
                        SqlDataReader rdr2 = null;
                        if (dlg.dlgType.Equals(TEXTDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                            }
                            rdr2.Close();
                        } else if (dlg.dlgType.Equals(CARDDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, IMG_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT, " +
                                    "CARD_DIVISION, CARD_VALUE " +
                                    "FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' ";
                            //if (channel.Equals("facebook"))
                            //{
                            //    cmd2.CommandText += "FB_USE_YN = 'Y' ";
                            //}
                            cmd2.CommandText += "ORDER BY CARD_ORDER_NO";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);
                            List<CardList> dialogCards = new List<CardList>();
                            while (rdr2.Read())
                            {
                                CardList dlgCard = new CardList();
                                dlgCard.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlgCard.cardSubTitle = rdr2["CARD_SUBTITLE"] as string;
                                dlgCard.cardText = rdr2["CARD_TEXT"] as string;
                                dlgCard.imgUrl = rdr2["IMG_URL"] as string;
                                dlgCard.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlgCard.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlgCard.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlgCard.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlgCard.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlgCard.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlgCard.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlgCard.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlgCard.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlgCard.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlgCard.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlgCard.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                                dlgCard.cardDivision = rdr2["CARD_DIVISION"] as string;
                                dlgCard.cardValue = rdr2["CARD_VALUE"] as string;
                                dialogCards.Add(dlgCard);
                            }
                            dlg.dialogCard = dialogCards;
                        } else if (dlg.dlgType.Equals(MEDIADLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT, MEDIA_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT " +
                                    "FROM TBL_DLG_MEDIA WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                                dlg.mediaUrl = rdr2["MEDIA_URL"] as string;
                                dlg.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlg.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlg.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlg.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlg.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlg.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlg.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlg.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlg.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlg.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlg.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlg.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                            }
                        }
                        
                    }
                    dialogs.Add(dlg);
                }
                rdr.Close();
            }
            return dialogs;
        }

        public DialogList SelectDialog(int dlgID)
        {
            SqlDataReader rdr = null;
            DialogList dlg = new DialogList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT   				    ";
                cmd.CommandText += " 	DLG_ID,                 ";
                cmd.CommandText += " 	DLG_NAME,               ";
                cmd.CommandText += " 	DLG_DESCRIPTION,        ";
                cmd.CommandText += " 	DLG_LANG,               ";
                cmd.CommandText += " 	DLG_TYPE,               ";
                cmd.CommandText += " 	DLG_ORDER_NO,           ";
                cmd.CommandText += " 	DLG_GROUP               ";
                cmd.CommandText += " FROM TBL_DLG               ";
                cmd.CommandText += " WHERE DLG_ID = @dlgId      ";
                cmd.CommandText += " AND USE_YN = 'Y'           ";
                cmd.CommandText += " ORDER BY  DLG_ORDER_NO     ";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    dlg.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    dlg.dlgType = rdr["DLG_TYPE"] as string;
                    dlg.dlgGroup = rdr["DLG_GROUP"] as string;
                    dlg.dlgOrderNo = rdr["DLG_ORDER_NO"] as string;

                    using (SqlConnection conn2 = new SqlConnection(connStr))
                    {
                        SqlCommand cmd2 = new SqlCommand();
                        conn2.Open();
                        cmd2.Connection = conn2;
                        SqlDataReader rdr2 = null;
                        if (dlg.dlgType.Equals(TEXTDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                            }
                            rdr2.Close();
                        }
                        else if (dlg.dlgType.Equals(CARDDLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, IMG_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT, " +
                                    "CARD_DIVISION, CARD_VALUE, CARD_ORDER_NO " +
                                    "FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' ORDER BY CARD_ORDER_NO";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);
                            List<CardList> dialogCards = new List<CardList>();
                            while (rdr2.Read())
                            {
                                CardList dlgCard = new CardList();
                                dlgCard.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlgCard.cardSubTitle = rdr2["CARD_SUBTITLE"] as string;
                                dlgCard.cardText = rdr2["CARD_TEXT"] as string;
                                dlgCard.imgUrl = rdr2["IMG_URL"] as string;
                                dlgCard.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlgCard.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlgCard.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlgCard.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlgCard.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlgCard.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlgCard.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlgCard.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlgCard.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlgCard.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlgCard.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlgCard.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                                dlgCard.cardDivision = rdr2["CARD_DIVISION"] as string;
                                dlgCard.cardValue = rdr2["CARD_VALUE"] as string;
                                //dlgCard.card_order_no = rdr2["CARD_ORDER_NO"] as string;
                                dlgCard.card_order_no = Convert.ToInt32(rdr2["CARD_ORDER_NO"]);

                                dialogCards.Add(dlgCard);
                            }
                            dlg.dialogCard = dialogCards;
                        }
                        else if (dlg.dlgType.Equals(MEDIADLG))
                        {
                            cmd2.CommandText = "SELECT CARD_TITLE, CARD_TEXT, MEDIA_URL," +
                                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, BTN_4_TYPE, BTN_4_TITLE, BTN_4_CONTEXT , CARD_DIVISION, CARD_VALUE " +
                                    "FROM TBL_DLG_MEDIA WHERE DLG_ID = @dlgID AND USE_YN = 'Y'";
                            cmd2.Parameters.AddWithValue("@dlgID", dlg.dlgId);
                            rdr2 = cmd2.ExecuteReader(CommandBehavior.CloseConnection);

                            while (rdr2.Read())
                            {
                                dlg.cardTitle = rdr2["CARD_TITLE"] as string;
                                dlg.cardText = rdr2["CARD_TEXT"] as string;
                                dlg.mediaUrl = rdr2["MEDIA_URL"] as string;
                                dlg.btn1Type = rdr2["BTN_1_TYPE"] as string;
                                dlg.btn1Title = rdr2["BTN_1_TITLE"] as string;
                                dlg.btn1Context = rdr2["BTN_1_CONTEXT"] as string;
                                dlg.btn2Type = rdr2["BTN_2_TYPE"] as string;
                                dlg.btn2Title = rdr2["BTN_2_TITLE"] as string;
                                dlg.btn2Context = rdr2["BTN_2_CONTEXT"] as string;
                                dlg.btn3Type = rdr2["BTN_3_TYPE"] as string;
                                dlg.btn3Title = rdr2["BTN_3_TITLE"] as string;
                                dlg.btn3Context = rdr2["BTN_3_CONTEXT"] as string;
                                dlg.btn4Type = rdr2["BTN_4_TYPE"] as string;
                                dlg.btn4Title = rdr2["BTN_4_TITLE"] as string;
                                dlg.btn4Context = rdr2["BTN_4_CONTEXT"] as string;
                                dlg.cardDivision = rdr2["CARD_DIVISION"] as string;
                                dlg.cardValue = rdr2["CARD_VALUE"] as string;
                            }
                        }

                    }
                }
            }
            return dlg;
        }

        public List<CardList> SelectDialogCard(int dlgID)
        {
            SqlDataReader rdr = null;
            List<CardList> dialogCard = new List<CardList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT CARD_DLG_ID, DLG_ID, CARD_TITLE, CARD_SUBTITLE, CARD_TEXT, IMG_URL," +
                    "BTN_1_TYPE, BTN_1_TITLE, BTN_1_CONTEXT, BTN_2_TYPE, BTN_2_TITLE, BTN_2_CONTEXT, BTN_3_TYPE, BTN_3_TITLE, BTN_3_CONTEXT, " +
                    "CARD_DIVISION, CARD_VALUE " +
                    "FROM TBL_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999 ORDER BY CARD_ORDER_NO";
                    //"FROM TBL_SECCS_DLG_CARD WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999 ORDER BY CARD_ORDER_NO";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int cardDlgId = Convert.ToInt32(rdr["CARD_DLG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardSubTitle = rdr["CARD_SUBTITLE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;
                    string imgUrl = rdr["IMG_URL"] as string;
                    string btn1Type = rdr["BTN_1_TYPE"] as string;
                    string btn1Title = rdr["BTN_1_TITLE"] as string;
                    string btn1Context = rdr["BTN_1_CONTEXT"] as string;
                    string btn2Type = rdr["BTN_2_TYPE"] as string;
                    string btn2Title = rdr["BTN_2_TITLE"] as string;
                    string btn2Context = rdr["BTN_2_CONTEXT"] as string;
                    string btn3Type = rdr["BTN_3_TYPE"] as string;
                    string btn3Title = rdr["BTN_3_TITLE"] as string;
                    string btn3Context = rdr["BTN_3_CONTEXT"] as string;
                    string cardDivision = rdr["CARD_DIVISION"] as string;
                    string cardValue = rdr["CARD_VALUE"] as string;

                    CardList dlgCard = new CardList();
                    dlgCard.cardDlgId = cardDlgId;
                    dlgCard.dlgId = dlgId;
                    dlgCard.cardTitle = cardTitle;
                    dlgCard.cardSubTitle = cardSubTitle;
                    dlgCard.cardText = cardText;
                    dlgCard.imgUrl = imgUrl;
                    dlgCard.btn1Type = btn1Type;
                    dlgCard.btn1Title = btn1Title;
                    dlgCard.btn1Context = btn1Context;
                    dlgCard.btn2Type = btn2Type;
                    dlgCard.btn2Title = btn2Title;
                    dlgCard.btn2Context = btn2Context;
                    dlgCard.btn3Type = btn3Type;
                    dlgCard.btn3Title = btn3Title;
                    dlgCard.btn3Context = btn3Context;
                    dlgCard.cardDivision = cardDivision;
                    dlgCard.cardValue = cardValue;

                    dialogCard.Add(dlgCard);
                }
            }
            return dialogCard;
        }

        public List<TextList> SelectDialogText(int dlgID)
        {
            SqlDataReader rdr = null;
            List<TextList> dialogText = new List<TextList>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE, CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";
                //cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE, CARD_TEXT FROM TBL_SECCS_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgID", dlgID);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int textDlgId = Convert.ToInt32(rdr["TEXT_DLG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;


                    TextList dlgText = new TextList();
                    dlgText.textDlgId = textDlgId;
                    dlgText.dlgId = dlgId;
                    dlgText.cardTitle = cardTitle;
                    dlgText.cardText = cardText;


                    dialogText.Add(dlgText);
                }
            }
            return dialogText;
        }


        public List<TextList> SelectSorryDialogText(string dlgGroup)
        {
            SqlDataReader rdr = null;
            List<TextList> dialogText = new List<TextList>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE,CARD_TEXT FROM TBL_DLG_TEXT WHERE DLG_ID = (SELECT DLG_ID FROM TBL_DLG WHERE DLG_GROUP = @dlgGroup) AND USE_YN = 'Y'";
                //cmd.CommandText = "SELECT TEXT_DLG_ID, DLG_ID, CARD_TITLE, CARD_TEXT FROM TBL_SECCS_DLG_TEXT WHERE DLG_ID = @dlgID AND USE_YN = 'Y' AND DLG_ID > 999";

                cmd.Parameters.AddWithValue("@dlgGroup", dlgGroup);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    int textDlgId = Convert.ToInt32(rdr["TEXT_DLG_ID"]);
                    int dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    string cardTitle = rdr["CARD_TITLE"] as string;
                    string cardText = rdr["CARD_TEXT"] as string;


                    TextList dlgText = new TextList();
                    dlgText.textDlgId = textDlgId;
                    dlgText.dlgId = dlgId;
                    dlgText.cardTitle = cardTitle;
                    dlgText.cardText = cardText;


                    dialogText.Add(dlgText);
                }
            }
            return dialogText;
        }
        
        
        //KSO START
        public CardList BannedChk(string orgMent)
        {
            SqlDataReader rdr = null;
            CardList SelectBanned = new CardList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " SELECT                                                                                                                                                         ";
                cmd.CommandText += " TOP 1 TD.DLG_ID, (SELECT TOP 1 BANNED_WORD FROM TBL_BANNED_WORD_LIST WHERE CHARINDEX(BANNED_WORD, @msg) > 0) AS BANNED_WORD, TDT.CARD_TITLE, TDT.CARD_TEXT     ";
                cmd.CommandText += " FROM TBL_DLG TD, TBL_DLG_TEXT TDT                                                                                                                              ";
                cmd.CommandText += " WHERE TD.DLG_ID = TDT.DLG_ID                                                                                                                                   ";
                cmd.CommandText += " AND                                                                                                                                                            ";
                cmd.CommandText += " 	TD.DLG_GROUP =                                                                                                                                              ";
                cmd.CommandText += " 	(                                                                                                                                                           ";
                cmd.CommandText += " 	   SELECT CASE WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 3 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 3                                                  ";
                cmd.CommandText += " 			  WHEN SUM(CASE WHEN BANNED_WORD_TYPE = 4 THEN CHARINDEX(A.BANNED_WORD, @msg) END) > 0 THEN 4                                                       ";
                cmd.CommandText += " 			 END                                                                                                                                                ";
                cmd.CommandText += " 	   FROM TBL_BANNED_WORD_LIST A                                                                                                                              ";
                cmd.CommandText += " 	) AND TD.DLG_GROUP IN (3,4)                                                                                                                                 ";
                cmd.CommandText += " ORDER BY NEWID()                                                                                                                                               ";

                cmd.Parameters.AddWithValue("@msg", orgMent);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    //answerMsg = rdr["CARD_TEXT"] + "@@" + rdr["DLG_ID"] + "@@" + rdr["CARD_TITLE"];

                    int dlg_id = Convert.ToInt32(rdr["DLG_ID"]);
                    String card_title = rdr["CARD_TITLE"] as String;
                    String card_text = rdr["CARD_TEXT"] as String;
                    
                    SelectBanned.dlgId = dlg_id;
                    SelectBanned.cardTitle = card_title;
                    SelectBanned.cardText = card_text;
                }
            }
            return SelectBanned;
        }

        public CacheList CacheChk(string orgMent)
        {
            SqlDataReader rdr = null;
            CacheList result = new CacheList();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, ISNULL(LUIS_INTENT_SCORE,'') AS LUIS_INTENT_SCORE FROM TBL_QUERY_ANALYSIS_RESULT WHERE LOWER(QUERY) = LOWER(@msg) AND RESULT ='H'";

                cmd.Parameters.AddWithValue("@msg", orgMent);
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                
                /*
                if (rdr.Read())
                {
                    Debug.WriteLine("* YES - TBL_QUERY_ANALYSIS_RESULT");
                }
                else
                {
                    Debug.WriteLine("* NO - TBL_QUERY_ANALYSIS_RESULT");
                }
                */

                while (rdr.Read())
                {
                    string luisId = rdr["LUIS_ID"] as String;
                    string intentId = rdr["LUIS_INTENT"] as String;
                    string entitiesId = rdr["LUIS_ENTITIES"] as String;
                    string luisScore = rdr["LUIS_INTENT_SCORE"] as String;
                    
                    result.luisId = luisId;
                    result.luisIntent = intentId;
                    result.luisEntities = entitiesId;
                    result.luisScore = luisScore;

                    Debug.WriteLine("Yes rdr | intentId : " + intentId + " | entitiesId : "+ entitiesId + " | luisScore : " + luisScore);
                }

            }
            return result;
        }

        public List<RelationList> DefineTypeChk(string luisId, string intentId, string entitiesId)
        {
            SqlDataReader rdr = null;
            List<RelationList> result = new List<RelationList>();
            Debug.WriteLine("luisId ::: "+ luisId);
            Debug.WriteLine("intentId ::: " + intentId);
            Debug.WriteLine("entitiesId ::: " + entitiesId);
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, ISNULL(DLG_ID,0) AS DLG_ID, DLG_API_DEFINE, API_ID ";
                cmd.CommandText += "  FROM TBL_DLG_RELATION_LUIS                                                    ";
                cmd.CommandText += " WHERE 1=1                                               ";
                //cmd.CommandText += " WHERE LUIS_INTENT = @intentId                                                 ";
                cmd.CommandText += "   AND LUIS_ENTITIES = @entities                                                ";
                //cmd.CommandText += "   AND LUIS_ID = @luisId                                                        ";

                if(intentId != null){
                    cmd.Parameters.AddWithValue("@intentId", intentId);
                }else{
                    cmd.Parameters.AddWithValue("@intentId", DBNull.Value);
                }

                if (entitiesId != null){
                    cmd.Parameters.AddWithValue("@entities", entitiesId);
                }else{
                    cmd.Parameters.AddWithValue("@entities", DBNull.Value);
                }

                if (luisId != null){
                    cmd.Parameters.AddWithValue("@luisId", luisId);
                }
                else{
                    cmd.Parameters.AddWithValue("@luisId", DBNull.Value);
                }


                

                Debug.WriteLine("query : " + cmd.CommandText);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    RelationList relationList = new RelationList();
                    relationList.luisId = rdr["LUIS_ID"] as string;
                    relationList.luisIntent = rdr["LUIS_INTENT"] as string;
                    relationList.luisEntities = rdr["LUIS_ENTITIES"] as string;
                    relationList.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    relationList.dlgApiDefine = rdr["DLG_API_DEFINE"] as string;
                    //relationList.apiId = Convert.ToInt32(rdr["API_ID"] ?? 0);
                    relationList.apiId = rdr["API_ID"].Equals(DBNull.Value)? 0 : Convert.ToInt32(rdr["API_ID"]) ;
                    //DBNull.Value
                    result.Add(relationList);
                }
            }
            return result;
        }

        public List<RelationList> DefineTypeChkSpare(string entity)
        {
            SqlDataReader rdr = null;
            List<RelationList> result = new List<RelationList>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += "SELECT  LUIS_ID, LUIS_INTENT, LUIS_ENTITIES, ISNULL(DLG_ID,0) AS DLG_ID, DLG_API_DEFINE, API_ID ";
                cmd.CommandText += "  FROM  TBL_DLG_RELATION_LUIS                                                    ";
                cmd.CommandText += " WHERE  LUIS_ENTITIES = @entities                                                ";

                Debug.WriteLine("query : " + cmd.CommandText);
                cmd.Parameters.AddWithValue("@entities", entity);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (rdr.Read())
                {
                    RelationList relationList = new RelationList();
                    relationList.luisId = rdr["LUIS_ID"] as string;
                    relationList.luisIntent = rdr["LUIS_INTENT"] as string;
                    relationList.luisEntities = rdr["LUIS_ENTITIES"] as string;
                    relationList.dlgId = Convert.ToInt32(rdr["DLG_ID"]);
                    relationList.dlgApiDefine = rdr["DLG_API_DEFINE"] as string;
                    //relationList.apiId = Convert.ToInt32(rdr["API_ID"] ?? 0);
                    relationList.apiId = rdr["API_ID"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(rdr["API_ID"]);
                    //DBNull.Value
                    result.Add(relationList);
                }
            }
            return result;
        }


        //KSO END

        //TBL_CHATBOT_CONF 정보 가져오기
        //      LUIS_APP_ID	    - 루이스APP_ID
        //      LUIS_TIME_LIMIT - 루이스제한
        //      LUIS_SCORE_LIMIT - 스코어 제한
        //      LUIS_SUBSCRIPTION   - 루이스구독
        //      BOT_NAME        - 봇이름?
        //      BOT_APP_ID      - 봇앱아이디?
        //      BOT_APP_PASSWORD- 봇앱패스워드?
        //      QUOTE           - 견적url
        //      TESTDRIVE       - 시승url
        //      CATALOG         - 카달로그url
        //      DISCOUNT        - 할인url
        //      EVENT           - 이벤트url

        public List<ConfList> SelectConfig()
        //public List<ConfList> SelectConfig(string config_type)
        {
            SqlDataReader rdr = null;
            List<ConfList> conflist = new List<ConfList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                DButil.HistoryLog("db conn SelectConfig !!");
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = " SELECT CNF_TYPE, CNF_NM, CNF_VALUE" +
                                  " FROM TBL_CHATBOT_CONF " +
                                  //" WHERE CNF_TYPE = 'LUIS_APP_ID' " +
                                  " ORDER BY CNF_TYPE DESC, ORDER_NO ASC ";

                Debug.WriteLine("* cmd.CommandText : " + cmd.CommandText);
                //cmd.Parameters.AddWithValue("@config_type", config_type);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string cnfType = rdr["CNF_TYPE"] as string;
                    string cnfNm = rdr["CNF_NM"] as string;
                    string cnfValue = rdr["CNF_VALUE"] as string;

                    ConfList list = new ConfList();

                    list.cnfType = cnfType;
                    list.cnfNm = cnfNm;
                    list.cnfValue = cnfValue;


                    Debug.WriteLine("* cnfNm : " + cnfNm + " || cnfValue : " + cnfValue);
                    DButil.HistoryLog("* cnfNm : " + cnfNm + " || cnfValue : " + cnfValue);
                    conflist.Add(list);
                }
            }
            return conflist;
        }

		public string SelectChgMsg(string oldMsg)
		{
			SqlDataReader rdr = null;
			string newMsg = "";

			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;

				cmd.CommandText += "	SELECT FIND.CHG  CHG_WORD FROM(    					    ";
				cmd.CommandText += "	SELECT                                                  ";
				cmd.CommandText += "			CASE WHEN LEN(ORG_WORD) = LEN(@oldMsg)          ";
				cmd.CommandText += "				THEN CHARINDEX(ORG_WORD, @oldMsg)           ";
				cmd.CommandText += "				ELSE 0                                      ";
				cmd.CommandText += "				END AS FIND_NUM,                            ";
				cmd.CommandText += "				REPLACE(@oldMsg, ORG_WORD, CHG_WORD) CHG    ";
				cmd.CommandText += "	  FROM TBL_WORD_CHG_DICT                                ";
				cmd.CommandText += "	  ) FIND                                                ";
				cmd.CommandText += "	  WHERE FIND.FIND_NUM > 0                               ";





				cmd.Parameters.AddWithValue("@oldMsg", oldMsg);

				rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

				while (rdr.Read())
				{
					newMsg = rdr["CHG_WORD"] as string;
				}
			}
			return newMsg;
		}
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Query Analysis
		// Insert user chat message for history and analysis
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int insertUserQuery()
		{
			int dbResult = 0;
			using (SqlConnection conn = new SqlConnection(connStr))
			{
                String luisID = "", intentName = "", entities = "", result = "", intentScore = "";

                int appID = 0,luisScore = 0;


                //if(MessagesController.recommendResult != "recommend")
                if(MessagesController.apiFlag != "RECOMMEND")
                {
                    //if (MessagesController.relationList.Equals(null))
                    if (MessagesController.relationList == null)
                    {
                        entities = "None";
                        intentName = "None";
                        luisID = "None";
                        luisScore = 0;
                    }
                    else
                    {

                        if (MessagesController.relationList.Count() > 0)
                        {
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisId))
                            {
                                luisID = "None";
                            }
                            else
                            {
                                luisID = MessagesController.relationList[0].luisId;
                            }
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisIntent))
                            {
                                intentName = "None";
                            }
                            else
                            {
                                intentName = MessagesController.relationList[0].luisIntent;
                            }
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisEntities))
                            {
                                entities = "None";
                            }
                            else
                            {
                                entities = MessagesController.relationList[0].luisEntities;
                            }
                            if (String.IsNullOrEmpty(MessagesController.relationList[0].luisScore.ToString()))
                            {
                                intentScore = "0";
                            }
                            else
                            {
                                intentScore = MessagesController.relationList[0].luisScore.ToString();
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisId))
                            {
                                luisID = "None";
                            }
                            else
                            {
                                luisID = MessagesController.cacheList.luisId;
                            }
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisIntent))
                            {
                                intentName = "None";
                            }
                            else
                            {
                                intentName = MessagesController.cacheList.luisIntent;
                            }
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisEntities))
                            {
                                entities = "None";
                            }
                            else
                            {
                                entities = MessagesController.cacheList.luisEntities;
                            }
                            if (String.IsNullOrEmpty(MessagesController.cacheList.luisScore))
                            {
                                intentScore = "0";
                            }
                            else
                            {
                                intentScore = MessagesController.cacheList.luisScore;
                            }
                        }
                    }

                    if (String.IsNullOrEmpty(MessagesController.replyresult))
                    {
                        result = "D";
                    }
                    else
                    {
                        result = MessagesController.replyresult;
                    }
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "sp_insertusehistory4";

                    cmd.CommandType = CommandType.StoredProcedure;


                    //if (result.Equals("S") || result.Equals("D"))
                    //{
                    //    cmd.Parameters.AddWithValue("@Query", "");
                    //    cmd.Parameters.AddWithValue("@intentID", "");
                    //    cmd.Parameters.AddWithValue("@entitiesIDS", "");
                    //    cmd.Parameters.AddWithValue("@intentScore", "");
                    //    cmd.Parameters.AddWithValue("@luisID", "");
                    //    cmd.Parameters.AddWithValue("@result", result);
                    //    cmd.Parameters.AddWithValue("@appID", appID);
                    //}
                    //else
                    //{
                    Debug.WriteLine("DDDDDD : " + Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline).Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Query", Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline).Trim().ToLower());
                    cmd.Parameters.AddWithValue("@intentID", intentName.Trim());
                    cmd.Parameters.AddWithValue("@entitiesIDS", entities.Trim().ToLower());
                    if(result.Equals("D") || result.Equals("S"))
                    {
                        cmd.Parameters.AddWithValue("@intentScore", "0");
                    }
                    else
                    {
                        //if(MessagesController.relationList != null)
                        //{
                            if (MessagesController.relationList.Count > 0 && MessagesController.relationList[0].luisEntities != null)
                            {
                                cmd.Parameters.AddWithValue("@intentScore", MessagesController.relationList[0].luisScore);
                            }
                        //}
                        else
                        {
                            cmd.Parameters.AddWithValue("@intentScore", MessagesController.cacheList.luisScore);
                        }
                    }
                    cmd.Parameters.AddWithValue("@luisID", luisID);
                    cmd.Parameters.AddWithValue("@result", result);
                    cmd.Parameters.AddWithValue("@appID", appID);
                    //}

                    dbResult = cmd.ExecuteNonQuery();
                }

                
			}
			return dbResult;
		}

        public int insertUserQuery(string korQuery, string intentID, string entitiesIDS, string intentScore, String luisID, char result, int appID)
        {
            int dbResult = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "sp_insertusehistory4";

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Query", korQuery.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentID", intentID.Trim());
                cmd.Parameters.AddWithValue("@entitiesIDS", entitiesIDS.Trim().ToLower());
                cmd.Parameters.AddWithValue("@intentScore", intentScore.Trim().ToLower());
                cmd.Parameters.AddWithValue("@luisID", luisID);
                cmd.Parameters.AddWithValue("@result", result);
                cmd.Parameters.AddWithValue("@appID", appID);


                dbResult = cmd.ExecuteNonQuery();
            }
            return dbResult;
        }



        public int insertHistory(string userNumber, string channel, int responseTime)
		{
            //SqlDataReader rdr = null;
            int appID = 0;
            int result;
            String intentName = "";

            //if (MessagesController.relationList.Equals(null))
            if (MessagesController.relationList == null)
            {
                intentName = "None";
            }
            else
            {
                if (MessagesController.relationList.Count() > 0)
                {
                    if (String.IsNullOrEmpty(MessagesController.relationList[0].luisIntent))
                    {
                        intentName = "None";
                    }
                    else
                    {
                        intentName = MessagesController.relationList[0].luisIntent;
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(MessagesController.cacheList.luisIntent))
                    {
                        intentName = "None";
                    }
                    else
                    {
                        intentName = MessagesController.cacheList.luisIntent;
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText += " INSERT INTO TBL_HISTORY_QUERY ";
				cmd.CommandText += " (USER_NUMBER, CUSTOMER_COMMENT_KR, CHATBOT_COMMENT_CODE, CHANNEL, RESPONSE_TIME, REG_DATE, ACTIVE_FLAG, APP_ID) ";
				cmd.CommandText += " VALUES ";
				cmd.CommandText += " (@userNumber, @customerCommentKR, @chatbotCommentCode, @channel, @responseTime, CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  DATEADD( HH, 9, GETDATE() ), 24), 0, @appID) ";

				cmd.Parameters.AddWithValue("@userNumber", userNumber);
				cmd.Parameters.AddWithValue("@customerCommentKR", MessagesController.queryStr);

                if(MessagesController.replyresult.Equals("S"))
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", "SEARCH");
                }
                else if (MessagesController.replyresult.Equals("D"))
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", "ERROR");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", intentName);
                }
                    
				cmd.Parameters.AddWithValue("@channel", channel);
				cmd.Parameters.AddWithValue("@responseTime", responseTime);
				cmd.Parameters.AddWithValue("@appID", appID);

				result = cmd.ExecuteNonQuery();
				Debug.WriteLine("result : " + result);
			}
			return result;
		}

        public int insertHistoryOrderNumber(string userNumber, string channel, int responseTime, String orderNumber, String commentCode)
        {
            int appID = 0;
            int result;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " INSERT INTO TBL_HISTORY_QUERY ";
                cmd.CommandText += " (USER_NUMBER, CUSTOMER_COMMENT_KR, CHATBOT_COMMENT_CODE, CHANNEL, RESPONSE_TIME, REG_DATE, ACTIVE_FLAG, APP_ID) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (@userNumber, @customerCommentKR, @chatbotCommentCode, @channel, @responseTime, CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  DATEADD( HH, 9, GETDATE() ), 24), 0, @appID) ";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@customerCommentKR", orderNumber);

                if (MessagesController.replyresult.Equals("S"))
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", "SEARCH");
                }
                else if (MessagesController.replyresult.Equals("D"))
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", "ERROR");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@chatbotCommentCode", commentCode);
                }

                cmd.Parameters.AddWithValue("@channel", channel);
                cmd.Parameters.AddWithValue("@responseTime", responseTime);
                cmd.Parameters.AddWithValue("@appID", appID);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        public int SelectUserQueryErrorMessageCheck(string userID, int appID)
		{
			SqlDataReader rdr = null;
			int result = 0;
			//userID = arg.Replace("'", "''");
			using (SqlConnection conn = new SqlConnection(connStr))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;

				cmd.CommandText += " SELECT TOP 1 A.CHATBOT_COMMENT_CODE ";
				cmd.CommandText += " FROM ( ";
				cmd.CommandText += " 	SELECT  ";
				cmd.CommandText += " 		SID, ";
				cmd.CommandText += " 		CASE  CHATBOT_COMMENT_CODE  ";
				cmd.CommandText += " 			WHEN 'SEARCH' THEN '1' ";
				cmd.CommandText += " 			WHEN 'ERROR' THEN '1' ";
				cmd.CommandText += " 			ELSE '0' ";
				cmd.CommandText += " 		END CHATBOT_COMMENT_CODE ";
				cmd.CommandText += " 	FROM TBL_HISTORY_QUERY WHERE USER_NUMBER = '" + userID + "' AND APP_ID = " + appID;
				cmd.CommandText += " ) A ";
				cmd.CommandText += " ORDER BY A.SID DESC ";

				rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

				while (rdr.Read())
				{
					result = Convert.ToInt32(rdr["CHATBOT_COMMENT_CODE"]);
				}
			}
			return result;
		}



        public string SelectArray(string entities)
        {
            SqlDataReader rdr = null;
            string newMsg = "";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "	SELECT ";
                cmd.CommandText += "        ISNULL(MAX(CASE WHEN POS = 1 THEN VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 2 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 3 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 4 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 5 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 6 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 7 THEN ',' + VAL1 END), '') ";
                cmd.CommandText += "        + ISNULL(MAX(CASE WHEN POS = 8 THEN ',' + VAL1 END), '') AS VAL ";
                cmd.CommandText += "        FROM ";
                cmd.CommandText += "            ( ";
                cmd.CommandText += "                SELECT VAL1, POS ";
                cmd.CommandText += "                FROM Split2(@entities, ',') ";
                cmd.CommandText += "            ) A                             ";

                cmd.Parameters.AddWithValue("@entities", entities);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    newMsg = rdr["VAL"] as string;
                }
            }
            return newMsg;
        }

        public String SearchCommonEntities
        {
            get
            {
                String query = Regex.Replace(MessagesController.queryStr, @"[^a-zA-Z0-9ㄱ-힣]", "", RegexOptions.Singleline).Replace(" ", "");
                SqlDataReader rdr = null;
                //List<RecommendConfirm> rc = new List<RecommendConfirm>();
                String entityarr = "";

                using (SqlConnection conn = new SqlConnection(connStr))
                {

                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    //cmd.CommandText += "SELECT  ENTITY_VALUE, ENTITY ";
                    //cmd.CommandText += "FROM    TBL_COMMON_ENTITY_DEFINE ";
                    //cmd.CommandText += "WHERE   CHARINDEX(ENTITY_VALUE,@kr_query) > 0";

                    cmd.CommandText += "SELECT RESULT AS ENTITIES FROM FN_ENTITY_ORDERBY_ADD(@kr_query) ";

                    cmd.Parameters.AddWithValue("@kr_query", query);

                    rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    //int count = 0;
                    try
                    {
                        while (rdr.Read())
                        {
                            entityarr += rdr["ENTITIES"];
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                }
                return entityarr;
            }
        }

        public String SelectUserHistoryComment(string userNumber, string chatbotCommentCode)
        {
            // userNumber, chatbotCommentCode
            string resultComment = "";
            SqlDataReader rdr = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += "	SELECT ";
                cmd.CommandText += "        TOP 1 ISNULL(CUSTOMER_COMMENT_KR, '') AS COMMENT ";
                cmd.CommandText += "        FROM TBL_HISTORY_QUERY";
                cmd.CommandText += " 	WHERE USER_NUMBER = '" + userNumber + "' AND CHATBOT_COMMENT_CODE = '" + chatbotCommentCode + "'";
                cmd.CommandText += "    ORDER BY SID DESC";
                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@chatbotCommentCode", chatbotCommentCode);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    resultComment = rdr["COMMENT"] as string;

                }

                //resultComment = newComment;
            }

            return resultComment;
        }
        /*
         * 처음 초기화 정보 저장
         * */
        public int initOrderList(string userNumber)
        {
            int result;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " INSERT INTO TBL_ORDER_LIST ";
                cmd.CommandText += " (USER_NUMBER, ITEM_1_CNT, ITEM_2_CNT, ITEM_3_CNT, ITEM_4_CNT, REG_DATE, USE_YN) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (@userNumber, 0, 0, 0, 0, CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  DATEADD( HH, 9, GETDATE() ), 24), @userYn) ";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@userYn", "N");
                
                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }
        /*
         * 상품 기본정보 저장
         * */
        public int insertProductBasicInfo(string userNumber, string item_name)
        {
            int result;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " INSERT INTO TBL_ORDER_LIST_DETAIL ";
                cmd.CommandText += " (USER_NUMBER, ITEM_NM, CART_IN, REG_DATE) ";
                cmd.CommandText += " VALUES ";
                cmd.CommandText += " (@userNumber, @item_name, 'N', CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  DATEADD( HH, 9, GETDATE() ), 24)) ";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@item_name", item_name);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        //저장된 상품의 SID 추출
        public String selectProductSID(string userNumber, string item_name)
        {
            SqlDataReader rdr = null;
            String result_string = "";
            
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TOP 1 CAST(SID AS VARCHAR(12)) AS SID ";
                cmd.CommandText += " FROM TBL_ORDER_LIST_DETAIL";
                cmd.CommandText += " WHERE USER_NUMBER = @userNumber AND ITEM_NM = @item_name";
                cmd.CommandText += " ORDER BY REG_DATE DESC";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@item_name", item_name);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    result_string = rdr["SID"] as string;
                }
            }
            return result_string;
        }


        /*
         * 상품 옵션 정보저장
         * */
        public int updateProductOption(string userNumber, string item_name, string option_style, string option_data, string sid)
        {
            int result;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " UPDATE TBL_ORDER_LIST_DETAIL SET ";
                if (option_style.Equals("cleanse"))
                {
                    cmd.CommandText += " O_CLEANSE = @option_data ";
                }else if (option_style.Equals("smoothie"))
                {
                    cmd.CommandText += " O_SMOOTHIE = @option_data ";
                }else if (option_style.Equals("delivery"))
                {
                    cmd.CommandText += " O_DELIVERY = @option_data ";
                }
                else if (option_style.Equals("orderAmount"))
                {
                    cmd.CommandText += " ORDER_AMOUNT = @option_data ";
                }
                else if (option_style.Equals("pickMeOption1"))
                {
                    cmd.CommandText += " O_PICK_1 = @option_data ";
                }
                else if (option_style.Equals("pickMeOption2"))
                {
                    cmd.CommandText += " O_PICK_2 = @option_data ";
                }
                else if (option_style.Equals("pickMeOption3"))
                {
                    cmd.CommandText += " O_PICK_3 = @option_data ";
                }
                else if (option_style.Equals("pickMeOption4"))
                {
                    cmd.CommandText += " O_PICK_4 = @option_data ";
                }
                else if (option_style.Equals("pickMeOption5"))
                {
                    cmd.CommandText += " O_PICK_5 = @option_data ";
                }
                else if (option_style.Equals("pickMeOption6"))
                {
                    cmd.CommandText += " O_PICK_6 = @option_data ";
                }
                else if (option_style.Equals("pickMeOption7"))
                {
                    cmd.CommandText += " O_PICK_7 = @option_data ";
                }
                else
                {
                    //error
                }
                
                cmd.CommandText += " WHERE ";
                cmd.CommandText += " SID = CONVERT(int, @sid) AND ";
                cmd.CommandText += " USER_NUMBER = @userNumber AND ";
                cmd.CommandText += " ITEM_NM = @item_name AND ";
                cmd.CommandText += " CART_IN = 'N' ";

                cmd.Parameters.AddWithValue("@sid", sid);
                cmd.Parameters.AddWithValue("@option_data", option_data);
                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@item_name", item_name);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        //주문내역 확인
        public List<CartList> selectOrderResult(string userNumber, string item_name)
        {
            SqlDataReader rdr = null;
            List<CartList> cartlist = new List<CartList>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT TOP 1 CAST(SID AS VARCHAR(12)) AS SID, USER_NUMBER, ITEM_NM, O_CLEANSE, O_SMOOTHIE, O_DELIVERY ";
                cmd.CommandText += " ,O_PICK_1, O_PICK_2, O_PICK_3, O_PICK_4, O_PICK_5, O_PICK_6, O_PICK_7, ORDER_AMOUNT, REG_DATE ";
                cmd.CommandText += " FROM TBL_ORDER_LIST_DETAIL";
                cmd.CommandText += " WHERE USER_NUMBER = @userNumber AND ITEM_NM = @item_name";
                cmd.CommandText += " ORDER BY REG_DATE DESC";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@item_name", item_name);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string sid = rdr["SID"] as string;
                    string user_number = rdr["USER_NUMBER"] as string;
                    string item_nm = rdr["ITEM_NM"] as string;
                    string o_cleanse = rdr["O_CLEANSE"] as string;
                    string o_smoothie = rdr["O_SMOOTHIE"] as string;
                    string o_delivery = rdr["O_DELIVERY"] as string;
                    string o_pick1 = rdr["O_PICK_1"] as string;
                    string o_pick2 = rdr["O_PICK_2"] as string;
                    string o_pick3 = rdr["O_PICK_3"] as string;
                    string o_pick4 = rdr["O_PICK_4"] as string;
                    string o_pick5 = rdr["O_PICK_5"] as string;
                    string o_pick6 = rdr["O_PICK_6"] as string;
                    string o_pick7 = rdr["O_PICK_7"] as string;
                    string order_amount = rdr["ORDER_AMOUNT"] as string;

                    CartList list = new CartList();

                    list.sid = sid;
                    list.userNumber = user_number;
                    list.itemNm = item_nm;
                    list.oCleanse = o_cleanse;
                    list.oSmoothie = o_smoothie;
                    list.oDelivery = o_delivery;
                    list.oPick1 = o_pick1;
                    list.oPick2 = o_pick2;
                    list.oPick3 = o_pick3;
                    list.oPick4 = o_pick4;
                    list.oPick5 = o_pick5;
                    list.oPick6 = o_pick6;
                    list.oPick7 = o_pick7;
                    list.orderAmount = order_amount;

                    DButil.HistoryLog("* userNumber : " + userNumber + " || item_name : " + item_name);
                    cartlist.Add(list);
                }
            }
            return cartlist;
        }

        /*
         * 상품 카트에 담기
         * */
        public int updateProductCart(string userNumber, string item_name)
        {
            int result;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText += " UPDATE TBL_ORDER_LIST_DETAIL SET CART_IN = 'Y'";
                cmd.CommandText += " ,CART_IN_DATE =  CONVERT(VARCHAR,  GETDATE(), 101) + ' ' + CONVERT(VARCHAR,  DATEADD( HH, 9, GETDATE() ), 24) ";
                cmd.CommandText += " WHERE SID IN ";
                cmd.CommandText += " ( SELECT TOP 1 SID FROM TBL_ORDER_LIST_DETAIL WHERE ";
                cmd.CommandText += " USER_NUMBER = @userNumber AND ";
                cmd.CommandText += " ITEM_NM = @item_name ORDER BY REG_DATE DESC)";

                cmd.Parameters.AddWithValue("@userNumber", userNumber);
                cmd.Parameters.AddWithValue("@item_name", item_name);

                result = cmd.ExecuteNonQuery();
                Debug.WriteLine("result : " + result);
            }
            return result;
        }

        //카트보기
        public String selectCartList(string userNumber)
        {
            SqlDataReader rdr = null;
            String result_string = "";
            
            String theBeginningTitle = "the beginning";
            String toAnotherLevelTitle = "to another level";
            String beautifulRevolutionTitle = "beautiful revolution";
            String pickMePTitle = "PICK ME";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                cmd.CommandText += " SELECT SID, USER_NUMBER, ITEM_NM, O_CLEANSE, O_SMOOTHIE, O_DELIVERY ";
                cmd.CommandText += " ,O_PICK_1, O_PICK_2, O_PICK_3, O_PICK_4, O_PICK_5, O_PICK_6, O_PICK_7, ORDER_AMOUNT, CONVERT(char(20), CART_IN_DATE, 120) as CART_IN_DATE, REG_DATE ";
                cmd.CommandText += " FROM TBL_ORDER_LIST_DETAIL";
                cmd.CommandText += " WHERE USER_NUMBER = @userNumber AND CART_IN = 'Y'";
 
                cmd.Parameters.AddWithValue("@userNumber", userNumber);

                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string user_number = rdr["USER_NUMBER"] as string;
                    string item_nm = rdr["ITEM_NM"] as string;
                    string o_cleanse = rdr["O_CLEANSE"] as string;
                    string o_smoothie = rdr["O_SMOOTHIE"] as string;
                    string o_delivery = rdr["O_DELIVERY"] as string;
                    string o_pick1 = rdr["O_PICK_1"] as string;
                    string o_pick2 = rdr["O_PICK_2"] as string;
                    string o_pick3 = rdr["O_PICK_3"] as string;
                    string o_pick4 = rdr["O_PICK_4"] as string;
                    string o_pick5 = rdr["O_PICK_5"] as string;
                    string o_pick6 = rdr["O_PICK_6"] as string;
                    string o_pick7 = rdr["O_PICK_7"] as string;
                    string order_amount = rdr["ORDER_AMOUNT"] as string;
                    string cart_in_date = rdr["CART_IN_DATE"] as string;

                    if (item_nm.Equals(theBeginningTitle))

                    {
                        result_string = result_string+ "["+ cart_in_date + "] 더비기닝" + "/" + o_cleanse + "/" + o_smoothie + "/" + o_delivery + "/" + order_amount + "개\n\n";
                    }

                    if (item_nm.Equals(toAnotherLevelTitle))
                    {
                        result_string = result_string + "[" + cart_in_date + "] 투어나더레벨" + "/" + o_cleanse + "/" + o_delivery + "/" + order_amount + "개\n\n";
                    }

                    if (item_nm.Equals(beautifulRevolutionTitle))
                    {
                        result_string = result_string + "[" + cart_in_date + "] 뷰티풀레볼루션" + "/" + o_cleanse + "/" + o_delivery + "/" + order_amount + "개\n\n";
                    }

                    if (item_nm.Equals(pickMePTitle))
                    {
                        result_string = result_string + "[" + cart_in_date + "] 픽미" + "/" + o_pick1 + "/" + o_pick2 + "/" + o_pick3 + "/" + o_pick4 + "/" + o_pick5 + "/" + o_pick6 + "/" + o_pick7 + "/" + o_delivery + "/" + order_amount + "개\n\n";
                    }
                }
            }
            return result_string;
        }

    }
}